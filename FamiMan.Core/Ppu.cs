using FamiMan.Core.Interfaces;
using System;

namespace FamiMan.Core
{

    // Picture processing unit
    public class Ppu
    {
        private Bus _b;
        private Ram _r;

        private readonly byte[] _nametableRam = new byte[0x800];
        private readonly byte[] _paletteRam = new byte[0x20];
        private readonly byte[] _oam = new byte[0x100];

        public const ushort PPUCTRL_ADDR    = 0x2000;
        public const ushort PPUMASK_ADDR    = 0x2001;
        public const ushort PPUSTATUS_ADDR  = 0x2002;
        public const ushort OAMADDR_ADDR    = 0x2003;
        public const ushort PPUSCROLL_ADDR  = 0x2005;
        public const ushort PPUADDR_ADDR    = 0x2006;
        public const ushort PPUDATA_ADDR    = 0x2007;
        public const ushort OAMDMA_ADDR     = 0x4014;

        public const ushort NAMETABLE_START = 0x2000;
        public const ushort NAMETABLE_ATTR_START = 0x23C0;

        public const ushort PALETTE_START = 0x3F00;

        public Ppu(Bus b)
        {
            _b = b;
            _r = new Ram(16 * 1024);
            Register = new PPURegister(this)
            {
                // Vblank always on
                PPUSTATUS = 0b10000000 //128
            };

            /* The PPU addresses a 16kB space, $0000-3FFF, 
             * completely separate from the CPU's address bus. 
             * It is either directly accessed by the PPU itself, 
             * or via the CPU with memory mapped registers at $2006 and $2007.
             * The NES has 2kB of RAM dedicated to the PPU, 
             * normally mapped to the nametable address space from $2000-2FFF, 
             * but this can be rerouted through custom cartridge wiring. */
        }

        /*
         * PPUCTRL	    $2000	VPHB SINN	NMI enable (V), PPU master/slave (P), sprite height (H), background tile select (B), sprite tile select (S), increment mode (I), nametable select (NN)
         * PPUMASK	    $2001	BGRs bMmG	color emphasis (BGR), sprite enable (s), background enable (b), sprite left column enable (M), background left column enable (m), greyscale (G)
         * PPUSTATUS	$2002	VSO- ----	vblank (V), sprite 0 hit (S), sprite overflow (O); read resets write pair for $2005/$2006
         * OAMADDR	    $2003	aaaa aaaa	OAM read/write address
         * OAMDATA	    $2004	dddd dddd	OAM data read/write
         * PPUSCROLL    $2005	xxxx xxxx	fine scroll position (two writes: X scroll, Y scroll)
         * PPUADDR	    $2006	aaaa aaaa	PPU read/write address (two writes: most significant byte, least significant byte)
         * PPUDATA	    $2007	dddd dddd	PPU data read/write
         * OAMDMA	    $4014	aaaa aaaa	OAM DMA high address
        */
        public PPURegister Register;

        public NametableMirroring Mirroring { get; set; }

        // Exposed so timing behavior can be learned and tested directly.
        public int Cycle { get; private set; }
        public int Scanline { get; private set; }
        public bool FrameComplete { get; private set; }
        public byte ScrollX { get; private set; }

        private byte _fineX;

        public byte ScrollY { get; private set; }

        private (byte X, byte Y, byte PPUCTRL)[] _scanlineScrolls = new (byte X, byte Y, byte PPUCTRL)[240];
        private bool[] _scanlineCaptured = new bool[240];
        private int[] _bgNametableAtScanline = new int[240];

        /// <summary>
        /// Returns the background state used for one visible scanline. This is
        /// intended for tests and debug tooling; reading it has no PPU side effects.
        /// </summary>
        public (byte X, byte Y, byte PpuCtrl, bool Captured) GetScrollStateForScanline(int scanline)
        {
            if (scanline < 0 || scanline >= 240)
                throw new ArgumentOutOfRangeException(nameof(scanline));

            if (_scanlineCaptured[scanline])
            {
                var captured = _scanlineScrolls[scanline];
                return (captured.X, captured.Y, captured.PPUCTRL, true);
            }

            return (ScrollX, ScrollY, Register.PPUCTRL, false);
        }

