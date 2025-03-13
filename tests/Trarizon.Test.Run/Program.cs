#pragma warning disable TRAEXP

using BenchmarkDotNet.Running;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Deenote.Library.Collections.StackAlloc;
using System.Collections;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Trarizon.Library;
using Trarizon.Library.Buffers.Pooling;
using Trarizon.Library.CodeAnalysis;
using Trarizon.Library.CodeAnalysis.MemberAccess;
using Trarizon.Library.CodeGeneration;
using Trarizon.Library.Collections;
using Trarizon.Library.Collections.AllocOpt;
using Trarizon.Library.Collections.Generic;
using Trarizon.Library.Numerics;
using Trarizon.Library.RunTest.Examples;
using Trarizon.Library.Text;
using Trarizon.Library.Text.Json;
using Trarizon.Library.Threading;
using Trarizon.Library.Wrappers;
using Trarizon.Test.Run;

int[] arr = { 1, 2, 3, 4, 5, 6, 7 };

foreach (var item in new ReadOnlyReversedSpan<int>(arr)) {
    item.Print();
}


#nullable enable

//using CommunityToolkit.HighPerformance;
//using System;

namespace Deenote.Library.Collections.StackAlloc
{
    public readonly ref struct ReadOnlyReversedSpan<T>
    {
        private readonly ReadOnlySpan<T> _span;

        public ReadOnlyReversedSpan(ReadOnlySpan<T> span)
        {
            _span = span;
        }

        public Enumerator GetEnumerator() => new Enumerator(_span);

        public ref struct Enumerator
        {
            private ReadOnlySpan<T> _span;

            public Enumerator(ReadOnlySpan<T> span)
            {
                _span = span;
            }

            public readonly ref readonly T Current => ref _span.DangerousGetReferenceAt(_span.Length);

            public bool MoveNext()
            {
                if (_span.IsEmpty)
                    return false;
                _span = _span[..^1];
                return true;
            }
        }
    }
}
