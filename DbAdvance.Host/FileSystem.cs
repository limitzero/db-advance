using System;
using System.Globalization;
using System.IO;

namespace DbAdvance.Host
{
    public class FileSystem : IFileSystem
    {
        private readonly ILogger log;

        public FileSystem(ILogger log)
        {
            this.log = log;
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

        private static string GetFileName(string path)
        {
            var fileName = Path.GetFileName(path);

            if (fileName == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Can not extract file name from path '{0}'", path));
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
                        log.Log(file);
                    }
                }
                else
                {
                    destFilePath = Path.Combine(remotePath, fileName);
                    CopyFile(file, destFilePath);
                    log.Log(file);
                }
            }
        }
    }
}