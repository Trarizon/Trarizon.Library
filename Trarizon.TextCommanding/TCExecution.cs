using Trarizon.TextCommanding.Definition;
using Trarizon.TextCommanding.Input;

namespace Trarizon.TextCommanding;
public static class TCExecution
{
    internal const char Quote = '"';

    [Obsolete("Not implemented", true)]
    public static TCExecutionResult Execute<TContext>(IEnumerable<ITextCommand<TContext>> commands, TContext context)
    {
        foreach (var command in commands) {
            var res = Execute(command, context);
            if (res == TCExecutionResult.Success)
                return res;
        }
        return TCExecutionResult.Failure;
    }

    [Obsolete("Not implemented", true)]
    public static TCExecutionResult Execute<TContext>(ITextCommand<TContext> command, TContext context)
    {
        throw new NotImplementedException();
    }

    public static T ParseArgs<T>(string input, ParseArgsSettings? settings = null) 
        => ParseArgs<T>(input.AsMemory(), settings ?? ParseArgsSettings.Shared);

    public static T ParseArgs<T>(ReadOnlyMemory<char> input, ParseArgsSettings? settings = null)
        => ParseArgs<T, StringRawInput>(new StringRawInput(input), settings ?? ParseArgsSettings.Shared);

    private static T ParseArgs<T, TRawInput>(TRawInput args, ParseArgsSettings settings) where TRawInput : IRawInput
    {
        TextCommand command = TextCommand.Get<T>();
        RawArguments rawArguments = RawArguments.Parse(args, command, settings);
        return (T)command.Construct(rawArguments);
    }
}
