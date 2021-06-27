using System;
using System.IO;

namespace DataProcessor
{
    class FileProcessor
    {
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
        }
    }
}