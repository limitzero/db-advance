using System.Collections.Generic;

namespace DbAdvance.Host
{
    public interface IFileSystem
    {
        void DeleteFolderContents(string path);

        void CopyFolderContents(string path, string remotePath);

        void CopyFolderContents(string path, string remotePath, bool copySubFolders);

        void CopyFolderWithSubfoldersFilter(string sourcePath, string destPath, string excludePattern);

        void CopyFileToFolder(string path, string destPath);

        void CreateFolder(string path);

        bool FolderExists(string path);

        bool FileExists(string path);

        void DeleteFile(string path);

        void CopyFile(string path, string destFileName);

        void DeleteFolder(string path);

        IEnumerable<string> GetInstallFilesInPath(string path);

        IEnumerable<string> GetRollbackFilesInPath(string path);

        /// <summary>
        /// Retreives the set of root directories for a series of files that have not been 
        /// executed against the target.
        /// </summary>
        /// <param name="pending"></param>
        /// <returns></returns>
        IEnumerable<string> GetDirectoriesForPendingFiles(IEnumerable<string> pending);

        void GetFilesInPath(HashSet<string> files, string packagePath, string pattern = "*.sql");

        string GetFileNameForPackageAsZipFile(string packageAsZipFile = "");

        void TraverseDirectoriesUntilFolderReached(
            string foundDirectoryPath,
            string startingDirectoryPath,
            params string[] foldersToInspectFor);

        void GetFoldersInPath(HashSet<string> folders, string path);
    }
}