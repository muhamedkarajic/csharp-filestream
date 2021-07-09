using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace DataProcessor
{
    class Program
    {
        private static ConcurrentDictionary<string, string> FilesToProcess = new ConcurrentDictionary<string, string>();

        static void Main(string[] args)
        {
            Console.WriteLine("Parsing command line options");

            var directoryToWatch = args[0];

            if (!Directory.Exists(directoryToWatch))
                Console.WriteLine($"Error: {directoryToWatch} dose not exist.");
            else
            {
                Console.WriteLine($"Watching directory {directoryToWatch} for changes.");

                using (var inputFileWatcher = new FileSystemWatcher(directoryToWatch))
                using (var timer = new Timer(ProcessFiles, null, 0, 1000))
                {
                    inputFileWatcher.IncludeSubdirectories = false;
                    inputFileWatcher.InternalBufferSize = 32768; //32 KB
                    inputFileWatcher.Filter = "*.*";
                    inputFileWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;

                    inputFileWatcher.Created += FileCreated;
                    inputFileWatcher.Changed += FileChanged;
                    inputFileWatcher.Deleted += FileDeleted;
                    inputFileWatcher.Renamed += FileRenamed;
                    inputFileWatcher.Error += FileError;

                    inputFileWatcher.EnableRaisingEvents = true;

                    Console.WriteLine("Press enter to quit.");
                    Console.ReadLine();
                }
            }
        }

        private static void FileCreated(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"Event: File Created | {e.Name} - {e.ChangeType}");

            FilesToProcess.TryAdd(e.FullPath, e.FullPath);
        }

        private static void FileChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"Event: File Changed | {e.Name} - {e.ChangeType}");

            FilesToProcess.TryAdd(e.FullPath, e.FullPath);
        }

        private static void FileDeleted(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"Event: File Deleted | {e.Name} - {e.ChangeType}");
        }

        private static void FileRenamed(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"Event: File Renamed | {e.Name} - {e.ChangeType}");
        }

        private static void FileError(object sender, ErrorEventArgs e)
        {
            Console.WriteLine($"ERROR: File system watching may no longer be active: {e.GetException()}");
        }

        private static void ProcessingSingleFile(string filePath)
        {
            var fileProcessor = new FileProcessor(filePath);
            fileProcessor.Process();
        }

        private static void ProcessDirectory(string directoryPath, string fileType)
        {
            var allFiles = Directory.GetFiles(directoryPath);

            switch (fileType)
            {
                case "TEXT":
                    string[] textFiles = Directory.GetFiles(directoryPath, "*.txt");
                    foreach (var textFilePath in textFiles)
                    {
                        var fileProcessor = new FileProcessor(textFilePath);
                        fileProcessor.Process();
                    }
                    break;
                default:
                    Console.WriteLine($"ERROR: {fileType} is not supported.");
                    return;
            }
        }

        private static void ProcessFiles(Object stateInfo)
        {
            foreach (var filePath in FilesToProcess.Keys)
            {
                if (FilesToProcess.TryRemove(filePath, out _))
                {
                    var fileProcessor = new FileProcessor(filePath);
                    fileProcessor.Process();
                }
            }
        }
    }
}
