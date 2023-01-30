namespace FamiMan.Core.Interfaces
{
    public interface IMapper
    {
        public ref byte GetByteAtAddress(ushort address);

        public ref byte[] GetBytesAtAddress(ushort[] address);

        ref byte GetPPUByteAtAddress(ushort index);
    }
}