        /// <summary>
        /// Initializes one byte of sprite OAM independently of the CPU-visible
        /// OAM registers. Intended for focused sprite tests and debugging.
        /// </summary>
        public void SetOamByte(byte address, byte value)
        {
            _oam[address] = value;
        }

        public byte ReadOamByte(byte address)
        {
            return _oam[address];
        }

        public byte[] ReadOamBytes(int from, int to) => _oam[from..to];

        /// <summary>
        /// Reads the PPU's separate $0000-$3FFF address space.
        /// This is intentionally separate from the CPU-visible registers at
        /// $2000-$2007 handled by ReadCpuRegister().
        /// </summary>
        public byte ReadPpuMemory(ushort address)
        {
            // We only have a 14bit address bus so $0000–$3FFF
            // After that it just wraps around to $0000 again. So we can just mask the address to 14 bits.
            address &= 0x3FFF;
            if (address <= 0x1FFF)
            {
                return _b.IO.CHRROM[address];
            }
            else if (address <= 0x3EFF)
            {
                int nametableAddress = GetNametableIndex(address);
                return _nametableRam[nametableAddress];
            }
            else if (address <= 0x3FFF)
            {
                int paletteAddress = (address - 0x3F00) % 0x20;
                // Special aliases for the background color and sprite color
                if (paletteAddress is 0x10 or 0x14 or 0x18 or 0x1C)
                    paletteAddress -= 0x10;
                return _paletteRam[paletteAddress];
            }
            else
            {
                throw new InvalidOperationException("Invalid memory address access in PPU");
            }
        }

        /// <summary>
        /// Writes the PPU's separate $0000-$3FFF address space.
        /// </summary>
        public void WritePpuMemory(ushort address, byte value)
        {
            /*
             *  $0000–$1FFF  CHR-ROM or CHR-RAM
                $2000–$3EFF  nametable memory and mirrors
                    $2000–$23BF  tile numbers
                    $23C0–$23FF  attribute bytes
                $3F00–$3FFF  palette memory and mirrors
            */
            address &= 0x3FFF;
            address = (ushort)(address % 0x4000);
            if (address <= 0x1FFF)
            {
                _b.IO.CHRROM[address] = value;
                return;
            }
            else if (address <= 0x3EFF)
            {
                int nametableAddress = GetNametableIndex(address);
                
                _nametableRam[nametableAddress] = value;
                return;
            }
            else if (address <= 0x3FFF)
            {
                int paletteAddress = (address - 0x3F00) % 0x20;
                // Special aliases for the background color and sprite color
                if (paletteAddress is 0x10 or 0x14 or 0x18 or 0x1C)
                    paletteAddress -= 0x10;
                _paletteRam[paletteAddress] = value;
                return;
            }
            else
            {
                throw new InvalidOperationException("Invalid memory address access in PPU");
            }
        }

        private int GetNametableIndex(ushort address)
        {
            // First collapse $3000-$3EFF onto $2000-$2EFF.
            int logicalOffset = (address - 0x2000) % 0x1000;

            // Each logical nametable occupies $400 bytes.
            // So which table and what offset within that table?
            int table = logicalOffset / 0x400;
            int offsetWithinTable = logicalOffset % 0x400;

            // We have two actual tables but four logical tables
            int physicalTable = Mirroring switch
            {
                NametableMirroring.Vertical => table % 2,   // 0 1 0 1
                NametableMirroring.Horizontal => table / 2, // 0 0 1 1
                _ => throw new InvalidOperationException()
            };

            // table 0 or 1 + the offset within that table gives us the final index into the nametable RAM.
            return physicalTable * 0x400 + offsetWithinTable;
        }

        private byte _ppuDataReadBuffer;

