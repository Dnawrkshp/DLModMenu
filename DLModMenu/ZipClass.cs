using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Packaging;

namespace DLModMenu
{
    class ZipClass
    {
        private const long BUFFER_SIZE = 4096;

        public static void AddFileToZip(string zipFilename, string fileToAdd)
        {
            using (Package zip = System.IO.Packaging.Package.Open(zipFilename, FileMode.OpenOrCreate))
            {
                string destFilename = ".\\" + Path.GetFileName(fileToAdd);
                Uri uri = PackUriHelper.CreatePartUri(new Uri(destFilename, UriKind.Relative));
                if (zip.PartExists(uri))
                {
                    zip.DeletePart(uri);
                }
                PackagePart part = zip.CreatePart(uri, "",CompressionOption.Normal);
                using (FileStream fileStream = new FileStream(fileToAdd, FileMode.Open, FileAccess.Read))
                {
                    using (Stream dest = part.GetStream())
                    {
                        CopyStream(fileStream, dest);
                    }
                }
            }
        }

        private static void CopyStream(System.IO.FileStream inputStream, System.IO.Stream outputStream)
        {
            long bufferSize = inputStream.Length < BUFFER_SIZE ? inputStream.Length : BUFFER_SIZE;
            byte[] buffer = new byte[bufferSize];
            int bytesRead = 0;
            long bytesWritten = 0;
            while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                outputStream.Write(buffer, 0, bytesRead);
                bytesWritten += bufferSize;
            }
        }

        private static void CopyStream(System.IO.Stream inputStream, System.IO.FileStream outputStream)
        {
            long bufferSize = inputStream.Length < BUFFER_SIZE ? inputStream.Length : BUFFER_SIZE;
            byte[] buffer = new byte[bufferSize];
            int bytesRead = 0;
            long bytesWritten = 0;
            while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                outputStream.Write(buffer, 0, bytesRead);
                bytesWritten += bufferSize;
            }
        }

        public static List<string> ExtractZIP(string filename, string outDir)
        {

            List<string> ret = new List<string>();

            using (Package package = ZipPackage.Open(filename, FileMode.Open, FileAccess.Read))
            {
                foreach (PackagePart part in package.GetParts())
                {
                    string target = Path.GetFullPath(Path.Combine(outDir, part.Uri.OriginalString.TrimStart('/')));
                    var targetDir = target.Remove(target.LastIndexOf('\\'));
                    ret.Add(target);

                    if (!Directory.Exists(targetDir))
                        Directory.CreateDirectory(targetDir);

                    using (Stream source = part.GetStream(FileMode.Open, FileAccess.Read))
                    {
                        FileStream targetFile = File.OpenWrite(target);
                        //source.CopyTo(targetFile);
                        CopyStream(source, targetFile);
                        targetFile.Close();
                    }
                }
            }

            return ret;
        }
    
    }
}
