using FamiMan.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace FamiMan.Core
{
    public class IO
    {
        private Bus _b;

        private const ushort HEADER_LEN = 16;
        public byte[] PRGROM;
        public byte[] CHRROM;

        public IO(Bus b)
        {
            _b = b;

            // TODO: size?
        }


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
            byte[] file = new byte[new FileInfo(path).Length];
            file = File.ReadAllBytes(path); 

            var headerName = Encoding.Default.GetString(file[0..3]);
            if (headerName != "NES") throw new RomLoadingException("Unexpected header value: " + headerName);

            int prgrom_size = file[4];
            int chrrom_size = file[5];
            PRGROM = new byte[prgrom_size * 16384];
            PRGROM = file[(HEADER_LEN - 1) .. (HEADER_LEN + prgrom_size * 16384 - 1)];

            //if (prgrom_size == 2)
            //{
            //    byte[] temp = new byte[PRGROM.Length];
            //    Array.Copy(PRGROM, temp, PRGROM.Length);
            //    PRGROM = new byte[PRGROM.Length * 2];
            //    Array.Copy(temp, 0, PRGROM, 0, temp.Length);
            //    Array.Copy(temp, 0, PRGROM, temp.Length - 1, temp.Length);
            //}

            CHRROM = new byte[chrrom_size * 8192];
            CHRROM = file[(HEADER_LEN + PRGROM.Length - 1) .. (HEADER_LEN + PRGROM.Length + (chrrom_size * 8192) - 1)];

            var rest = file[(HEADER_LEN + PRGROM.Length + CHRROM.Length - 1)..];

            return new Rom { FileLength = new FileInfo(path).Length, Type = RomType.INES, PRGROM_Size = prgrom_size, CHRROM_Size = chrrom_size };
        }
    }
}
