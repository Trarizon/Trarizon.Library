using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Trarizon.Library.CodeTemplating;
using Trarizon.Library.CodeTemplating.TaggedUnion;

//[Singleton]
internal sealed partial class Program
{
    enum ALargeUnionKind
    {
        [TagVariant<int, string>()]
        A
    }

    ref int Do()
    {
        var en = new Span<int>().GetEnumerator();
        var union = new ALargeUnionType();
        var a = union.A;

        switch (union) {
            case { Kind: ALargeUnionKind.A, A: (var i, var str) }:

            default:
                break;
        }
    }

    struct ALargeUnionType
    {
        private object _item;
        private ALargeUnionKind _kind;
        private int _item1;

        public readonly ALargeUnionKind Kind => _kind;

        [UnscopedRef]
        public AVariant A => new(ref this);

        public bool TryGetA(out int item1, out string item2)
        {
            if (_kind is ALargeUnionKind.A) {
                A.Deconstruct(out item1, out item2);
            }
        }

        [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref struct AVariant(ref ALargeUnionType union)
        {
            private ref ALargeUnionType _union = ref union;

            public ref int Item1 => ref _union._item1;
            public ref string Item2 => ref Unsafe.As<object, string>(ref _union._item);

            public void Deconstruct(out int item1, out string item2)
            {
                item1 = Item1;
                item2 = Item2;
            }
        }
    }
}
