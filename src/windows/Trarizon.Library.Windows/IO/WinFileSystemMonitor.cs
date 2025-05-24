using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanara.PInvoke;

namespace Trarizon.Library.Windows.IO;
internal class WinFileSystemMonitor : IDisposable
{
    private string _monitorPath;
    private SafeHANDLE _dirHandle;

    private bool _isActive;
    private bool _disposedValue;

    public WinFileSystemMonitor(string path)
    {
        _monitorPath = path;
    }

    public void SetActive(bool active)
    {
        if (_isActive == active)
            return;

        if (_isActive) {
            _dirHandle = Kernel32.CreateFile(
                _monitorPath,
                Kernel32.FileAccess.FILE_LIST_DIRECTORY,
                FILE_SHARE.FILE_SHARE_READ | FILE_SHARE.FILE_SHARE_WRITE | FILE_SHARE.FILE_SHARE_DELETE,
                null,
                Kernel32.CreationOption.OPEN_EXISTING,
                FileFlagsAndAttributes.FILE_FLAG_BACKUP_SEMANTICS | FileFlagsAndAttributes.FILE_FLAG_OVERLAPPED,
                nint.Zero);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue) {
            if (disposing) {
                // TODO: 释放托管状态(托管对象)
            }

            _dirHandle.Dispose();
            _dirHandle = null;
            // TODO: 释放未托管的资源(未托管的对象)并重写终结器
            // TODO: 将大型字段设置为 null
            _disposedValue = true;
        }
    }

    ~WinFileSystemMonitor()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

