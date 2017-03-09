using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomIdent2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Reading dat file: {0}", args[0]);
            IDatLoader datFile = new LogiqxLoader(args[0]); //DatLoader(args[0]);

            datFile.Process();
            Console.WriteLine("Games in .dat File: {0}", datFile.AvailableGames.Count);

            Console.WriteLine("Scanning current directory...");
            DirScan f = new DirScan(Directory.GetCurrentDirectory()) { Recurse = false };
            f.PerformScan();
            Console.WriteLine("Files Scanned: {0}", f.FileList.Count);

            Console.WriteLine("Generating List...");
            ScanGames(datFile.AvailableGames, f.FileList);            
            PrintGames(datFile.AvailableGames);
            Console.WriteLine("Done.");
            //HaveGames(datFile.AvailableGames, f.FileList);
            //Console.WriteLine("===================================================\r\nDumping Everything\r\n===================================================");
            //PrintFoundFiles(f.FileList);
            //PrintDatGames(datFile.AvailableGames);
        }

        static void PrintFoundFiles(List<RomModel> fileList)
        {
            foreach(var file in fileList)
            {
                Console.WriteLine("------------------------------------------------------------");
                Console.WriteLine(file.FilePath);
                Console.WriteLine(file.RomFileName);
                Console.WriteLine(file.ZipFileName);
                Console.WriteLine(file.CRC);
                Console.WriteLine(file.MD5);
                Console.WriteLine(file.SHA1);
            }
        }

        static void PrintDatGames(Dictionary<string, Game> fileList)
        {
            foreach (var file in fileList)
            {
                Console.WriteLine("------------------------------------------------------------");
                Console.WriteLine(file.Key);
                Console.WriteLine(file.Value.Name);
                Console.WriteLine(file.Value.Description);
                Console.WriteLine(file.Value.BaseGame);
                foreach(var rom in file.Value.Roms)
                {
                    Console.WriteLine(rom.Filename);
                    Console.WriteLine(rom.BaseRom);
                    Console.WriteLine(rom.CRC);
                    Console.WriteLine(rom.MD5);
                    Console.WriteLine(rom.SHA1);
                    Console.WriteLine(rom.Size.ToString());
                }
            }
        }

        static void HaveGames(Dictionary<string, Game> gameList, List<RomModel> fileList)
        {
            foreach (var game in gameList)
            {
                Console.WriteLine("------------------------------------------------------------");
                Console.Write("Checking game: ");
                Console.WriteLine(game.Key);
                Console.WriteLine("Name: " + game.Value.Name);
                Console.WriteLine("Description: " + game.Value.Description);
                Console.WriteLine("Base Game: " + game.Value.BaseGame);
                foreach (var rom in game.Value.Roms)
                {
                    Console.WriteLine("====");
                    Console.WriteLine("Filename: " + rom.Filename);
                    Console.WriteLine("Base Rom: " + rom.BaseRom);

                    var matches = fileList.Where(x => x.SHA1.Equals(rom.SHA1, StringComparison.InvariantCultureIgnoreCase) || 
                        x.MD5.Equals(rom.MD5, StringComparison.InvariantCultureIgnoreCase) ||
                        x.CRC.Equals(rom.CRC, StringComparison.InvariantCultureIgnoreCase));
                    if (rom.FoundRom == null)
                        rom.FoundRom = new List<RomModel>();
                    rom.FoundRom.AddRange(matches);

                    foreach (var file in matches)
                    {
                        Console.WriteLine("++++");
                        Console.Write("Found in Directory: ");
                        Console.WriteLine(file.FilePath);
                        if (!string.IsNullOrEmpty(file.ZipFileName))
                            Console.WriteLine("[" + file.RomFileName + "] -> " + file.ZipFileName);
                        else
                            Console.WriteLine(file.RomFileName);
                    }
                }
            }
        }

        static void ScanGames(Dictionary<string, Game> gameList, List<RomModel> fileList)
        {
            foreach (var game in gameList)
            {
                foreach (var rom in game.Value.Roms)
                {
                    var matches = fileList.Where(x => x.SHA1.Equals(rom.SHA1, StringComparison.InvariantCultureIgnoreCase) ||
                        x.MD5.Equals(rom.MD5, StringComparison.InvariantCultureIgnoreCase) ||
                        x.CRC.Equals(rom.CRC, StringComparison.InvariantCultureIgnoreCase));
                    if (rom.FoundRom == null)
                        rom.FoundRom = new List<RomModel>();
                    rom.FoundRom.AddRange(matches);
                }
            }
        }

        static void PrintGames(Dictionary<string, Game> gameList)
        {
            double totalGamesFound = 0;
            foreach (var game in gameList)
            {
                var matchedGames = game.Value.Roms.Where(x => x.FoundRom != null && x.FoundRom.Count > 0).ToList();
                if (matchedGames.Count > 0)
                {
                    totalGamesFound += matchedGames.Count;
                    Console.WriteLine("------------------------------------------------------------");
                    Console.WriteLine("Game: " + game.Value.Name);
                    Console.WriteLine("Description: " + game.Value.Description);
                    Console.WriteLine("Base Game: " + game.Value.BaseGame);
                    foreach (var rom in game.Value.Roms)
                    {
                        Console.WriteLine("====");
                        Console.WriteLine("Filename: " + rom.Filename);
                        Console.WriteLine("Base Rom: " + rom.BaseRom);
                        foreach (var file in rom.FoundRom)
                        {
                            Console.WriteLine("++++");
                            Console.Write("Found in Directory: ");
                            Console.WriteLine(file.FilePath);
                            if (!string.IsNullOrEmpty(file.ZipFileName))
                                Console.WriteLine("[" + file.RomFileName + "] -> " + file.ZipFileName);
                            else
                                Console.WriteLine(file.RomFileName);
                        }
                    }
                }
            }
            Console.WriteLine("============================================================");
            Console.WriteLine("Total Games Found: " + totalGamesFound.ToString());
        }
    }

}
