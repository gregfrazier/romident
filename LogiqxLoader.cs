using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RomIdent2
{
    internal class LogiqxLoader : IDatLoader
    {
        public Dictionary<string, Game> AvailableGames { get; set; }
        public string DatFilename { get; set; }
        public DatHeader Header { get; set; }

        public LogiqxLoader(string datfile)
        {
            DatFilename = datfile;
            AvailableGames = new Dictionary<string, Game>();
            Header = new DatHeader();
        }

        public void Process()
        {
            var datXML = XDocument.Load(new StreamReader(DatFilename));
            foreach(var node in datXML.Elements().Nodes())
            {
                var element = (node as XElement);
                if(element != null)
                {
                    if(element.Name.ToString().Equals("header", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var y = element.Nodes().Select(x => {
                            var headerNode = (x as XElement);
                            return new { Name = headerNode?.Name?.ToString(), Value = headerNode?.Value };
                        });
                        
                        Header = new DatHeader
                        {
                            Name = y.Where(x => x.Name.Equals("name", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault()?.Value,
                            Author = y.Where(x => x.Name.Equals("author", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault()?.Value,
                            Category = y.Where(x => x.Name.Equals("category", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault()?.Value,
                            Description = y.Where(x => x.Name.Equals("description", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault()?.Value,
                            Version = y.Where(x => x.Name.Equals("version", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault()?.Value
                        };

                    }
                    else if(element.Name.ToString().Equals("game", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var y = element.Nodes().Where(x => (x as XElement).Name.ToString().Equals("year", StringComparison.InvariantCultureIgnoreCase) &&
                            (x as XElement).Name.ToString().Equals("description", StringComparison.InvariantCultureIgnoreCase) &&
                            (x as XElement).Name.ToString().Equals("manufacturer", StringComparison.InvariantCultureIgnoreCase)
                            ).Select(x => new { Name = (x as XElement).Name?.ToString(), Value = (x as XElement)?.Value }).ToDictionary(x => x.Name.ToLower());

                        var currentGame = new Game
                        {
                            Name = element.Attribute("name")?.Value,
                            BaseGame = element.Attribute("romof")?.Value,
                            CloneGame = element.Attribute("cloneof")?.Value,
                            Year = y.ContainsKey("year") ? y["year"].Value : string.Empty,
                            Description = y.ContainsKey("description") ? y["description"].Value : string.Empty,
                            Manufacturer = y.ContainsKey("manufacturer") ? y["manufacturer"].Value : string.Empty
                        };

                        var roms = element.Nodes().Where(x => (x as XElement).Name.ToString().Equals("rom", StringComparison.InvariantCultureIgnoreCase))
                           .Select(x =>
                           {
                               var r = (x as XElement);
                               return new Rom
                               {
                                   Filename = r.Attribute("name")?.Value,
                                   BaseRom = r.Attribute("merge")?.Value,
                                   CRC = r.Attribute("crc")?.Value,
                                   MD5 = r.Attribute("md5")?.Value,
                                   SHA1 = r.Attribute("sha1")?.Value,
                                   Size = Convert.ToInt32(r.Attribute("size")?.Value ?? "0")
                               };
                           }).ToList();
                        currentGame.Roms = roms;

                        var disks = element.Nodes().Where(x => (x as XElement).Name.ToString().Equals("disk", StringComparison.InvariantCultureIgnoreCase))
                           .Select(x =>
                           {
                               var r = (x as XElement);
                               return new Disk
                               {
                                   Filename = r.Attribute("name")?.Value,
                                   BaseRom = r.Attribute("merge")?.Value,
                                   CRC = r.Attribute("crc")?.Value,
                                   MD5 = r.Attribute("md5")?.Value,
                                   SHA1 = r.Attribute("sha1")?.Value
                               };
                           }).ToList();
                        currentGame.Disks = disks;

                        var samples = element.Nodes().Where(x => (x as XElement).Name.ToString().Equals("sample", StringComparison.InvariantCultureIgnoreCase))
                           .Select(x => (x as XElement)?.Attribute("name")?.Value).ToList();
                        currentGame.Samples = samples;

                        if (!AvailableGames.ContainsKey(currentGame.Name))
                            AvailableGames.Add(currentGame.Name, currentGame);
                    }
                }
            }
        }
    }
}
