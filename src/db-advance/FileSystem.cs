using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Castle.Core.Logging;

namespace DbAdvance.Host
{
    public class FileSystem : IFileSystem
    {
        private readonly ILogger _logger;

        public static IList<string> InstallWhiteListDirectoryNames = new[]
        {
            "Install",
            "Commit"
        };

        public static IList<string> RollbackWhiteListDirectoryNames = new[]
        {
            "Rollback",
        };

        public FileSystem(ILogger logger)
        {
            this._logger = logger;
        }

        public void DeleteFolderContents(string path)
        {
            foreach (var dir in Directory.EnumerateDirectories(path))
            {
                Directory.Delete(dir, true);
            }

            foreach (var dir in Directory.EnumerateFiles(path))
            {
                DeleteFile(dir);
            }
        }

        public void DeleteFile(string dir)
        {
            File.Delete(dir);
        }

        public void CopyFolderContents(string path, string remotePath)
        {
            CopyDirectory(remotePath, path, true);
        }

        public void CopyFolderContents(string path, string remotePath, bool copySubFolders)
        {
            CopyDirectory(remotePath, path, copySubFolders);
        }

        public void CopyFolderWithSubfoldersFilter(string sourcePath, string destPath, string excludePattern)
        {
            CreateFolder(destPath);

            foreach (var file in Directory.GetFiles(sourcePath))
            {
                var dest = Path.Combine(destPath, Path.GetFileName(file));
                File.Copy(file, dest);
            }

            foreach (var folder in Directory.GetDirectories(sourcePath))
            {
                var folderName = Path.GetFileName(folder);

                if (folderName != excludePattern)
                {
                    var dest = Path.Combine(destPath, folderName);
                    CopyFolderWithSubfoldersFilter(folder, dest, excludePattern);
                }
            }
        }

