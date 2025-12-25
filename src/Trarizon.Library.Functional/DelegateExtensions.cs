namespace Trarizon.Library.Functional;

internal static class DelegateExtensions
{
    public static Func<T, TResult> Compose<T, TMid, TResult>(this Func<T, TMid> f, Func<TMid, TResult> g)
        => t => g(f(t));
}
