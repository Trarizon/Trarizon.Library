using Microsoft.Win32.SafeHandles;
using Windows.Win32;
using Windows.Win32.Storage.FileSystem;

namespace Trarizon.Library.Windows.IO;

internal class WinFileSystemMonitor : IDisposable
{
    private string _monitorPath;
    private SafeFileHandle? _dirHandle;

    private bool _isActive;
    private bool _disposedValue;

    public WinFileSystemMonitor(string path)
    {
        _monitorPath = path;
        _dirHandle = null;
    }

    public void SetActive(bool active)
    {
        if (_isActive == active)
            return;

        if (_isActive) {
            _dirHandle = PInvoke.CreateFile(
                _monitorPath,
                1, // FILE_LIST_DIRECTORY
                FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE | FILE_SHARE_MODE.FILE_SHARE_DELETE,
                null,
                FILE_CREATION_DISPOSITION.OPEN_EXISTING,
                FILE_FLAGS_AND_ATTRIBUTES.FILE_FLAG_BACKUP_SEMANTICS | FILE_FLAGS_AND_ATTRIBUTES.FILE_FLAG_OVERLAPPED);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue) {
            if (disposing) {
                // TODO: 释放托管状态(托管对象)
            }

            _dirHandle?.Dispose();
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

