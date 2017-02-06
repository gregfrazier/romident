using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomIdent2
{
    internal enum ChecksumType
    {
        SHA1,
        MD5,
        CRC32
    }

    internal class RomModel
    {
        public string ZipFileName { get; set; }
        public string RomFileName { get; set; }
        public string FilePath { get; set; }

        public int Size { get; set; }
        public string CRC { get; set; }
        public string MD5 { get; set; }
        public string SHA1 { get; set; }
    }

    internal class DatModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
    }

    internal class DatHeader
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
    }

    internal class Rom
    {
        public string Filename { get; set; }
        public int Size { get; set; }
        public string CRC { get; set; }
        public string MD5 { get; set; }
        public string SHA1 { get; set; }
        public string BaseRom { get; set; }

        public ChecksumType HighestChecksum
        {
            get
            {
                if (!String.IsNullOrEmpty(SHA1))
                    return ChecksumType.SHA1;
                if (!String.IsNullOrEmpty(MD5))
                    return ChecksumType.MD5;
                return ChecksumType.CRC32;
            }
        }

        public List<RomModel> FoundRom { get; set; }
    }

    internal class Game
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string BaseGame { get; set; }
        public List<Rom> Roms { get; set; }
    }
}