        public byte ReadCpuRegister(ushort address)
        {
            if (address == PPUSTATUS_ADDR) 
            {
                _expectingFirstWrite = true;
                byte result = Register.Registers[PPURegister.PPUSTATUS_IDX];
                Register.Registers[PPURegister.PPUSTATUS_IDX] &= 0x7F;
                return result;
            }
            else if (address == PPUCTRL_ADDR)
            {
                return Register.Registers[PPURegister.PPUCTRL_IDX];
            }
            else if (address == PPUMASK_ADDR)
            {
                return Register.Registers[PPURegister.PPUMASK_IDX];
            }
            else if (address == PPUDATA_ADDR)
            {
                byte result;
                if (_ppuAddressV <= 0x3EFF)
                {
                     result = _ppuDataReadBuffer;
                    _ppuDataReadBuffer = ReadPpuMemory(_ppuAddressV);
                }
                else
                {
                    result = ReadPpuMemory(_ppuAddressV);
                }
                IncrementPpuAddress();
                return result;
            }
            else
            {
                throw new InvalidOperationException("Invalid memory address access in PPU");
            }
        }

        ushort _ppuAddressV; // actual currently request PPU address
        ushort _ppuAddressT; // temporary while being written to

        bool _expectingFirstWrite = true;
        private int _backgroundNametable; // Current bg nametable idx
        

        public void WriteCpuRegister(ushort index, byte value)
        {
            if (index == PPUSTATUS_ADDR) 
            {
                Register.Registers[PPURegister.PPUSTATUS_IDX] = value;
            }
            else if (index == PPUCTRL_ADDR)
            {
                Register.Registers[PPURegister.PPUCTRL_IDX] = value;
                _backgroundNametable = value & 0x03;
            }
            else if (index == PPUMASK_ADDR)
            {
                Register.Registers[PPURegister.PPUMASK_IDX] = value;
            }
            else if (index == OAMADDR_ADDR)
            {
                // Set the destination position in OAM
                Register.OAMADDR = value;
            }
            else if (index == PPUSCROLL_ADDR)
            {
                if (_expectingFirstWrite)
                {
                    ScrollX = value;

                    // Bottom three bits select pixel 0-7 within the tile.
                    _fineX = (byte)(value & 0x07);

                    // Remaining five bits select tile column 0-31.
                    int coarseX = value >> 3;

                    // Replace only T's coarse-X bits 0–4.
                    _ppuAddressT = (ushort)((_ppuAddressT & ~0x001F) | coarseX);
                }
                else
                    ScrollY = value;

                _expectingFirstWrite = !_expectingFirstWrite;
            }
            else if (index == PPUADDR_ADDR)
            {
                WritePpuAddress(value);
            }
            else if (index == PPUDATA_ADDR)
            {
                WritePpuMemory(_ppuAddressV, value);
                IncrementPpuAddress();

            }
            else
            {
                throw new InvalidOperationException(
                    $"Invalid write to CPU-visible PPU register ${index:X4}");

            }
        }

        private void IncrementPpuAddress()
        {
            if ((Register.PPUCTRL & 0x04) != 0)
                _ppuAddressV += 32;  // Write down column
            else
                _ppuAddressV++;      // Write across row
        }

        private void WritePpuAddress(byte value)
        {
            if (_expectingFirstWrite)
            {
                // Only six address bits are meaningful because PPU addresses are 14-bit.
                // We got high bytes so move the value into the high byte of the 16-bit address and clear the low byte.
                _ppuAddressT = (ushort)((value & 0x3F) << 8);
                _expectingFirstWrite = false;
            }
            else
            {
                // Bitwise OR to combine the high byte we already have with the low byte we just got.
                _ppuAddressT |= value;

                _ppuAddressV = _ppuAddressT;

                int coarseX = _ppuAddressV & 0x001F;

                // Convert the tile column back into a pixel position.
                // Preserve fine X from the earlier PPUSCROLL write.
                ScrollX = (byte)((coarseX << 3) | _fineX);

                _backgroundNametable = (_ppuAddressV >> 10) & 0x03;

                _expectingFirstWrite = true;
            }
        }

