namespace FamiMan.Core.Interfaces
{
    public interface IMapper
    {
        byte ReadCpu(ushort address);
        void WriteCpu(ushort address, byte value);

        byte ReadPpu(ushort address);
        void WritePpu(ushort address, byte value);

        public byte GetByteAtAddress(ushort address);

        public byte[] GetBytesAtAddress(ushort[] address);

        byte GetPPUByteAtAddress(ushort index);
    }
}