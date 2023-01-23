namespace FamiMan.Core
{
    public class Rom
    {
        public long FileLength { get; set; }

        // in 16kb units
        public int PRGROM_Size { get; set; }

        // in 8kb units
        public int CHRROM_Size { get; set; }

        public RomType Type { get; set; }
    }

    public enum RomType
    {
        INES
    }
}