using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RomIdent2
{
    internal class DatLoader : IDatLoader
    {
        public string DatFilename { get; set; }
        public DatHeader Header { get; set;}
        public Dictionary<string, Game> AvailableGames { get; set; }

        public DatLoader(string datfile)
        {
            DatFilename = datfile;
            AvailableGames = new Dictionary<string, Game>();
            Header = new DatHeader();
        }

        public void Process()
        {

            string HeaderRegex = @"^\s*clrmamepro\s*\(.*$";
            string GameRegex = @"^\s*game\s*\(.*$";

            // Check line type
            using (StreamReader str = new StreamReader(DatFilename))
            {
                while (str.Peek() > -1)
                {
                    string line = str.ReadLine();
                    if (line == string.Empty)
                    {
                        continue;
                    }
                    else if (Regex.IsMatch(line, HeaderRegex))
                    {
                        string y = "g";
                        while (y != string.Empty && (str.Peek() > -1))
                        {
                            y = str.ReadLine();
                            line += "\r\n" + y;
                        }
                        SetHeader(line);
                    }
                    else if (Regex.IsMatch(line, GameRegex))
                    {
                        string y = "g";
                        while (y != string.Empty && (str.Peek() > -1))
                        {
                            y = str.ReadLine();
                            line += "\r\n" + y;
                        }
                        AddGame(line);
                    }
                }
            }
        }

        private void SetHeader(string h)
        {
            string namergx = @"^\s*name\s+\""?(((?<=\"")[^\""]*)|(?<!\"")\S*)\""?\s*$";
            //string descrgx = @"^\s*description\s+\""?(((?<=\"")[^\""]*)|(?<!\"")\S*)\""?\s*$";
            Match mtx = Regex.Match(h, namergx, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (mtx.Success)
                Header.Name = mtx.Groups[1].Value;
        }

        private void AddGame(string game)
        {
            string namergx = @"^\s*name\s+\""?(((?<=\"")[^\""]*)|(?<!\"")\S*)\""?\s*$";
            string descrgx = @"^\s*description\s+\""?(((?<=\"")[^\""]*)|(?<!\"")\S*)\""?\s*$";
            string romof = @"^\s*romof\s+\""?(((?<=\"")[^\""]*)|(?<!\"")\S*)\""?\s*$";

            string romrgx = @"^\s*rom\s*\(.*\)";
            string filenamergx = @"^\s*rom\s*\(.*\sname\s+\""?(((?<=\"")[^\""]*)|(?<!\"")\S*)\""?";
            string mergergx = @"^\s*rom\s*\(.*\smerge\s+\""?(((?<=\"")[^\""]*)|(?<!\"")\S*)\""?";
            string sizergx = @"^\s*rom\s*\(.*\ssize\s+\""?([0-9]*)\""?";
            string crcrgx = @"^\s*rom\s*\(.*\scrc\s+\""?([0-9a-fA-F]*)\""?";
            string md5rgx = @"^\s*rom\s*\(.*\smd5\s+\""?([0-9a-fA-F]*)\""?";
            string sha1rgx = @"^\s*rom\s*\(.*\ssha1\s+\""?([0-9a-fA-F]*)\""?";

            Match mtx;
            Rom r;
            Game n = new Game();
            n.Roms = new List<Rom>();

            // Game name
            mtx = Regex.Match(game, namergx, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (mtx.Success)
                n.Name = mtx.Groups[1].Value;

            // Game description
            mtx = Regex.Match(game, descrgx, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (mtx.Success)
                n.Description = mtx.Groups[1].Value;

            // Rom Of (Base Game)
            mtx = Regex.Match(game, romof, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (mtx.Success)
                n.BaseGame = mtx.Groups[1].Value;

            // There can be multiple rom definitions per game
            foreach (Match mtu in Regex.Matches(game, romrgx, RegexOptions.IgnoreCase | RegexOptions.Multiline))
            {
                r = new Rom();
                
                // Proper Filename
                mtx = Regex.Match(mtu.Value, filenamergx, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (mtx.Success)
                    r.Filename = mtx.Groups[1].Value;

                // Base ROM Exists
                mtx = Regex.Match(mtu.Value, mergergx, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (mtx.Success)
                    r.BaseRom = mtx.Groups[1].Value;
                
                // Official Filesize
                mtx = Regex.Match(mtu.Value, sizergx, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (mtx.Success)
                    r.Size = Convert.ToInt32(mtx.Groups[1].Value);
                
                // CRC Checksum
                mtx = Regex.Match(mtu.Value, crcrgx, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (mtx.Success)
                    r.CRC = mtx.Groups[1].Value.ToLower();
                
                // MD5 Checksum
                mtx = Regex.Match(mtu.Value, md5rgx, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (mtx.Success)
                    r.MD5 = mtx.Groups[1].Value;
                
                // SHA1 Checksum
                mtx = Regex.Match(mtu.Value, sha1rgx, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (mtx.Success)
                    r.SHA1 = mtx.Groups[1].Value;
                
                n.Roms.Add(r);
            }

            if (!AvailableGames.ContainsKey(n.Name))
                AvailableGames.Add(n.Name, n);
        }
    }
}
