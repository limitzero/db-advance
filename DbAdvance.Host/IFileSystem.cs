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
    }
}