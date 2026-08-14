using System;
using System.IO;
using Xunit;

namespace FamiMan.Core.Tests
{
    public class INesAndNromMappingTests
    {
        [Fact]
        public void INesLoaderStartsPrgRomAfterEntireHeader()
        {
            string path = Path.GetTempFileName();

            try
            {
                byte[] file = CreateRom(prgBanks: 1);
                file[16] = 0x4C;
                File.WriteAllBytes(path, file);

                var bus = new Bus();
                bus.IO.LoadINesRomFile(path);

                Assert.Equal(0x4C, bus.IO.PRGROM[0]);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void NRom128MirrorsSinglePrgBankAt8000AndC000()
        {
            var bus = new Bus();
            bus.IO.PRGROM = new byte[16_384];
            bus.IO.CHRROM = Array.Empty<byte>();
            bus.IO.PRGROM[0] = 0x4C;

            Assert.Equal(0x4C, bus[0x8000]);
            Assert.Equal(0x4C, bus[0xC000]);
        }

        [Fact]
        public void NRom256MapsEntirePrgRomFrom8000ToFFFF()
        {
            var bus = new Bus();
            bus.IO.PRGROM = new byte[32_768];
            bus.IO.CHRROM = Array.Empty<byte>();
            bus.IO.PRGROM[0] = 0x11;
            bus.IO.PRGROM[^1] = 0x22;

            Assert.Equal(0x11, bus[0x8000]);
            Assert.Equal(0x22, bus[0xFFFF]);
        }

        private static byte[] CreateRom(byte prgBanks)
        {
            byte[] file = new byte[16 + prgBanks * 16_384];
            file[0] = (byte)'N';
            file[1] = (byte)'E';
            file[2] = (byte)'S';
            file[3] = 0x1A;
            file[4] = prgBanks;
            file[5] = 0;
            return file;
        }
    }
}
