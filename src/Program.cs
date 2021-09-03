using DirectoryWatchDog.FunctionslTools;
using System;

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
                        ("Exit", () => { exit = true; })
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

        static void WatchDirectory()
        {
            var directoryPath = ReadInput("Please enter directory path:");

            Console.WriteLine("Press enter to exit watch mode");

            var result = DirectoryManager.WatchDirectoryFP(directoryPath, PrintFile, () => Console.ReadLine());

            $"{result.Sucess} - {result.ErrorValue}".Print(ConsoleColor.Yellow);
        }

        private static string ReadInput(string message)
        {
            message.Print();
            return Console.ReadLine();
        }

        private static void PrintFile(System.IO.FileInfo fileInfo) =>
            $"{fileInfo.Name} - {fileInfo.CreationTime} - {fileInfo.Length}bytes".Print(ConsoleColor.Yellow);
    }
}
