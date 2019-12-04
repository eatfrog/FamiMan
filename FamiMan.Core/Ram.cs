using System;
using System.Collections.Generic;
using System.Text;

namespace FamiMan.Core
{
    public class Ram
    {
        public static byte[] _ram = new byte[2 * Constants.KB];

        public byte this[ushort index]
        {
            get => _ram[index];
            set => _ram[index] = value;
        }
    }
}
