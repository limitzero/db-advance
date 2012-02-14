using System;
using System.Collections.Generic;
using System.IO;

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace DbAdvance.Host.Archiver
{
    public class ZipArchiver
    {
        public void ZipFolder(string outZipFileName, string path)
        {
            var directoryInfo = new DirectoryInfo(path);

            CreateZipFile(outZipFileName, directoryInfo.FullName, directoryInfo);
        }

        public void Unzip(string zipFilePath, string folder)
        {
            var s = new ZipInputStream(File.OpenRead(zipFilePath));
            ZipEntry theEntry;

            while ((theEntry = s.GetNextEntry()) != null)
            {
                var directoryName = Path.GetDirectoryName(theEntry.Name);
                var fileName = Path.GetFileName(theEntry.Name);

                Directory.CreateDirectory(Path.Combine(folder, directoryName));
                if (fileName == string.Empty)
                {
                    continue;
                }

                var streamWriter = File.Create(Path.Combine(folder, theEntry.Name));
                var data = new byte[2048];

                while (true)
                {
                    var size = s.Read(data, 0, data.Length);
                    if (size > 0)
                    {
                        streamWriter.Write(data, 0, size);
                    }
                    else
                    {
                        break;
                    }
                }

                streamWriter.Close();
            }

            s.Close();
        }

        public void Zip(string outPathname, IList<ZipItem> contents)
        {
            var outputStream = File.Create(outPathname);
            var zipStream = new ZipOutputStream(outputStream);

            zipStream.SetLevel(3);

            foreach (var item in contents)
            {
                if (item.IsDirectory)
                {
                    var files = Directory.EnumerateFiles(item.FilePath);

                    foreach (var file in files)
                    {
                        AppendFile(zipStream, item.FolderInZip, file);
                    }
                }
                else
                {
                    AppendFile(zipStream, item.FolderInZip, item.FilePath);
                }
            }

            zipStream.IsStreamOwner = true;
            zipStream.Close();
        }

        private static void AppendFile(ZipOutputStream zipStream, string folderInZip, string filePath)
        {
            var fileName = Path.GetFileName(filePath);

            var fileNameInZip = ZipEntry.CleanName(Path.Combine(folderInZip, fileName));

            var newEntry = new ZipEntry(fileNameInZip) { DateTime = DateTime.Now };

            zipStream.UseZip64 = UseZip64.Off;
            zipStream.PutNextEntry(newEntry);

            var buffer = new byte[4096];
            using (var streamReader = File.OpenRead(filePath))
            {
                StreamUtils.Copy(streamReader, zipStream, buffer);
            }

            zipStream.CloseEntry();
        }

        private static void CreateZipFile(string outPathname, string rootPath, params FileSystemInfo[] fileSystemInfosToZip)
        {
            var z = ZipFile.Create(outPathname);

            z.BeginUpdate();

            GetFilesToZip(fileSystemInfosToZip, rootPath, z);

            z.CommitUpdate();
            z.Close();
        }

        private static void GetFilesToZip(IEnumerable<FileSystemInfo> fileSystemInfosToZip, string rootPath, ZipFile z)
        {
            foreach (var fi in fileSystemInfosToZip)
            {
                if (fi is DirectoryInfo)
                {
                    var di = (DirectoryInfo)fi;
                    GetFilesToZip(di.GetFileSystemInfos(), rootPath, z);
                }
                else
                {
                    var fileName = fi.FullName.Replace(rootPath, string.Empty);
                    z.Add(fi.FullName, fileName);
                }
            }
        }
    }
}