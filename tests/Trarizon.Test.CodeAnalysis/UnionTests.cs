using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Trarizon.Library.Functional.Unions.Attributes;

namespace Trarizon.Test.CodeAnalysis;

internal class UnionTests
{
    public static void Test()
    {
        U u = 15L;

        u.TryAs<long>(out var s);

    }
}

[Union(typeof(string), typeof(int), typeof(long), typeof(DateTime), typeof(CancellationToken), typeof(List<int>), typeof(ValueTuple<int, string>))]
public readonly partial struct U
{

}
