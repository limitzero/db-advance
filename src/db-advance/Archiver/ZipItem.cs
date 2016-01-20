namespace DbAdvance.Host.Archiver
{
    public class ZipItem
    {
        public ZipItem(string filePath, string folderInZip)
            : this(filePath, folderInZip, false)
        {
        }

        public ZipItem(string filePath, string folderInZip, bool isDirectory)
        {
            FilePath = filePath;
            FolderInZip = folderInZip;
            IsDirectory = isDirectory;
        }

        public string FilePath { get; set; }

        public string FolderInZip { get; set; }

        public bool IsDirectory { get; set; }
    }
}