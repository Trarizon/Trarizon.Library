// See https://aka.ms/new-console-template for more information
using Trarizon.Library.CodeAnalysis.Diagnostics;
using Trarizon.Library.CodeAnalysis.Generation;

Console.WriteLine("Hello, World!");

//var invk = S.Instance.T();

namespace A.BBd
{
    partial class B
    {
        [GenDisposing(Explicitly = true)]
        public ref partial struct Dis : IDisposable
        {
            private Stream stream;
            private IDisposable disp;

            public Stream Stream { get; }

            public Stream Stream2 { get => field ??= new FileStream("", FileMode.Create); }

            public Stream Stream3 => stream;

            public void Dispose()
            {
                this.DisposeDisposableMembers();
            }
        }

        public partial class C
        {
            [Singleton(InstanceAccessibility = SingletonAccessibility.Internal,
                InstancePropertyName = "Shared",
                SingletonProviderName = "")]
            public partial class S
            {
                [FriendAccess(typeof(F))]
                public string? T() => ToString();

                public string? D => base.ToString().Replace('c', 'd');

                public event Action? Ac;
            }
        }
    }
}

[ExternalSealed]
public class Extern
{

}

class F
{
    public static string? Create() => A.BBd.B.C.S.Shared.T();
}

ref struct D
{
    public void Dispose() { }
}