        public void CreateFolder(string path)
        {
            if (!FolderExists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public void CopyFileToFolder(string path, string destPath)
        {
            var fileName = GetFileName(path);
            var destFileName = Path.Combine(destPath, fileName);

            CopyFile(path, destFileName);
        }

        public void CopyFile(string path, string destFileName)
        {
            if (FileExists(destFileName))
            {
                File.Delete(destFileName);
            }

            File.Copy(path, destFileName);
        }

        public void DeleteFolder(string path)
        {
            Directory.Delete(path, true);
        }

        public bool FolderExists(string path)
        {
            return Directory.Exists(path);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public IEnumerable<string> GetInstallFilesInPath(string path)
        {
            var files = new HashSet<string>();
            GetFilesInPath(files, path);

            var scripts = files
                .Select(file => Path.GetDirectoryName(file))
                .Where(directory => InstallWhiteListDirectoryNames.Any(name => directory.Contains(name)))
                .Distinct()
                .ToList();

            return scripts;
        }

        public IEnumerable<string> GetRollbackFilesInPath(string path)
        {
            var files = new HashSet<string>();
            GetFilesInPath(files, path);

            var scripts = files
                .Select(file => Path.GetDirectoryName(file))
                .Where(directory => RollbackWhiteListDirectoryNames.Any(name => directory.Contains(name)))
                .Distinct()
                .ToList();

            return scripts;
        }

        public IEnumerable<string> GetDirectoriesForPendingFiles(IEnumerable<string> pending)
        {
            var directories = pending.Select(p => Directory.GetParent(p));

            while (
                FileSystem.InstallWhiteListDirectoryNames.Any(name => directories.Any(f => f.FullName.Contains(name))) ||
                FileSystem.RollbackWhiteListDirectoryNames.Any(name => directories.Any(f => f.FullName.Contains(name))))
            {
                directories = directories.Select(p => Directory.GetParent(p.FullName));
            }

            return directories.Select(d => d.FullName).Distinct().ToList();
        }

        public void GetFilesInPath(HashSet<string> files, string packagePath, string pattern = "*.sql")
        {
            try
            {
                foreach (string d in Directory.GetDirectories(packagePath))
                {
                    foreach (string f in Directory.GetFiles(d, pattern))
                    {
                        if (files.Any(file => file.Equals(f))) continue;
                        files.Add(f);
                    }
                    GetFilesInPath(files, d);
                }
            }
            catch
            {
            }
        }

        public void GetFoldersInPath(HashSet<string> folders, string path)
        {
            foreach (string d in Directory.GetDirectories(path))
            {
                foreach (string f in Directory.GetDirectories(d))
                {
                    folders.Add(f);
                }
                GetFoldersInPath(folders, d);
            }
        }

        public void TraverseDirectoriesUntilFolderReached(
            string foundDirectoryPath,
            string startingDirectoryPath,
            params string[] foldersToInspectFor)
        {
            if (!string.IsNullOrEmpty(foundDirectoryPath)) return;

            Func<string, string> targetsFound = (directory) =>
            {
                if (foldersToInspectFor.Any(name => directory.Contains(name)))
                {
                    var result = directory;
                    foreach (var folder in foldersToInspectFor)
                    {
                        if (result.Contains(folder))
                            result = result.Replace(string.Concat(@"\", folder), string.Empty);
                    }

                    return result;
                }

                return string.Empty;
            };

            foreach (string d in Directory.GetDirectories(startingDirectoryPath))
            {
                foundDirectoryPath = targetsFound(d);
                if (!string.IsNullOrEmpty(foundDirectoryPath)) break;

                foreach (string s in Directory.GetDirectories(d))
                {
                    foundDirectoryPath = targetsFound(s);
                    if (!string.IsNullOrEmpty(foundDirectoryPath)) break;
                }

                if (!string.IsNullOrEmpty(foundDirectoryPath)) break;
                TraverseDirectoriesUntilFolderReached(foundDirectoryPath, d, foldersToInspectFor);
            }
        }

        public string GetFileNameForPackageAsZipFile(string packageAsZipFile = "")
        {
            var zipFileName = string.Empty;

            if (File.Exists(packageAsZipFile))
            {
                zipFileName = Path.GetFileNameWithoutExtension(packageAsZipFile);
            }
            else if (!string.IsNullOrEmpty(packageAsZipFile)
                     && packageAsZipFile.EndsWith(".zip"))
            {
                zipFileName = packageAsZipFile.Replace(".zip", string.Empty);
            }
            else if (string.IsNullOrEmpty(packageAsZipFile))
            {
                zipFileName = Directory
                    .EnumerateFiles(Environment.CurrentDirectory)
                    .Where(file => Path.GetExtension(file) == ".zip")
                    .OrderByDescending(file => file)
                    .FirstOrDefault()
                    .Replace(".zip", string.Empty);
            }

            return zipFileName;
        }

        private static string GetFileName(string path)
        {
            var fileName = Path.GetFileName(path);

            if (fileName == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                    "Can not extract file name from path '{0}'", path));
            }

            return fileName;
        }

        private void CopyDirectory(string remotePath, string path, bool copySubFolders)
        {
            Directory.CreateDirectory(remotePath);

            foreach (var file in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
            {
                var fileName = GetFileName(file);
                var filePath = Path.GetDirectoryName(file.Substring(path.Length + (path.EndsWith("\\") ? 0 : 1)));

                string destFilePath;

                if (!string.IsNullOrEmpty(filePath))
                {
                    if (copySubFolders)
                    {
                        var destFolderPath = Path.Combine(remotePath, filePath);
                        CreateFolder(destFolderPath);

                        destFilePath = Path.Combine(destFolderPath, fileName);
                        CopyFile(file, destFilePath);
                        _logger.Info(file);
                    }
                }
                else
                {
                    destFilePath = Path.Combine(remotePath, fileName);
                    CopyFile(file, destFilePath);
                    _logger.Info(file);
                }
            }
        }
    }
}