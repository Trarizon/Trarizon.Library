// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Trarizon.Library;
using Trarizon.Library.CodeAnalysis;
using Trarizon.Library.Collections.AllocOpt;
using Trarizon.Library.Collections.Helpers;
using Trarizon.Library.RunTest.Examples;
using Trarizon.Library.Wrappers;
using Trarizon.Test.Run;

AllocOptQueue<int> q = new(6);
q.EnqueueRange([1, 2, 3, 4, 5]);
q.Dequeue(2);
Print(); // _ _ 3 4 5 _ -1.1
q.Dequeue(114);
Print(); // _ _ _ _ _ _ -1.2
q.EnqueueRange([1, 2, 3, 4, 5]); // 1 2 3 4 5 _
q.Dequeue(3);                    // _ _ _ 4 5 _
q.EnqueueRange([6, 7, 8]);       // 7 8 _ 4 5 6
var cpy = q;
cpy.Dequeue(2);
Print(cpy); // 7 8 _ _ _ 6 -2.1
cpy = q;
cpy.Dequeue(4);
Print(cpy); // _ 8 _ _ _ _ -2.2.1
cpy = q;
cpy.Dequeue(114);
Print(cpy); // _ _ _ _ _ _ -2.2.2


Console.WriteLine();

void Print(AllocOptQueue<int>? queue = null)
{
    queue ??= q;
    queue.Value.Print();
    queue.Value.GetUnderlyingArray().Print();
    Console.WriteLine();
}