        /// <summary>
        /// Returns the NES palette value for one background pixel.
        /// </summary>
        public byte GetBackgroundPixel(int x, int y)
        {
            byte colorIndex = GetBackgroundColorIndex(x, y);

            int coarseX = _ppuAddressV & 0x001F;
            int coarseY = (_ppuAddressV >> 5) & 0x001F;
            int nametable = (_ppuAddressV >> 10) & 0x03;
            int fineY = (_ppuAddressV >> 12) & 0x07;

            // Palette attributes use the same scrolled background coordinate
            // as the tile/pattern lookup.
            var scroll = _scanlineCaptured[y]
                        ? _scanlineScrolls[y]
                        : (X: ScrollX, Y: ScrollY, PPUCTRL: Register.Registers[PPURegister.PPUCTRL_IDX]);
            int scrolledX = x + scroll.X;
            int scrolledY = y + scroll.Y;
            byte palette = GetBackgroundPaletteNumber(scrolledX, scrolledY);

            return GetBackgroundPaletteValue(palette, colorIndex);
        }

        /// <summary>
        /// Returns the start of the CHR pattern table selected for background
        /// tiles by bit 4 of PPUCTRL.
        /// </summary>
        public ushort GetBackgroundPatternTableBase()
        {
            bool bit4IsSet = (Register.PPUCTRL & 0x10) != 0;
            return (ushort)((bit4IsSet ? 0x1000 : 0));
        }

        /// <summary>
        /// Returns the first CHR address belonging to a background tile.
        /// </summary>
        public ushort GetBackgroundTileAddress(byte tileNumber)
        {
            // a tile is 8x8px, and each pixel is 2 bits, so each tile takes 16 bytes in the CHR pattern table.
            return (ushort)(GetBackgroundPatternTableBase() + (tileNumber * 16));
        }

        /// <summary>
        /// Returns the CHR pattern-table base selected for 8x8 sprites by
        /// PPUCTRL bit 3.
        /// </summary>
        public ushort GetSpritePatternTableBase()
        {
            bool bit3IsSet = (Register.PPUCTRL & 0x08) != 0;
            return (ushort)((bit3IsSet ? 0x1000 : 0));
        }

        /// <summary>
        /// Returns the first CHR address belonging to an 8x8 sprite tile.
        /// </summary>
        public ushort GetSpriteTileAddress(byte tileNumber)
        {
            return (ushort)(GetSpritePatternTableBase() + (tileNumber * 16));
        }

        /// <summary>
        /// Decodes one pixel inside an 8x8 sprite tile to color index 0-3.
        /// The coordinates are local to the tile.
        /// </summary>
        public byte GetSpriteTilePixelColorIndex(byte tileNumber, int x, int y)
        {
            ushort tileStart = GetSpriteTileAddress(tileNumber);
            return DecodeTilePixelColorIndex(tileStart, x, y);
        }

        /// <summary>
        /// Returns one sprite's raw color index at a screen coordinate. Zero
        /// means either outside the sprite rectangle or a transparent pixel.
        /// </summary>
        public byte GetSpritePixelColorIndex(int spriteIndex, int screenX, int screenY)
        {
            byte oamAddr = (byte)(spriteIndex * 4);
            byte[] oamBytes = ReadOamBytes(oamAddr, oamAddr + 4);
            int spriteX = oamBytes[3];
            int spriteY = oamBytes[0] + 1;
            int localX = screenX - spriteX;
            int localY = screenY - spriteY;

            byte attributes = oamBytes[2];
            bool horizontallyFlipped = (attributes & 0x40) != 0;
            if (horizontallyFlipped)
                localX = 7 - localX;

            bool verticallyFlipped = (attributes & 0x80) != 0;
            if (verticallyFlipped)
            {
                localY = 7 - localY;
            }

            if (localX < 0 || localY < 0 || localX > 7 || localY > 7)
                return 0;

            byte tileNumber = oamBytes[1];
            return GetSpriteTilePixelColorIndex(tileNumber, localX, localY);
        }

        /// <summary>
        /// Returns the background pattern color index before palette lookup.
        /// Sprite-zero hit needs opacity (index 0 versus 1-3), not an RGB or
        /// NES system-palette value.
        /// </summary>
        public byte GetBackgroundColorIndex(int x, int y)
        {
            // Screen coordinates and background coordinates differ when the
            // CPU has written a scroll position through PPUSCROLL.
            var scroll = _scanlineCaptured[y]
                        ? _scanlineScrolls[y]
                        : (X: ScrollX, Y: ScrollY, Register.PPUCTRL);

            int scrolledX = x + scroll.X;
            int scrolledY = y + scroll.Y;

            byte tileNumber = GetNametableTileNumber(scrolledX, scrolledY);
            int tileX = scrolledX % 8;
            int tileY = scrolledY % 8;

            return GetTilePixelColorIndex(tileNumber, tileX, tileY);
        }

