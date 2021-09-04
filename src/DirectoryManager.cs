using DirectoryWatchDog.FunctionslTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DirectoryWatchDog
{
    public static class DirectoryManager
    {
        public static Result<string, bool> CreateDirectory(string path)
        {
            if (path.IsEmpty())
            {
                return "Empty path".Failure<string, bool>();
            }

            if (Directory.Exists(path))
            {
                return true.Ok<string, bool>();
            }

            Directory.CreateDirectory(path);

            return true.Ok<string, bool>();
        }

        public static Result<string, bool> DoDirectoryExists(string path)
        {
            if (path.IsEmpty())
            {
                return "Empty path".Failure<string, bool>();
            }

            if (Directory.Exists(path))
            {
                return true.Ok<string, bool>();
            }

            return "Not found".Failure<string, bool>();
        }

        public static Result<string, bool> CopyFileToDirectory(string filePath, string directoryPath)
        {
            if (filePath.IsEmpty())
            {
                return "Empty file path".Failure<string, bool>();
            }

            if (directoryPath.IsEmpty())
            {
                return "Empty directory path".Failure<string, bool>();
            }

            if (!Directory.Exists(directoryPath))
            {
                return "Directory not found".Failure<string, bool>();
            }

            if (!File.Exists(filePath))
            {
                return "File not found".Failure<string, bool>();
            }

            File.Copy(filePath, Path.Combine(directoryPath, new FileInfo(filePath).Name));

            return Result<string, bool>.Ok(true);
        }

        public static Result<string, IEnumerable<FileInfo>> ReadFilesFromDirectory(string path)
        {
            if (path.IsEmpty())
            {
                return "Empty path".Failure<string, IEnumerable<FileInfo>>();
            }

            if (!Directory.Exists(path))
            {
                return "Directory not found".Failure<string, IEnumerable<FileInfo>>();
            }

            var files = Directory.GetFiles(path).Select(m => new FileInfo(m));

            return Result<string, IEnumerable<FileInfo>>.Ok(files);
        }

        public static Result<string, bool> WatchDirectory(string path, Action<FileInfo, WatcherChangeTypes> onChange, Action existWatchMode)
        {
            if (path.IsEmpty())
            {
                return "Empty path".Failure<string, bool>();
            }

            if (!Directory.Exists(path))
            {
                return "Directory not found".Failure<string, bool>();
            }

            RunWatcher(path, onChange, existWatchMode);

            //using (var watcher = new FileSystemWatcher(path))
            //{
            //    watcher.NotifyFilter = NotifyFilters.Attributes
            //                     | NotifyFilters.CreationTime
            //                     | NotifyFilters.DirectoryName
            //                     | NotifyFilters.FileName
            //                     | NotifyFilters.LastAccess
            //                     | NotifyFilters.LastWrite
            //                     | NotifyFilters.Security
            //                     | NotifyFilters.Size;

            //    void change(object sender, FileSystemEventArgs e)
            //    {
            //        onChange(new FileInfo(e.FullPath));
            //    }

            //    watcher.Created += change;
            //    watcher.Changed += change;

            //    watcher.Renamed += (object sender, RenamedEventArgs e) =>
            //    {
            //        onChange(new FileInfo(e.FullPath));
            //    };

            //    watcher.EnableRaisingEvents = true;

            //    existWatchMode();
            //}

            return true.Ok<string, bool>();
        }

        /************************** FP Helpers ************************/

        private static Result<string, bool> PathToBoolResult(Result<string, string> result) =>
            Result<string, bool>.Of(result.Sucess, result.Sucess, result.ErrorValue);

        private static Result<string, string> DirExists(string path) =>
            Result<string, string>.Of(Directory.Exists(path), path, "Directory not found");

        private static Result<string, string> DirNotEmpty(string path) =>
            Result<string, string>.Of(path.SafeTrim().NotEmpty(), path, "Empty path");

        /************************** DoDirectoryExistsFP ************************/

        public static Result<string, bool> DoDirectoryExistsFP(string path) =>
            DirNotEmpty(path)
            .Bind(DirExists)
            .Map(PathToBoolResult);

        /************************** CreateDirectoryFP ************************/

        public static Result<string, bool> CreateDirectoryFP(string path) =>
            DirNotEmpty(path)
            .Bind(m => Result<string, string>.Of(!Directory.Exists(path), path, "Already exists"))
            .Tee(m => Directory.CreateDirectory(m))
            .Map(PathToBoolResult);


        /************************** CopyFileToDirectoryFP ************************/

        public static Result<string, bool> CopyFileToDirectoryFP(string filePath, string directoryPath) =>
            DirNotEmpty(directoryPath)
            .Bind(m => Result<string, string>.Of(File.Exists(filePath), filePath, "File not found"))
            .Bind(m => DirNotEmpty(filePath))
            .Bind(m => DirExists(directoryPath))
            .Tee(m => File.Copy(filePath, Path.Combine(directoryPath, new FileInfo(filePath).Name)))
            .Map(PathToBoolResult);

        /************************** ReadFilesFromDirectoryFP ************************/

        public static Result<string, IEnumerable<FileInfo>> ReadFilesFromDirectoryFP(string path) =>
            DirNotEmpty(path)
            .Bind(DirExists)
            .MapResult(m => Directory.GetFiles(path).Select(p => new FileInfo(p)).Ok<string, IEnumerable<FileInfo>>());

        /************************** WatchDirectoryFP ************************/

        public static Result<string, bool> WatchDirectoryFP(string path, Action<FileInfo, WatcherChangeTypes> onChange, Action existWatchMode) =>
            DirNotEmpty(path)
            .Bind(DirExists)
            .Tee(m => RunWatcher(path, onChange, existWatchMode))
            .Map(PathToBoolResult);

        private static void RunWatcher(string path, Action<FileInfo, WatcherChangeTypes> onChange, Action existWatchMode)
        {
            void change(object sender, FileSystemEventArgs e)
            {
                onChange(new FileInfo(e.FullPath), e.ChangeType);
            }

            var watcher = new FileSystemWatcher(path)
            {
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size
            };

            using (watcher)
            {
                watcher.Created += change;
                watcher.Changed += change;
                watcher.Renamed += (object sender, RenamedEventArgs e) =>
                {
                    onChange(new FileInfo(e.FullPath), e.ChangeType);
                };

                existWatchMode();
            }
        }
    }
}
