using System.Diagnostics.CodeAnalysis;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Trarizon.Library.Windows;

internal static class TraWinFileSystem
{
    // PInvoke: Shell32.dll
    public static nint GetFileFirstIconHandle(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found.", filePath);

        Span<HICON> buffer = stackalloc HICON[1];
        var count = PInvoke.ExtractIconEx(filePath, 0, buffer);
        if (count < 1)
            return nint.Zero;
        return buffer[0];
    }

    // COM: Windows Script Host Ojbect Model
    [field: MaybeNull]
    private static IWshRuntimeLibrary.WshShell WshShell => field ??= new();
    public static LnkFileTarget GetLnkFileTarget(string lnkFilePath)
    {
        IWshRuntimeLibrary.IWshShortcut shortcut = WshShell.CreateShortcut(lnkFilePath);
        return new LnkFileTarget(shortcut);
    }

    public readonly struct LnkFileTarget
    {
        private readonly IWshRuntimeLibrary.IWshShortcut _shortcut;

        public string ExePath => _shortcut.TargetPath;
        public string Arguments => _shortcut.Arguments;

        internal LnkFileTarget(IWshRuntimeLibrary.IWshShortcut shortcut)
        {
            _shortcut = shortcut;
        }
    }
}
