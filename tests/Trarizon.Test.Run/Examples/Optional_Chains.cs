using Trarizon.Library.Functional;

namespace Trarizon.Test.Run.Examples;

internal class Optional_Chains
{
    public void M()
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
            .Bind(x => Optional.Of(x.ToString()), (x, y) => $"{x}{y}")             // ref - val - val
            .Bind(x => Optional.Of(x.AsSpan()), (x, y) => $"{x}{y}")               // val - ref - val
            .Bind(x => Optional.Of(x.ToString()), (x, y) => $"{x}{y}".AsSpan())    // val - val - ref
            .Bind(x => Optional.Of(x.ToString()), (x, y) => $"{x}{y}".AsSpan())    // ref - val - ref
            .Bind(x => Optional.Of(x[..1]), (x, y) => $"{x}{y}".ToString())        // ref - ref - val
            .Bind(x => Optional.Of(x.AsSpan()), (x, y) => $"{x}{y}".AsSpan());     // val - ref - ref
    }
}
