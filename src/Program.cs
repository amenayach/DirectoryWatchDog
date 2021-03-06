using DirectoryWatchDog.FunctionslTools;
using System;
using System.IO;
using System.Linq;

namespace DirectoryWatchDog
{
    class Program
    {
        static void Main(string[] args)
        {
            var exit = false;

            while (!exit)
            {
                ArgumentActionRouter.RunAsync(
                        ("CreateDirectory", CreateDirectory),
                        ("DoDirectoryExists", DoDirectoryExists),
                        ("CopyFileToDirectory", CopyFileToDirectory),
                        ("ReadFilesFromDirectory", ReadFilesFromDirectory),
                        ("WatchDirectory", WatchDirectory),
                        ("DeleteFile", DeleteFile),
                        ("DownloadThenWriteToDirectory", DownloadThenWriteToDirectory),
                        ("Exit", () => { exit = true; }
                )
                    );
            }
        }

        static void CreateDirectory()
        {
            var directoryPath = ReadInput("Please enter directory path:");

            var result = DirectoryManager.CreateDirectoryFP(directoryPath);

            $"{result.Sucess} - {result.ErrorValue}".Print(ConsoleColor.Yellow);
        }

        static void DoDirectoryExists()
        {
            var directoryPath = ReadInput("Please enter directory path:");

            var result = DirectoryManager.DoDirectoryExistsFP(directoryPath);

            $"{result.Sucess} - {result.ErrorValue}".Print(ConsoleColor.Yellow);
        }

        static void CopyFileToDirectory()
        {
            var filePath = ReadInput("Please enter file path:");
            var directoryPath = ReadInput("Please enter directory path:");

            var result = DirectoryManager.CopyFileToDirectoryFP(filePath, directoryPath);

            $"{result.Sucess} - {result.ErrorValue}".Print(ConsoleColor.Yellow);
        }

        static void ReadFilesFromDirectory()
        {
            var directoryPath = ReadInput("Please enter directory path:");

            var result = DirectoryManager.ReadFilesFromDirectoryFP(directoryPath);

            result.Match(
            m =>
            {
                $"{m.Sucess} - {m.ErrorValue}".Print(ConsoleColor.Yellow);
            },
            m =>
            {
                foreach (var fileInfo in m.Value) PrintFile(fileInfo);
            });
        }

        static void DeleteFile()
        {
            var filePath = ReadInput("Please enter file path:");

            var result = DirectoryManager.DeleteFileFP(filePath);

            $"{result.Sucess} - {result.ErrorValue}".Print(ConsoleColor.Yellow);
        }

        static void DownloadThenWriteToDirectory()
        {
            var url = ReadInput("Please enter the url to download from:");
            var fileName = ReadInput("Please enter the file name:");
            var directoryPath = ReadInput("Please enter the directory path:");
            var proxy = ReadInput("Please enter the proxy address:");

            fileName = GetFileNameIfEmpty(url, fileName);

            var result = DirectoryManager.DownloadThenWriteToDirectoryFP(url, fileName, directoryPath, proxy);

            $"{result.Sucess} - {result.ErrorValue}".Print(ConsoleColor.Yellow);
        }

        static void WatchDirectory()
        {
            var directoryPath = ReadInput("Please enter directory path:");

            Console.WriteLine("Press enter to exit watch mode");

            var result = DirectoryManager.WatchDirectoryFP(directoryPath, PrintFileChange, () => Console.ReadLine());

            $"{result.Sucess} - {result.ErrorValue}".Print(ConsoleColor.Yellow);
        }

        private static string GetFileNameIfEmpty(string url, string fileName) =>
            fileName.NotEmpty() ? fileName : url.Split('/').Last();

        private static string ReadInput(string message)
        {
            message.Print();
            return Console.ReadLine();
        }

        private static void PrintFile(FileInfo fileInfo)
        {
            var firstBytes = ReadBytes(fileInfo, 10);
            var firstBytesText = firstBytes.NotEmpty() ? string.Join(' ', firstBytes) : string.Empty;
            $"{fileInfo.Name} - {fileInfo.CreationTime} - {fileInfo.Length}bytes - First bytes: {firstBytesText}".Print(ConsoleColor.Yellow);
        }

        private static byte[] ReadBytes(FileInfo fileInfo, int limit)
        {
            var byteLimit = (int)Math.Min(fileInfo.Length, limit);
            var bytes = new byte[byteLimit];

            using (var fileStream = new FileStream(fileInfo.FullName, FileMode.Open))
            using (var reader = new BinaryReader(fileStream))
            {
                reader.Read(bytes, 0, byteLimit);
            }

            return bytes;
        }

        private static void PrintFileChange(FileInfo fileInfo, WatcherChangeTypes changeType) =>
            $"{fileInfo.Name} - {changeType} - {fileInfo.CreationTime} - {fileInfo.Length}bytes".Print(ConsoleColor.Yellow);
    }
}
