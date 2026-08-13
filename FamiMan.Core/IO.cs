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
        private int[] _supportedMappers = { 0x00 };
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

            ValidateMagicString(file);

            ValidateSupportedMapper(file);


            int prgrom_size = file[4];
            int chrrom_size = file[5];

            int mirroringMode = file[6] & 0x01;
            if (mirroringMode > 1 || mirroringMode < 0)
                throw new RomLoadingException("Unsupported mirroring mode: " + mirroringMode);
            _b.Ppu.Mirroring = (NametableMirroring) mirroringMode;

            if (file.Length < HEADER_LEN + prgrom_size * 16384 + chrrom_size * 8192)
                throw new RomLoadingException("File is too small for the specified PRGROM and CHRROM sizes");

            bool hasTrainer = (file[6] & 0x04) != 0;

            int dataStart = HEADER_LEN;

            if (hasTrainer)
                dataStart += 512;

            PRGROM = new byte[prgrom_size * 16384];
            PRGROM = file[(dataStart)..(dataStart + prgrom_size * 16384)];

            CHRROM = new byte[chrrom_size * 8192];
            int chrStart = dataStart + PRGROM.Length;
            int chrEnd = chrStart + chrrom_size * 8192;

            CHRROM = chrrom_size == 0
                ? new byte[8_192]
                : file[chrStart..chrEnd];

            return new Rom { FileLength = new FileInfo(path).Length, Type = RomType.INES, PRGROM_Size = prgrom_size, CHRROM_Size = chrrom_size };
        }

        private void ValidateSupportedMapper(byte[] file)
        {
            int mapperNumber =
                (file[6] >> 4) |
                (file[7] & 0xF0);

            if (!_supportedMappers.Contains(mapperNumber))
                throw new RomLoadingException("Unsupported mapper: " + mapperNumber);
        }

        private static void ValidateMagicString(byte[] file)
        {
            bool hasValidMagic =
                file.Length >= 4 &&
                file[0] == (byte)'N' &&
                file[1] == (byte)'E' &&
                file[2] == (byte)'S' &&
                file[3] == 0x1A;

            if (!hasValidMagic)
            {
                throw new RomLoadingException(
                    "Invalid iNES header. Expected NES followed by 0x1A.");
            }
        }
    }
}
