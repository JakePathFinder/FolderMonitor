using FileListener.Constants;

namespace FileListener.Model
{
    public delegate void ChangeHandler(FileSystemEventArgs e);

    public class FileSystemWatcherWrapper : IDisposable
    {
        public string FolderName { get; init; }
        private readonly FileSystemWatcher _watcher;
        private readonly FileSystemEventHandler _eventHandler;
        private readonly RenamedEventHandler _renameEventHandler;
        private readonly ErrorEventHandler _errorHandler;

        public FileSystemWatcherWrapper(string folderName, int bufferSize, ChangeHandler changeHandler, ErrorEventHandler errorHandler)
        {
            var watcher = new FileSystemWatcher
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = true,
                InternalBufferSize = bufferSize,
                NotifyFilter = Const.NotifyFilters,
                Path = folderName
            };

            FolderName = folderName;
            VerifyFolder();
            _eventHandler = (object o, FileSystemEventArgs e) => changeHandler(e);
            _renameEventHandler = (object o, RenamedEventArgs e) => changeHandler(e);
            _errorHandler = errorHandler;
            _watcher = new FileSystemWatcher { Path = folderName};
        }

        public void StartWatch()
        {
            _watcher.Changed += _eventHandler;
            _watcher.Created += _eventHandler;
            _watcher.Deleted += _eventHandler;
            _watcher.Renamed += _renameEventHandler;
            _watcher.Error += _errorHandler;
            _watcher.EnableRaisingEvents = true;
        }

        public void StopWatch()
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Changed -= _eventHandler;
            _watcher.Created -= _eventHandler;
            _watcher.Deleted -= _eventHandler;
            _watcher.Renamed -= _renameEventHandler;
            _watcher.Error += _errorHandler;
        }

        private void VerifyFolder()
        {
            if (string.IsNullOrEmpty(FolderName))
            {
                throw new ArgumentNullException(FolderName);
            }

            if (!Directory.Exists(FolderName))
            {
                throw new ArgumentException($"Inaccessible folder {FolderName}");
            }
        }

        public void Dispose()
        {
            StopWatch();
            _watcher.Dispose();
        }
    }
}
