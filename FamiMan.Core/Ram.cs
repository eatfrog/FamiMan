using System;
using System.Collections.Generic;
using System.Text;

namespace FamiMan.Core
{
    public class Ram
    {
        public byte[] _ram;

        public Ram(int size)
        {
            _ram = new byte[size];
        }
        

        public byte this[ushort index]
        {
            get => _ram[index];
            set => _ram[index] = value;
        }

        public Span<byte> AsSpan() => _ram;
    }
}
