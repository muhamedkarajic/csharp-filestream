using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace DataProcessor
{
    class Program
    {
        private static IMemoryCache FilesToProcess = new MemoryCache(new MemoryCacheOptions());

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

            AddToCache(e.FullPath);
        }

        private static void FileChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"Event: File Changed | {e.Name} - {e.ChangeType}");

            AddToCache(e.FullPath);
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

        private static void AddToCache(string fullPath)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(2));

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .AddExpirationToken(new CancellationChangeToken(cts.Token))
                .RegisterPostEvictionCallback(ProcessFile);

            FilesToProcess.Set(fullPath, fullPath, cacheEntryOptions);
        }

        private static void ProcessFile(object key, object value, EvictionReason reason, object state)
        {
            Console.WriteLine($"Event: Reason {reason}, object {value}.");
            if (reason == EvictionReason.TokenExpired)
            {
                var fileProcessor = new FileProcessor(key.ToString());
                fileProcessor.Process();
            }
        }
    }
}
