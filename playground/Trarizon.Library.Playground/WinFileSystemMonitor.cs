using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trarizon.Library.Playground;
internal class WinFileSystemMonitor
{
    private void Run()
    {
        using var fsw = new FileSystemWatcher(@"D:\Pictures")
        {
            Path = @"D:\Pictures",
            NotifyFilter = NotifyFilters.Attributes
                         | NotifyFilters.CreationTime
                         | NotifyFilters.DirectoryName
                         | NotifyFilters.FileName
                         | NotifyFilters.LastAccess
                         | NotifyFilters.LastWrite
                         | NotifyFilters.Security
                         | NotifyFilters.Size,
            EnableRaisingEvents = true,
            IncludeSubdirectories = true,
        };

        string? prevDel = null;

        fsw.Changed += (s, e) =>
        {
            (e.ChangeType, e.FullPath).Print();
        };

        fsw.Created += (s, e) =>
        {
            if (prevDel != null
                && Path.GetFileName(e.FullPath) == Path.GetFileName(prevDel)) {
                ("Move", e.FullPath).Print();
            }
            else {
                (e.ChangeType, e.FullPath).Print();
            }
            prevDel = null;
        };

        fsw.Deleted += (s, e) =>
        {
            prevDel = e.FullPath;
            (e.ChangeType, e.FullPath).Print();
        };

        fsw.Renamed += (s, e) =>
        {
            (e.ChangeType, e.FullPath).Print();
        };

    }
}
