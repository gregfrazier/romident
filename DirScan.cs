using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RomIdent2
{
    internal class DirScan
    {
        protected string root;
        protected bool exists;

        protected List<RomModel> files;
        
        protected Int64 directoryCount;
        protected Int64 fileCount;
        protected bool recursive;

        public bool IsValid { get { return exists; } }
        public List<RomModel> FileList { get { return files; } }
        public bool Recurse { get { return recursive; } set { recursive = value; } }

        public DirScan(string dir)
        {
            root = dir;
            exists = Directory.Exists(root);
            if (exists)
            {
                files = new List<RomModel>();
                directoryCount = 0;
                fileCount = 0;
                recursive = false;
            }
        }

        public void PerformScan()
        {
            DirectoryInfo rootDir = new DirectoryInfo(root);
            FileSystemInfo[] items = rootDir.GetFileSystemInfos();
            foreach (FileSystemInfo item in items)
            {
                if (item is DirectoryInfo)
                {
                    if (recursive)
                        PerformScanRecurse(item.FullName);
                }
                else if (item is FileInfo)
                {
                    if (Path.GetExtension(item.FullName) == ".zip")
                        files.AddRange(FileScanner.ZipToModel(item.FullName));
                    else
                        files.Add(FileScanner.FileToModel(item.FullName));
                }
            }
            return;
        }

        private void PerformScanRecurse(string g)
        {
            DirectoryInfo rootDir = new DirectoryInfo(g);
            FileSystemInfo[] items = rootDir.GetFileSystemInfos();
            foreach (FileSystemInfo item in items)
            {
                if (item is DirectoryInfo)
                {
                    PerformScanRecurse(item.FullName);
                }
                else if (item is FileInfo)
                {
                    if (Path.GetExtension(item.FullName) == ".zip")
                        files.AddRange(FileScanner.ZipToModel(item.FullName));
                    else
                        files.Add(FileScanner.FileToModel(item.FullName));
                }
            }
            return;
        }
    }

    internal static class FileScanner
    {
        private static RomModel HashFile(Stream file)
        {
            string md5Hash, sha1Hash, crc32Hash;

            using(var m = new MemoryStream())
            {
                file.CopyTo(m);
                if (m.CanSeek)
                    m.Seek(0, SeekOrigin.Begin);
                using (SHA1 sha1 = SHA1.Create())
                    sha1Hash = ByteString(sha1.ComputeHash(m));

                if (m.CanSeek)
                    m.Seek(0, SeekOrigin.Begin);
                using (MD5 md5 = MD5.Create())
                    md5Hash = ByteString(md5.ComputeHash(m));

                if (m.CanSeek)
                    m.Seek(0, SeekOrigin.Begin);
                crc32 crc = new crc32();
                crc32Hash = ByteString(crc.ComputeHash(m));
            }

            return new RomModel
            {
                CRC = crc32Hash,
                MD5 = md5Hash,
                SHA1 = sha1Hash,
                RomFileName = string.Empty,
                ZipFileName = string.Empty,
                FilePath = string.Empty,
                Size = 0
            };
        }
        public static RomModel FileToModel(string filename)
        {
            RomModel returnValue;
            FileStream file = null;

            if (File.Exists(filename))
                file = new FileStream(filename, FileMode.Open, FileAccess.Read);

            if (file == null)
                return null;

            if (file.CanSeek)
                file.Seek(0, SeekOrigin.Begin);

            returnValue = HashFile(file);
            returnValue.RomFileName = Path.GetFileName(filename);
            returnValue.FilePath = Path.GetDirectoryName(filename);
            
            file.Close();
            file.Dispose();

            return returnValue;
        }

        public static List<RomModel> ZipToModel(string filename)
        {
            List<RomModel> returnValues = new List<RomModel>();
            ZipFile zf = null;
            try
            {
                if (File.Exists(filename))
                {
                    FileStream fs = File.OpenRead(filename);
                    zf = new ZipFile(fs);
                    foreach (ZipEntry zipEntry in zf)
                        if (zipEntry.IsFile)
                        {
                            RomModel ret = HashFile(zf.GetInputStream(zipEntry));
                            ret.RomFileName = zipEntry.Name;
                            ret.ZipFileName = Path.GetFileName(filename);
                            ret.FilePath = Path.GetDirectoryName(filename);
                            returnValues.Add(ret);
                        }
                }
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true;
                    zf.Close();
                }
            }
            return returnValues;
        }

        private static string ByteString(byte[] b)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < b.Length; i++)
                sb.Append(b[i].ToString("x2"));
            return sb.ToString();
        }
    }

    //public class FileHash
    //{
    //    public string fullname;
    //    public string fname;
    //    public bool isZip = false;
    //    public ZipFileHash zipContents;
    //    public string md5Hash;
    //    public string sha1Hash;
    //    public string crc32Hash;

    //    public FileHash(string filename, Stream stream = null, bool isZipFile = false)
    //    {
    //        FileStream file = null;

    //        try
    //        {
    //            this.fullname = filename;
    //            this.fname = Path.GetFileName(filename);
    //            this.isZip = isZipFile;

    //            if (!isZipFile)
    //            {
    //                if (File.Exists(fullname))
    //                    file = new FileStream(fullname, FileMode.Open, FileAccess.Read);

    //                GetSHA1Hash(stream == null ? file : stream);

    //                GetMD5Hash(stream == null ? file : stream);
                    
    //                GetCRCHash(stream == null ? file : stream);

    //                if (file != null)
    //                {
    //                    file.Close();
    //                    file.Dispose();
    //                }
    //            }
    //            else
    //            {
    //                this.zipContents = new ZipFileHash(filename);
    //            }
    //        }
    //        catch(Exception)
    //        {
    //            // Did you see the list of what FileStream can throw? Fuck that, catch all here.
    //            if (file != null)
    //            {
    //                file.Close();
    //                file.Dispose();
    //            }
    //        }
    //    }

    //    protected string GetMD5HashFromFile(string fileName)
    //    {
    //        using (var md5 = MD5.Create())
    //        {
    //            using (var stream = File.OpenRead(fileName))
    //            {
    //                return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
    //            }
    //        }
    //    }

    //    protected void GetMD5Hash(Stream f)
    //    {
    //        bool selfClose = false;
    //        if (f == null)
    //            if (File.Exists(fullname)){
    //                f = new FileStream(fullname, FileMode.Open, FileAccess.Read);
    //                selfClose = true;
    //            }

    //        if (f.CanSeek)
    //            f.Seek(0, SeekOrigin.Begin);
    //        using (MD5 md5 = MD5.Create())
    //        {
    //            md5Hash = ByteString(md5.ComputeHash(f));
    //        }
            
    //        if(selfClose)
    //            f.Close();

    //        return;
    //    }
    //    protected void GetSHA1Hash(Stream f)
    //    {
    //        bool selfClose = false;
    //        if (f == null)
    //            if (File.Exists(fullname))
    //            {
    //                f = new FileStream(fullname, FileMode.Open, FileAccess.Read);
    //                selfClose = true;
    //            }

    //        if (f == null)
    //            return;

    //        if(f.CanSeek)
    //            f.Seek(0, SeekOrigin.Begin);
    //        using (SHA1 sha1 = SHA1.Create())
    //        {
    //            sha1Hash = ByteString(sha1.ComputeHash(f));
    //        }

    //        if (selfClose)
    //            f.Close();
    //        return;
    //    }
    //    protected void GetCRCHash(Stream f)
    //    {
    //        bool selfClose = false;
    //        if (f == null)
    //            if (File.Exists(fullname))
    //            {
    //                f = new FileStream(fullname, FileMode.Open, FileAccess.Read);
    //                selfClose = true;
    //            }

    //        if (f == null)
    //            return;

    //        if (f.CanSeek)
    //            f.Seek(0, SeekOrigin.Begin);
    //        crc32 crc = new crc32();
    //        crc32Hash = ByteString(crc.ComputeHash(f));

    //        if (selfClose)
    //            f.Close();

    //        return;
    //    }

    //    protected string ByteString(byte[] b)
    //    {
    //        StringBuilder sb = new StringBuilder();
    //        for (int i = 0; i < b.Length; i++)
    //            sb.Append(b[i].ToString("x2"));
    //        return sb.ToString();
    //    }
    //}

    //public class ZipFileHash
    //{
    //    public List<FileHash> fileList;
    //    public string fullname;
        
    //    public ZipFileHash(string filename)
    //    {
    //        this.fileList = new List<FileHash>();
    //        this.fullname = filename;
    //        ScanZipFile();
    //    }

    //    public byte[] ScanZipFile()
    //    {
    //        ZipFile zf = null;
    //        try
    //        {
    //            FileStream fs = File.OpenRead(this.fullname);
    //            zf = new ZipFile(fs);
    //            foreach(ZipEntry zipEntry in zf)
    //                if(zipEntry.IsFile)
    //                    fileList.Add(new FileHash(zipEntry.Name, zf.GetInputStream(zipEntry)));
    //        }
    //        finally
    //        {
    //            if (zf != null)
    //            {
    //                zf.IsStreamOwner = true;
    //                zf.Close();
    //            }
    //        }
    //        return null;
    //    }
    //}
}
