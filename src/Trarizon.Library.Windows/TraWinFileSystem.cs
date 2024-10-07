using System.Buffers;
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
    private static IWshRuntimeLibrary.WshShell? _wshShell;
    public static LnkFileTarget GetLnkFileTarget(string lnkFilePath)
    {
        _wshShell ??= new();
        IWshRuntimeLibrary.IWshShortcut shortcut = _wshShell.CreateShortcut(lnkFilePath);
        return new LnkFileTarget(shortcut.TargetPath, shortcut.Arguments);
    }

    public readonly record struct LnkFileTarget(string ExePath, string? Arguments);
}