        /// <summary>
        /// Returns the tile number selected by the nametable for a screen pixel.
        /// </summary>
        public byte GetNametableTileNumber(int x, int y)
        {
            int selectedTable = GetNametableIdx(x, y);
            int baseAddr = selectedTable * 0x400;

            // where in the actual nametable are we? 0-255, 0-239 since each nametable is 256x240px
            int localX = x % 256;
            int localY = y % 240;

            // which column of the nametable are we in? 0-31 since each column is 8px
            int tileColumn = localX / 8;

            // Each tile is 8x8px
            int tileRow = localY / 8;

            // nametable is a grid containing 32 tile numbers per row
            return ReadPpuMemory((ushort)(NAMETABLE_START + baseAddr + (tileRow * 32) + tileColumn));
        }

        private int GetNametableIdx(int x, int y)
        {
            // PPUCTRL bit 0 and 1 select which nametable starts at top left
            /*
             *  Bits 1–0	Starting nametable
                00	        $2000
                01	        $2400
                10	        $2800
                11	        $2C00
            */

            int baseTable = _scanlineCaptured[y] ? _bgNametableAtScanline[y] : _backgroundNametable;
            
            // Convert table number 0–3 into a row and column.
            // The four nametables represent a 2x2 grid, so we can use modulo and division to get the row and column.
            int baseTableColumn = baseTable % 2;
            int baseTableRow = baseTable / 2;

            // are we in left or right nametable of that row
            int nametableOffsetX = x / 256;

            // are we in top or bottom nametable of that column
            int nametableOffsetY = y / 240;

            // Move to the table on the right when offset is 1 (with wrap around)
            int selectedTableColumn = (baseTableColumn + nametableOffsetX) % 2;

            // Move to the table downwards if offset is 1 (with wrap around)
            int selectedTableRow = (baseTableRow + nametableOffsetY) % 2;

            int selectedTable = selectedTableRow * 2 + selectedTableColumn;
            return selectedTable;
        }

        /// <summary>
        /// Decodes the two CHR bitplanes into a color index from 0 through 3.
        /// The supplied coordinates are positions within an 8x8 tile.
        /// </summary>
        public byte GetTilePixelColorIndex(byte tileNumber, int x, int y)
        {
            ushort tileStart = GetBackgroundTileAddress(tileNumber);
            return DecodeTilePixelColorIndex(tileStart, x, y);
        }

        /// <summary>
        /// Decodes one pixel from a tile whose CHR start address has already
        /// been selected. Background and 8x8 sprite tiles share this format.
        /// </summary>
        private byte DecodeTilePixelColorIndex(ushort tileStart, int x, int y)
        {
            if (x < 0 || x >= 8 || y < 0 || y >= 8)
            {
                throw new InvalidOperationException("Tile pixel coordinates must be within 8x8 tile");
            }

            // one byte is an entire row of the tile since they are 8x8px
            // add y to get the row we want, and add 8 to get the high bitplane row
            byte lowPlaneRow = ReadPpuMemory((ushort)(tileStart + y));
            byte highPlaneRow = ReadPpuMemory((ushort)(tileStart + 8 + y));

            // the leftmost pixel is bit 7, so we need to shift the row right by (7 - x) to get the bit for this pixel
            int bitPosition = 7 - x;
            int lowBit = (lowPlaneRow >> bitPosition) & 1;
            int highBit = (highPlaneRow >> bitPosition) & 1;

            // combine the two bits into a color index from 0 to 3
            int colorIndex = lowBit | (highBit << 1);
            return (byte)(colorIndex);
        }

