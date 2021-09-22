using DirectoryWatchDog.FunctionslTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

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

        public static Result<string, bool> DeleteFile(string filePath)
        {
            if (filePath.IsEmpty())
            {
                return "Empty file path".Failure<string, bool>();
            }

            if (!File.Exists(filePath))
            {
                return "File not found".Failure<string, bool>();
            }

            File.Delete(filePath);

            return Result<string, bool>.Ok(true);
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

            return true.Ok<string, bool>();
        }

        /************************** FP Helpers ************************/

        private static Result<string, bool> ToBoolResult<T>(Result<string, T> result) =>
            Result<string, bool>.Of(result.Sucess, result.Sucess, result.ErrorValue);

        private static Result<string, string> DirExists(string path) =>
            Result<string, string>.Of(Directory.Exists(path), path, "Directory not found");

        private static Result<string, string> NotEmpty(string path) =>
            Result<string, string>.Of(path.SafeTrim().NotEmpty(), path, "Empty path");

        /************************** DoDirectoryExistsFP ************************/

        public static Result<string, bool> DoDirectoryExistsFP(string path) =>
            NotEmpty(path)
            .Bind(DirExists)
            .Map(ToBoolResult);

        /************************** CreateDirectoryFP ************************/

        public static Result<string, bool> CreateDirectoryFP(string path) =>
            NotEmpty(path)
            .Bind(m => Result<string, string>.Of(!Directory.Exists(path), path, "Already exists"))
            .Tee(m => Directory.CreateDirectory(m))
            .Map(ToBoolResult);


        /************************** CopyFileToDirectoryFP ************************/

        public static Result<string, bool> CopyFileToDirectoryFP(string filePath, string directoryPath) =>
            NotEmpty(directoryPath)
            .Bind(m => Result<string, string>.Of(File.Exists(filePath), filePath, "File not found"))
            .Bind(m => NotEmpty(filePath))
            .Bind(m => DirExists(directoryPath))
            .Tee(m => File.Copy(filePath, Path.Combine(directoryPath, new FileInfo(filePath).Name)))
            .Map(ToBoolResult);

        /************************** DownloadThenWiteToDirectoryFP ************************/

        public static Result<string, bool> DownloadThenWriteToDirectoryFP(string url, string fileName, string directoryPath, string proxy) =>
            NotEmpty(directoryPath)
            .Bind(m => NotEmpty(fileName))
            .Bind(m => DirExists(directoryPath))
            .BindTo(m => DownloadFromUrl(url, proxy))
            .Tee(m => File.WriteAllBytes(Path.Combine(directoryPath, new FileInfo(fileName).Name), m))
            .Map(ToBoolResult);

        /************************** ReadFilesFromDirectoryFP ************************/

        public static Result<string, IEnumerable<FileInfo>> ReadFilesFromDirectoryFP(string path) =>
            NotEmpty(path)
            .Bind(DirExists)
            .MapResult(m => Directory.GetFiles(path).Select(p => new FileInfo(p)).Ok<string, IEnumerable<FileInfo>>());

        /************************** DeleteFileFP ************************/

        public static Result<string, bool> DeleteFileFP(string filePath) =>
            NotEmpty(filePath)
            .Bind(m => Result<string, string>.Of(File.Exists(filePath), filePath, "File not found"))
            .Tee(m => File.Delete(filePath))
            .Map(ToBoolResult);

        /************************** WatchDirectoryFP ************************/

        public static Result<string, bool> WatchDirectoryFP(string path, Action<FileInfo, WatcherChangeTypes> onChange, Action existWatchMode) =>
            NotEmpty(path)
            .Bind(DirExists)
            .Tee(m => RunWatcher(path, onChange, existWatchMode))
            .Map(ToBoolResult);

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

        private static Result<string, byte[]> DownloadFromUrl(string url, string proxy)
        {
            var useProxy = proxy.NotEmpty();

            var handler = new HttpClientHandler
            {
                UseProxy = useProxy,
                Proxy = useProxy ? new WebProxy(proxy) : null
            };

            using (var httpClient = new HttpClient(handler))
            {
                var bytes = httpClient.GetByteArrayAsync(url).GetAwaiter().GetResult();
                return bytes.Ok<string, byte[]>();
            }
        }
    }
}
