// See https://aka.ms/new-console-template for more information
using Trarizon.Library.CodeAnalysis.Diagnostics;
using Trarizon.Library.CodeAnalysis.Generation;

Console.WriteLine("Hello, World!");

//var invk = S.Instance.T();

namespace A.BBd
{
    partial class B
    {
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
