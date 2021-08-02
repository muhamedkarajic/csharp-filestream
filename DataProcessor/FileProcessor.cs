using System;
using System.IO;

namespace DataProcessor
{
    class FileProcessor
    {
        private static readonly string BackupDirectoryName = "backup";
        private static readonly string InProgressDirectoryName = "processing";
        private static readonly string CompletedDirectoryName = "complete";

        public string InputFilePath { get; set; }

        public FileProcessor(string filePath)
        {
            InputFilePath = filePath;
        }

        public void Process()
        {
            Console.WriteLine($"Begin process of {InputFilePath}");
            if (!File.Exists(InputFilePath))
            {
                Console.WriteLine($"ERROR: File {InputFilePath} dose not exist.");
                return;
            }
            string rootDirectoryPath = new DirectoryInfo(InputFilePath).Parent.Parent.FullName;
            Console.WriteLine($"Root data path is {rootDirectoryPath}");

            string inputFileDirectoryPath = Path.GetDirectoryName(InputFilePath);
            string backupDirectoryPath = Path.Combine(rootDirectoryPath, BackupDirectoryName);

            if (!Directory.Exists(backupDirectoryPath))
            {
                Console.WriteLine($"Creating {backupDirectoryPath}");
                Directory.CreateDirectory(backupDirectoryPath);
            }

            string inputFileName = Path.GetFileName(InputFilePath);
            string backupFilePath = Path.Combine(backupDirectoryPath, inputFileName);
            Console.WriteLine($"Copying {inputFileName} to {backupFilePath}");
            File.Copy(InputFilePath, backupFilePath, true);

            Directory.CreateDirectory(Path.Combine(rootDirectoryPath, InProgressDirectoryName));
            string inProgressFilePath = Path.Combine(rootDirectoryPath, InProgressDirectoryName, inputFileName);

            if (File.Exists(inProgressFilePath))
            {
                Console.WriteLine($"ERROR: A file with the name {inProgressFilePath} is already being processed.");
                return;
            }

            Console.WriteLine($"Moving {InputFilePath} to {inProgressFilePath}");
            File.Move(InputFilePath, inProgressFilePath);

            string extension = Path.GetExtension(InputFilePath);
            switch (extension)
            {
                case ".txt":
                    ProcessTextFile(inProgressFilePath);
                    break;
                default:
                    Console.WriteLine($"{extension} is an unsupported file type.");
                    break;
            }

            string completeDirectoryPath = Path.Combine(rootDirectoryPath, CompletedDirectoryName);
            Directory.CreateDirectory(completeDirectoryPath);

            Console.WriteLine($"Moving {inProgressFilePath} to {completeDirectoryPath}");

            var completeFileName = $"{Path.GetFileNameWithoutExtension(InputFilePath)}-{Guid.NewGuid()}{extension}";
            completeFileName = Path.ChangeExtension(completeFileName, ".complete");

            var completedFilePath = Path.Combine(completeDirectoryPath, completeFileName);

            File.Move(inProgressFilePath, completedFilePath);

            // Note: Multiple Files are Processed at the same time therefor if the folder dosent exist it might cause errors.
            // string inProgressDirectoryPath = Path.GetDirectoryName(inProgressFilePath);
            // Directory.Delete(inProgressDirectoryPath, true);
        }

        private void ProcessTextFile(string inProgressFilePath)
        {
            Console.WriteLine($"Processing text file {InputFilePath}");
        }
    }
}