using Trarizon.Library.Functional;

namespace Trarizon.Test.Run.Examples;

internal class Functional_Chains
{
    public void Optionals()
    {
        _ = Optional.Of("str")
            .Select(i => i.ToString())  // val -> val
            .Select(o => o.AsSpan())    // val -> ref
            .Select(o => o[..1])        // ref -> ref
            .Select(x => x.ToString()); // ref -> val

        _ = Optional.Of("str")
            .Bind(x => Optional.Of(x.ToString()))  // val -> val
            .Bind(x => Optional.Of(x.AsSpan()))    // val -> ref
            .Bind(x => Optional.Of(x[..1]))        // ref -> ref
            .Bind(x => Optional.Of(x.ToString())); // ref -> val

        _ = Optional.Of("str".AsSpan())
            .SelectMany(x => Optional.Of(x.ToString()), (x, y) => $"{x}{y}")             // ref - val - val
            .SelectMany(x => Optional.Of(x.AsSpan()), (x, y) => $"{x}{y}")               // val - ref - val
            .SelectMany(x => Optional.Of(x.ToString()), (x, y) => $"{x}{y}".AsSpan())    // val - val - ref
            .SelectMany(x => Optional.Of(x.ToString()), (x, y) => $"{x}{y}".AsSpan())    // ref - val - ref
            .SelectMany(x => Optional.Of(x[..1]), (x, y) => $"{x}{y}".ToString())        // ref - ref - val
            .SelectMany(x => Optional.Of(x.AsSpan()), (x, y) => $"{x}{y}".AsSpan());     // val - ref - ref

        _ = Optional.Of("str".AsSpan()).Zip(Optional.Of("ing"), (x, y) => $"{x}{y}");    // ref + val = val
        _ = Optional.Of("str").Zip(Optional.Of("ing".AsSpan()), (x, y) => $"{x}{y}");    // val + ref = val
        _ = Optional.Of("str").Zip(Optional.Of("ing"), (x, y) => $"{x}{y}".AsSpan());    // val + val = ref
        _ = Optional.Of("str".AsSpan()).Zip(Optional.Of("ing"), (x, y) => $"{x}{y}".AsSpan()); // ref + val = ref
        _ = Optional.Of("str").Zip(Optional.Of("ing".AsSpan()), (x, y) => $"{x}{y}".AsSpan()); // val + ref = ref
        _ = Optional.Of("str".AsSpan()).Zip(Optional.Of("ing".AsSpan()), (x, y) => $"{x}{y}"); // ref + ref = val
        _ = Optional.Of("str".AsSpan()).Zip(Optional.Of("ing".AsSpan()), (x, y) => $"{x}{y}"); // ref + ref = ref

    }

    public void Results()
    {
        _ = Result.Success("str").Build<int>()
            .Select(i => i.ToString())  // val -> val
            .Select(o => o.AsSpan())    // val -> ref
            .Select(o => o[..1])        // ref -> ref
            .Select(x => x.ToString()); // ref -> val

        _ = Result.Success("str".AsSpan());
    }
}
