using Trarizon.Library.Wrappers;
using Trarizon.Yieliception;
using Trarizon.Yieliception.Components;
using Trarizon.Yieliception.Yieliceptors;

namespace Trarizon.Library.RunTest.Examples;
internal class YieliceptionExample() : ExampleBase()
{
    public void Run()
    {
        var iterator = GetEnumerator().ToYieliceptable();

        while (true) {
            string args = Console.ReadLine()!;

            iterator.Next(args);
            if (iterator.IsEnded)
                break;
        }
    }

    public void RunComponent()
    {
        //                                             Timer will call method from another thread, use threadSafe
        var iterator = GetComponentEnumerator().ToYieliceptable(threadSafe: true, new YieliceptionTimer());
        // This will called when iteration ended
        iterator.End += _ => Console.WriteLine("Iteration ended.");

        while (true) {
            string args = Console.ReadLine()!;

            // Timer may end the iteration ahead-of-time
            if (iterator.IsEnded) {
                Console.WriteLine("Already ended");
                break;
            }
            iterator.Next(args);
            if (iterator.IsEnded) {
                break;
            }
        }
    }

    public void RunCommunication()
    {
        var iterator = GetCommunicationEnumerator();

        // Enter to start
        string args = Console.ReadLine()!;

        while (iterator.SendAndNext(args, out var current)) {
            Console.WriteLine($"Yield number {current}, yield is {(iterator.Current is null ? "null" : "not null")}.");
            // current is default when yield null, you can use iterator.Current to check if the method is really return null
            args = Console.ReadLine()!;
        }

        // Equals to
#pragma warning disable CS8321
        static void SameAs()
        {
            var iterator = GetCommunicationEnumerator().ToYieliceptable();

            string args = Console.ReadLine()!;
            while (iterator.Next(args) == YieliceptionResult.Moved) {
                var current = (ValueDeliverer<int, string>)iterator.CurrentYieliceptor!;
                Console.WriteLine($"Yield number {current?.YieldValue ?? default}, yield is {(iterator.CurrentYieliceptor is null ? "null" : "not null")}.");
            }
        }
#pragma warning restore CS8321
    }

    private static IEnumerator<IYieliceptor<string>?> GetEnumerator()
    {
        // Validation
        {
            Console.WriteLine("Enter a string start with 1 to continue.");

            // Pass a Func<T, Optional<T>> as validator
            // [Optional] Pass a Action<VI<,>, T> as callback when rejected.
            // if validator returns true, enumerator move next, else onRejected invoked.
            // You can reuse it.
            ValidationYieliceptor<string, int> yieliceptor = new(
                validator: args => args.StartsWith('1') ? Optional.Of(args.Length) : default,
                onRejected: (_, args) => Console.WriteLine($"Value {args} is not start with 1."));
            yield return yieliceptor;

            Console.WriteLine($"Previous value length is {yieliceptor.Result}");

            // -----

            Console.WriteLine("Enter a string start with 9 to continue.");

            // Most yieliceptors public some WithXXX() methods for reusing
            yield return yieliceptor
                .WithValidator(args => args.StartsWith('9') ? Optional.Of(args.Length) : default)
                .WithRejectionHandler((_, args) => Console.WriteLine($"Reused! But value {args} is not start with 9."));

            Console.WriteLine($"Previous value length is {yieliceptor.Result}");

            // -----

            Console.WriteLine("Enter 2 to continue");

            // VI<T> is VI<T,T>
            // BTW most yieliceptors has a ctor like new T(out T, params...), for convenience.
            // thus you can call ctor in yield return sentence.
            yield return new ValidationYieliceptor<string>(out var yieliceptor1,
                args => args == "2");

            Console.WriteLine($"Previous input is {yieliceptor1.Result}");

            // -----

            Console.WriteLine("Enter anything to continue");

            // ConditionYieliceptor provides a non-parameter validator
            yield return new ConditionYieliceptor(() => true);
            Console.WriteLine("Continue.");


        }

        // ValueDelivering
        {
            Console.WriteLine("Enter anything to continue");

            // These validation always returns true,
            // Use then to pass args
            // You can reuse it.
            yield return new ValueDeliveryYieliceptor<string>(out var yieliceptor);

            Console.WriteLine($"Previous input is {yieliceptor.Value}");
        }

        // Null
        {
            Console.WriteLine("Enter anything to contine.");
            // Use null to pause once.
            yield return null;

            Console.WriteLine("Enter anything for 3 times to continue");
            for (int i = 0; i < 3; i++)
                yield return null;
        }
    }

    private static IEnumerator<IYieliceptor<string>?> GetComponentEnumerator()
    {
        // YieliceptionTimer
        {
            // TimerYieliceptor is a simple impl for YieliceptionTimer
            yield return new TimerYieliceptor(out var timerYieliceptor);

            // Use ITimerYieliceptor.IsTimeout to check
            if (timerYieliceptor.IsTimeout)
                Console.WriteLine("Timeout, but continue.");
            else
                Console.WriteLine("Continue.");

            // ---------------------

            string require = "0";
            Console.WriteLine("""
                Enter 0 to continue. Time limit 2s.
                Or Enter anything else to reset timer.
                """);

            // ValidationYieliceptor can use timer
            yield return new ValidationYieliceptor<string>(out var yieliceptor, args => args == require) {
                ResetTimerOnRejected = true, // If true, timer will reset onf rejected.
                TimeLimitMillisecond = 2000, // Timeout limit
            };

            if (yieliceptor.IsTimeout)
                Console.WriteLine("Timeout");
            else
                Console.WriteLine($"Previous input is {yieliceptor.Result}");

            Console.WriteLine("""
                Enter 1 to continue. Time limit 3s,
                Timer will not reset if you enter other things.
                """);

            require = "1";
            yieliceptor.TimeLimitMillisecond = 3000;
            yieliceptor.ResetTimerOnRejected = false;
            yield return yieliceptor;

            if (yieliceptor.IsTimeout) {
                Console.WriteLine("Timeout, process ends");
                yield break;
            }

            Console.WriteLine("Enter 2 to continue");

            require = "2";
            // Stop timer
            yieliceptor.TimeLimitMillisecond = Timeout.Infinite;
            yield return yieliceptor;

            Console.WriteLine("End");
        }
    }

    private static IEnumerator<ValueDeliverer<int, string>> GetCommunicationEnumerator()
    {
        // VTY<,> keeps an object, thus caller can use this value,
        // and when call next, it keeps the args transferred from caller
        yield return new ValueDeliverer<int, string>(out var yieliceptor, -1);

        Console.WriteLine($"You just entered '{yieliceptor.ReceivedValue}'");

        for (int i = 0; i < 3; i++) {
            // Reuse is allowed, but if user called IEnumerator.MoveNext(), ReceivedValue will be obsoleted value
            yield return yieliceptor.WithYield(i);

            Console.WriteLine($"You just entered '{yieliceptor.ReceivedValue}'");
            Console.WriteLine();
        }

        // Yield null is legal but not recommanded
        // If iterator won't yield null, caller can emit the judgement if (iterator.Current is null)
        yield return null!;
    }
}
