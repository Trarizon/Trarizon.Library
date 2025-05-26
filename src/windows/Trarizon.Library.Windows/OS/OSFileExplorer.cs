using System.Diagnostics;

namespace Trarizon.Library.Windows.OS;
public static partial class OSFileExplorer
{
    private const string ExplorerExe = "explorer.exe";

    public static void OpenDirectory(string directoryPath)
    {
        Process.Start(ExplorerExe, Path.GetFullPath(directoryPath));
    }

    public static bool TryOpenDirectory(string directoryPath)
    {
        if (Directory.Exists(directoryPath)) {
            OpenDirectory(directoryPath);
            return true;
        }
        return false;
    }

    public static void OpenSelectPath(string selectPath)
    {
        Process.Start(ExplorerExe, $"/select,{Path.GetFullPath(selectPath)}");
    }
}
