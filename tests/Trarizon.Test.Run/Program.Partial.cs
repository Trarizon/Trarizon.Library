using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Trarizon.Library.CodeTemplating;
using Trarizon.Library.CodeTemplating.TaggedUnion;
using Trarizon.Library.Collections.Helpers;

//[Singleton]
public sealed partial class Program
{
    public enum ALargeUnionKind
    {
        [TagVariant(typeof(int*))]
        Z = 1,
        [TagVariant<int, string>()]
        A,
        [TagVariant<long>()]
        B,
    }

    [TaggedUnion(nameof(Defi))]
    partial struct AL<T, T2>
    {
        (object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object) _vals;
        static partial void Defi(
            ValueTuple A,
            int B,
            (T A, int B) C
            );

        void A()
        {
            switch (ArrayInts()) {
                case { IsFixedSize: false, Length: var l }:
                    break;
                case { IsFixedSize: true, Length: var l }:

                default:
                    break;
            }
            var rf = __makeref(_vals);
        }
    }

    ref int Do()
    {
        var variantFields = EnumerateValues<(Type Type, string Identifier)>(null!);
        var fields = EnumerateValues<FieldInfo>(null!);

        var identifierDict = variantFields.ToDictionary();
        var groupedf = fields
            .GroupBy(f => f.DeclaringType!)
            .SelectMany(g => g.Select((f, i) => $"{identifierDict[g.Key]}_{i}"));



        var en = new Span<int>().GetEnumerator();
        var union = new ALargeUnionType();
        var a = union.A;

        switch (union) {
            case { Kind: ALargeUnionKind.A, A: (var i, var str) }:

            default:
                break;
        }
        return ref Unsafe.NullRef<int>();
    }

    async Task<int> DoAsync()
    {
        var union = ALargeUnionType.CreateA(1, "");
        switch (union) {
            case { Kind: ALargeUnionKind.Z }:
                return 1;
            case { Kind: ALargeUnionKind.A, A: (var i, var str) }:
                return 2;
            case { Kind: ALargeUnionKind.B, B.Item1: var i }:
                return 3;
            default:
                break;
        }
        await Task.Delay(1000);
        return 3;
    }

    public struct ALargeUnionType
    {
        private object _item;
        private readonly ALargeUnionKind _kind;
        private __UnmanagedUnion _unmanaged;

        public readonly ALargeUnionKind Kind => _kind;

        [UnscopedRef] public AVariant A => new(ref this);
        [UnscopedRef] public BVariant B => new(ref this);

#pragma warning disable CS8618
        private ALargeUnionType(ALargeUnionKind kind) => _kind = kind;
#pragma warning restore CS8618

        public static ALargeUnionType CreateA(int val, string str) => new(ALargeUnionKind.A) {
            _item = str,
            // _unmanaged = new() { _0 = val, },
        };

        public static ALargeUnionType CreateB(long Item1) => new(ALargeUnionKind.B) {
            // _unmanaged = new() { _1 = Item1 },
        };

        public bool TryGetA(out int item1, out string item2)
        {
            if (_kind is ALargeUnionKind.A) {
                A.Deconstruct(out item1, out item2);
                return true;
            }
            else {
                item1 = default!;
                item2 = default!;
                return false;
            }
        }

        public bool TryGetB(out long item1)
        {
            if (_kind is ALargeUnionKind.B) {
                item1 = B.Item1;
                return true;
            }
            else {
                item1 = default;
                return false;
            }
        }


        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        private struct __UnmanagedUnion
        {
            // 按variant设置union
            // tuple item经过排序与去重
            // 如果有包含关系也去重？
            [FieldOffset(0)] public (int, long) _0;
            [FieldOffset(0)] public (long, long) _1;
        }


        public ref struct AVariant(ref ALargeUnionType union)
        {
            private ref ALargeUnionType _union = ref union;

            public ref int Item1 => ref Unsafe.As<__UnmanagedUnion, int>(ref _union._unmanaged);
            public ref string Item2 => ref Unsafe.As<object, string>(ref _union._item);

            public void Deconstruct(out int Item1, out string Item2)
                => (Item1, Item2) = (this.Item1, this.Item2);
        }

        public ref struct BVariant(ref ALargeUnionType union)
        {
            private ref ALargeUnionType _union = ref union;

            public ref long Item1 => ref Unsafe.As<__UnmanagedUnion, long>(ref _union._unmanaged);
        }
    }

    public abstract class ALargeUnionClass
    {
        public ALargeUnionKind Kind => this switch {
            Z => ALargeUnionKind.Z,
            A => ALargeUnionKind.A,
            _ => throw new InvalidOperationException()
        };

        public sealed class Z : ALargeUnionClass;
        public sealed class A : ALargeUnionClass
        {
            public int Item1;
            public string Item2;

            public A(int Item1, string Item2)
                => (this.Item1, this.Item2) = (Item1, Item2);

            public void Deconstruct(out int Item1, out string Item2) => (Item1, Item2) = (this.Item1, this.Item2);
        }
    }

    public abstract record ALargeUnionRecord
    {
        public ALargeUnionKind Kind => this switch {
            Z => ALargeUnionKind.Z,
            A => ALargeUnionKind.A,
            _ => throw new InvalidOperationException()
        };

        public sealed record Z : ALargeUnionRecord;
        public sealed record A(int Item1, string Item2) : ALargeUnionRecord;
    }

    public ref struct ALargeUnionRefStruct
    {
        private readonly ALargeUnionKind _kind;
        private ref byte _0;
        private ref byte _1;

        public readonly ALargeUnionKind Kind => _kind;

        private ALargeUnionRefStruct(ALargeUnionKind kind) => _kind = kind;

        [UnscopedRef] public AVariant A => new(ref this);

        public static ALargeUnionRefStruct CreateA(ref int Item1, ref string Item2) => new(ALargeUnionKind.A) {
            _0 = ref Unsafe.As<int, byte>(ref Item1),
            _1 = ref Unsafe.As<string, byte>(ref Item2),
        };

        public ref struct AVariant(ref ALargeUnionRefStruct union)
        {
            public ref int Item1 = ref Unsafe.As<byte, int>(ref union._0);
            public ref string Item2 = ref Unsafe.As<byte, string>(ref union._1);
        }
    }

}
