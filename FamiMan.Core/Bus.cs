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
            get
            {
                // Ram = $0000 -$07FF
                if (index >= 0 && index < 0x7FF)
                    return Ram[index];
                else
                    throw new NotImplementedException("Not done");
            }
            set
            {
                if (index >= 0 && index < 0x7FF)
                    Ram[index] = value;
                else
                    throw new NotImplementedException("Not done");
            }
        }
    }
}
