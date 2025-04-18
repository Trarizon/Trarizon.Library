using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.Buffers;
using Vanara.PInvoke;

namespace Trarizon.Library.Windows;
internal static class TraWinFileSystem
{
    // PInvoke: Shell32.dll
    public static nint GetFileFirstIconHandle(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found.", filePath);

        using var p_arr = ArrayPool<HICON>.Shared.Rent(1, out var arr);
        var count = Shell32.ExtractIconEx(filePath, 0, phiconLarge: arr, nIcons: 1);
        if (count < 1)
            return nint.Zero;
        return arr[0].DangerousGetHandle();
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
