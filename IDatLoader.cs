using System.Collections.Generic;

namespace RomIdent2
{
    internal interface IDatLoader
    {
        Dictionary<string, Game> AvailableGames { get; set; }
        string DatFilename { get; set; }
        DatHeader Header { get; set; }

        void Process();
    }
}