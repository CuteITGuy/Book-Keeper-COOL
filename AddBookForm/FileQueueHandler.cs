using System.Collections.Generic;
using System.IO;


namespace AddBookForm
{
    public class FileQueueHandler
    {
        private readonly MainWindow _mainWindow;
        private readonly Queue<string> _fileQueue = new Queue<string>();

        public FileQueueHandler(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public void QueueFiles(IEnumerable<string> fileSystemEntries)
        {
            foreach (var entry in fileSystemEntries)
            {
                if (File.Exists(entry))
                {
                    _fileQueue.Enqueue(entry);
                }
                else if (Directory.Exists(entry))
                {
                    QueueFiles(Directory.EnumerateFileSystemEntries(entry));
                }
            }
        }

        private void HandleNextFile()
        {
            var nextFile = _fileQueue.Dequeue();
        }
    }
}