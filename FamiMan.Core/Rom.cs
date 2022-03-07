namespace FamiMan.Core
{
    public class Rom
    {
        public long Length { get; set; }
        public RomType Type { get; set; }
    }

    public enum RomType
    {
        INES
    }
}