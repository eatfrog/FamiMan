using System;
using System.Collections.Generic;
using System.Text;

namespace FamiMan.Core
{
    public class Bus
    {
        public Bus()
        {
            Ram = new Ram();
        }

        public Ram Ram { get; set; }
        public Cpu Cpu { get; set; }

        public byte this[ushort index]
        {
            get => Ram[(byte)index]; // TODO
            set => Ram[(byte)index] = value;
        }
    }
}
