using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace FamiMan.Core
{
    public class IO
    {
        private Bus _b;

        public IO(Bus b)
        {
            _b = b;
        }

        public void LoadProgramFromByteArray(byte[] program, byte startLoc)
        {
            foreach (byte b in program)
            {
                _b[startLoc] = b;
                startLoc++;
            }
        }

        public void LoadProgramFromHexString(string hexString, byte startLoc)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
            }

            byte[] data = new byte[hexString.Length / 2];
            for (int index = 0; index < data.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                data[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            LoadProgramFromByteArray(data, startLoc);
        }
    }
}
