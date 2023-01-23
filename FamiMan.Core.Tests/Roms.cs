using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FamiMan.Core.Tests
{
    public class Roms
    {
        private Bus _b;
        private Cpu _c;
        private IO _io;

        public Roms()
        {
            _b = new Bus();
            _c = _b.Cpu;
            _io = _b.IO;
        }

        [Fact]
        public void LoadINesRom()
        {
            var rom = _io.LoadINesRomFile(Directory.GetCurrentDirectory() + "\\test.nes");
            Assert.NotNull(rom);
        }
    }
}
