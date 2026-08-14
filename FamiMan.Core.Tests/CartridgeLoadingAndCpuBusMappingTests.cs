using System;
using System.IO;
using FamiMan.Core.Exceptions;
using Xunit;

namespace FamiMan.Core.Tests
{
    /// <summary>
    /// Focused regressions for the cartridge-loader and CPU-bus work that
    /// follows the completed CPU instruction milestone.
    /// </summary>
    public class CartridgeLoadingAndCpuBusMappingTests
    {
        [Fact]
        public void INesLoaderRequiresEntireMagicHeaderIncluding1A()
        {
            byte[] file = CreateRom(prgBanks: 1, chrBanks: 0);
            file[3] = 0x00;

            WithTemporaryRom(file, path =>
            {
                var bus = new Bus();
                Assert.Throws<RomLoadingException>(() => bus.IO.LoadINesRomFile(path));
            });
        }

        [Fact]
        public void INesLoaderSkipsTrainerBeforeReadingPrgRom()
        {
            byte[] file = CreateRom(prgBanks: 1, chrBanks: 0, hasTrainer: true);
            file[16] = 0x99;       // First trainer byte.
            file[16 + 512] = 0x4C; // First PRG-ROM byte.

            WithTemporaryRom(file, path =>
            {
                var bus = new Bus();
                bus.IO.LoadINesRomFile(path);

                Assert.Equal(0x4C, bus.IO.PRGROM[0]);
            });
        }

        [Fact]
        public void INesLoaderStartsChrRomImmediatelyAfterPrgRom()
        {
            byte[] file = CreateRom(prgBanks: 1, chrBanks: 1);
            int chrOffset = 16 + 16_384;
            file[chrOffset - 1] = 0xEE; // Last PRG-ROM byte.
            file[chrOffset] = 0x12;     // First CHR-ROM byte.

            WithTemporaryRom(file, path =>
            {
                var bus = new Bus();
                bus.IO.LoadINesRomFile(path);

                Assert.Equal(0x12, bus.IO.CHRROM[0]);
            });
        }

        [Fact]
        public void CartridgeWithNoChrBanksGetsEightKilobytesOfChrRam()
        {
            byte[] file = CreateRom(prgBanks: 1, chrBanks: 0);

            WithTemporaryRom(file, path =>
            {
                var bus = new Bus();
                bus.IO.LoadINesRomFile(path);

                Assert.Equal(8_192, bus.IO.CHRROM.Length);

                bus.IO.CHRROM[0] = 0x42;
                Assert.Equal(0x42, bus.IO.CHRROM[0]);
            });
        }

        [Fact]
        public void INesLoaderRejectsUnsupportedMapper()
        {
            byte[] file = CreateRom(prgBanks: 1, chrBanks: 1);
            file[6] = 0x10; // Mapper 1: lower mapper nibble is in flags 6 bits 4-7.

            WithTemporaryRom(file, path =>
            {
                var bus = new Bus();
                Assert.Throws<RomLoadingException>(() => bus.IO.LoadINesRomFile(path));
            });
        }

        [Fact]
        public void INesLoaderReportsTruncatedRomAsRomLoadingError()
        {
            byte[] file = CreateRom(prgBanks: 1, chrBanks: 1);
            Array.Resize(ref file, 32);

            WithTemporaryRom(file, path =>
            {
                var bus = new Bus();
                Assert.Throws<RomLoadingException>(() => bus.IO.LoadINesRomFile(path));
            });
        }

        [Fact]
        public void CpuBusMirrorsPpuRegistersEveryEightBytesThrough3FFF()
        {
            var bus = new Bus();

            bus[0x2008] = 0x42; // $2008 is a mirror of PPUCTRL at $2000.

            Assert.Equal(0x42, bus.Ppu.Register.PPUCTRL);
            Assert.Equal(0x42, bus[0x2000]);
        }

        [Fact]
        public void CpuBusRoutes4014ToPpuOamDmaRegister()
        {
            var bus = new Bus();

            bus[0x4014] = 0x02;

            Assert.Equal(0x02, bus.Ppu.Register.Registers[8]);
        }

        private static byte[] CreateRom(byte prgBanks, byte chrBanks, bool hasTrainer = false)
        {
            int trainerLength = hasTrainer ? 512 : 0;
            byte[] file = new byte[
                16 + trainerLength + prgBanks * 16_384 + chrBanks * 8_192];

            file[0] = (byte)'N';
            file[1] = (byte)'E';
            file[2] = (byte)'S';
            file[3] = 0x1A;
            file[4] = prgBanks;
            file[5] = chrBanks;

            if (hasTrainer)
                file[6] |= 0x04;

            return file;
        }

        private static void WithTemporaryRom(byte[] file, Action<string> test)
        {
            string path = Path.GetTempFileName();

            try
            {
                File.WriteAllBytes(path, file);
                test(path);
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}
