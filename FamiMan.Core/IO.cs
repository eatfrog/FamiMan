using FamiMan.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace FamiMan.Core
{
    public class IO
    {
        private Bus _b;

        private const ushort PRGROM_START = 0x8000;

        public IO(Bus b)
        {
            _b = b;

            // TODO: size?
            PRGROM = new byte[40 * 1024 * 8];
        }

        public byte[] PRGROM;

        public void LoadProgramFromByteArrayToLocation(byte[] program, ushort startLoc) => LoadProgramFromByteArrayToLocation(program, (byte) startLoc);
        public void LoadProgramFromByteArrayToLocation(byte[] program, byte startLoc)
        {
            foreach (byte b in program)
            {
                _b[startLoc] = b;
                startLoc++;
            }
        }

        public void LoadProgramFromHexString(string hexString, byte startLoc)
        {
            hexString = hexString.Replace(" ", "");
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

            LoadProgramFromByteArrayToLocation(data, startLoc);
        }

        public Rom LoadINesRomFile(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException($"Rom file {path} not found", path);
            using (FileStream fs = File.OpenRead(path))
            {
                var buffer = new byte[4096];
                int startLoc = 0;
                int read;
                while ((read = fs.Read(buffer)) != 0)
                {
                    foreach (byte b in buffer[0..read])
                    {
                        PRGROM[startLoc] = b;
                        startLoc++;
                    }
                }
                fs.Close();

            }

            var headerName = Encoding.Default.GetString(PRGROM[0..3]);
            if (headerName != "NES") throw new RomLoadingException("Unexpected header value: " + headerName);

            int prgrom_size = PRGROM[4];
            int chrrom_size = PRGROM[5];

            return new Rom { FileLength = new FileInfo(path).Length, Type = RomType.INES, PRGROM_Size = prgrom_size, CHRROM_Size = chrrom_size };
        }
    }
}
