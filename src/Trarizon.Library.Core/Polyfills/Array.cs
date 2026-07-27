namespace Trarizon.Library;

#if NETSTANDARD

internal static partial class Polyfills
{
    extension(Array)
    {
        public static int MaxLength => 0x7FFFFFC7;
    }
}

#endif