        /// <summary>
        /// Returns the background palette number selected by the attribute table.
        /// </summary>
        public byte GetBackgroundPaletteNumber(int x, int y)
        {
            var currentNametableIdx = GetNametableIdx(x, y);
            var (localX, localY) = (x % 256, y % 240);
            // each attribute background tile is 32x32px
            // divided into four 16x16px quadrants
            // each row of tiles in attr table is 8 tiles
            int attributeColumn = localX / 32;
            int attributeRow = localY / 32;

            ushort attributeAddress =
                (ushort)(0x23C0 + (currentNametableIdx * 0x400) + attributeRow * 8 + attributeColumn);

            // get the 32x32px attribute byte and select the correct quadrant
            int attributeValue = ReadPpuMemory(attributeAddress);
            int quadrantColumn = (x % 32) / 16; // 0 = left, 1 = right
            int quadrantRow = (y % 32) / 16;    // 0 = top,  1 = bottom

            // how much do we need to shift the bytes to get the correct quadrant's palette number into bits 0 and 1?
            int shift = (quadrantRow * 2 + quadrantColumn) * 2; // we want two bits for the palette number, so multiply by 2

            // move the relevant bits down to bit 0 and 1
            return (byte)((attributeValue >> shift) & 3);
        }

        /// <summary>
        /// Resolves a background palette number and color index to an NES palette value.
        /// There are 4 palettes and 4 indices in a palette
        /// </summary>
        public byte GetBackgroundPaletteValue(byte paletteNumber, byte colorIndex)
        {
            if (colorIndex == 0) paletteNumber = 0; // color index 0 always uses the universal background color at $3F00
            return ReadPpuMemory((ushort)(PALETTE_START + (paletteNumber * 4) + colorIndex));
        }

        private byte GetSpritePaletteValue(byte paletteNumber, byte colorIndex)
        {
            if (colorIndex == 0) return 0; // color index 0 is transparent for sprites
            return ReadPpuMemory((ushort)(PALETTE_START + 0x10 + (paletteNumber * 4) + colorIndex));
        }

        /// <summary>
        /// Builds one 256x240 frame of NES palette indices. Converting those
        /// indices to host ARGB colors belongs to NesSystemPalette/the UI.
        /// </summary>
        public byte[] RenderBackgroundFrame()
        {
            var ret = new byte[256*240];

            for (int y = 0; y < 240; y++)
            {
                for (int x = 0; x < 256; x++)
                {
                    ret[y * 256 + x] = GetBackgroundPixel(x, y);
                }
            }
            return ret;
        }

        /// <summary>
        /// Builds the final palette-index frame after sprites are composited
        /// with the background.
        /// </summary>
        public byte[] RenderFrame()
        {
            // background pixel + sprite pixel = final pixel
            var ret = RenderBackgroundFrame();

            var sprites = new byte[256 * 240];
            for (int y = 0; y < 240; y++)
            {
                for (int x = 0; x < 256; x++)
                {
                    bool found = false;
                    for (int spriteidx = 0; spriteidx < 64 && !found; spriteidx++)
                    {
                        // Get color idx for any sprites on this coordinate
                        sprites[y * 256 + x] = GetSpritePixelColorIndex(spriteidx, x, y);
                        if (sprites[y * 256 + x] != 0) // sprite pixel is not transparent
                        {
                            // Get the color value for the sprite pixel and overwrite the background pixel
                            byte attrs = ReadOamByte((byte)((spriteidx * 4) + 2));
                            var paletteNumber = (byte)(attrs & 0x03);
                            ret[y * 256 + x] = GetSpritePaletteValue(paletteNumber, sprites[y * 256 + x]);
                            found = true;
                        }
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Reports one completed frame to the host and clears the notification
        /// so the same frame is not presented repeatedly.
        /// </summary>
        public bool ConsumeFrameComplete()
        {
            if (FrameComplete)
            {
                FrameComplete = false;
                return true;
            }
            return false;
        }

        public void Tick()
        {
            Cycle++;
            if (Cycle > 340)
            {
                Cycle = 0;
                Scanline++;

                if (Scanline > 261)
                {
                    Scanline = 0;
                    FrameComplete = true;
                }
            }

            if (Scanline is >= 0 and < 240 && Cycle == 1)
            {
                _bgNametableAtScanline[Scanline] = _backgroundNametable;
                _scanlineScrolls[Scanline] = (ScrollX, ScrollY, Register.Registers[PPURegister.PPUCTRL_IDX]);
                _scanlineCaptured[Scanline] = true;
            }

            byte mask = Register.Registers[PPURegister.PPUMASK_IDX];

            bool backgroundEnabled = (mask & 0x08) != 0;
            bool spritesEnabled = (mask & 0x10) != 0;

            if (backgroundEnabled && spritesEnabled && Scanline >= 0 && Scanline < 240 && Cycle > 0 && Cycle < 256)
            {
                int screenY = Scanline;
                int screenX = Cycle - 1;
                var bgIdx = GetBackgroundColorIndex(screenX, screenY);
                var spriteIdx = GetSpritePixelColorIndex(0, screenX, screenY);
                if (bgIdx != 0 && spriteIdx != 0)
                {
                    Register.Registers[PPURegister.PPUSTATUS_IDX] |= 0x40;
                }
            }
            else if (Scanline == 241 && Cycle == 1)
            {
                Register.PPUSTATUS |= 0x80; // Set bit 7
                if ((Register.PPUCTRL & 0x80) != 0)
                {
                    _b.Cpu.RequestInterrupt(InterruptType.NMI);
                }
            }
            else if (Scanline == 261 && Cycle == 1)
            {
                Register.PPUSTATUS &= 0x7F; // Clear bit 7
                Register.PPUSTATUS &= 0x1F; // Clear hit flag bit
            }
        }
    }

    public enum NametableMirroring
    {
        Horizontal,
        Vertical
    }

    public class PPURegister
    {
        public PPURegister(Ppu ppu)
        {
            _ppu = ppu;
            _registers = new byte[9];
        }

        private Ppu _ppu;

        private byte[] _registers { get; set; }


        public byte[] Registers
        {
            get
            {
                return _registers;
            }
        }

        public const int PPUCTRL_IDX = 0;
        public const int PPUMASK_IDX = 1;
        public const int PPUSTATUS_IDX = 2;
        public const int OAMADDR_IDX = 3;
        public const int OAMDATA_IDX = 4;
        public const int PPUSCROLL_IDX = 5;
        public const int OAMDMA_IDX = 8;

        public byte PPUCTRL
        {
            get
            {
                return _ppu.ReadCpuRegister(Ppu.PPUCTRL_ADDR);
            }
            set
            {
                _registers[PPUCTRL_IDX] = value;
            }
        }

        public byte PPUMASK
        {
            get
            {
                return _ppu.ReadCpuRegister(Ppu.PPUMASK_ADDR);
            }
            set
            {
                _registers[PPUMASK_IDX] = value;
            }
        }

        public byte PPUSTATUS
        {
            get
            {
                return _ppu.ReadCpuRegister(Ppu.PPUSTATUS_ADDR);
            }
            set
            {
                _registers[PPUSTATUS_IDX] = value;
            }
        }

        public byte OAMADDR
        {
            get => _registers[OAMADDR_IDX]; set => _registers[OAMADDR_IDX] = value;
        }

        public byte OAMDATA
        {
            get => _registers[OAMDATA_IDX]; set => _registers[OAMDATA_IDX] = value;
        }

        public byte PPUSCROLL
        {
            get => _registers[PPUSCROLL_IDX]; set => _registers[PPUSCROLL_IDX] = value;
        }

        public byte PPUADDR
        {
            get
            {
                return _ppu.ReadCpuRegister(Ppu.PPUADDR_ADDR);
            }
            set
            {
                _ppu.WriteCpuRegister(Ppu.PPUADDR_ADDR, value);
            }
        }

        public byte PPUDATA
        {
            get
            {
                return _ppu.ReadCpuRegister(Ppu.PPUDATA_ADDR);
            }
            set
            {
                _ppu.WriteCpuRegister(Ppu.PPUDATA_ADDR, value);
            }
        }

        public byte OAMDMA
        {
            get => _registers[OAMDMA_IDX]; set => _registers[OAMDMA_IDX] = value;
        }

    }
}
