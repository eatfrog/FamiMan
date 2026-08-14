using System;

namespace FamiMan.Core
{
    /// <summary>
    /// Converts the PPU's 6-bit color value into a host ARGB8888 pixel.
    /// Exact RGB values are a display choice because the original PPU emitted
    /// an analog video signal; this project only needs one consistent palette.
    /// </summary>
    public static class NesSystemPalette
    {
        private static readonly uint[] Colors =
        [
            // $00-$0F
            0xFF757575, 0xFF271B8F, 0xFF0000AB, 0xFF47009F,
            0xFF8F0077, 0xFFAB0013, 0xFFA70000, 0xFF7F0B00,
            0xFF432F00, 0xFF004700, 0xFF005100, 0xFF003F17,
            0xFF1B3F5F, 0xFF000000, 0xFF000000, 0xFF000000,

            // $10-$1F
            0xFFBCBCBC, 0xFF0073EF, 0xFF233BEF, 0xFF8300F3,
            0xFFBF00BF, 0xFFE7005B, 0xFFDB2B00, 0xFFCB4F0F,
            0xFF8B7300, 0xFF009700, 0xFF00AB00, 0xFF00933B,
            0xFF00838B, 0xFF000000, 0xFF000000, 0xFF000000,

            // $20-$2F
            0xFFFFFFFF, 0xFF3FBFFF, 0xFF5F97FF, 0xFFA78BFD,
            0xFFF77BFF, 0xFFFF77B7, 0xFFFF7763, 0xFFFF9B3B,
            0xFFF3BF3F, 0xFF83D313, 0xFF4FDF4B, 0xFF58F898,
            0xFF00EBDB, 0xFF000000, 0xFF000000, 0xFF000000,

            // $30-$3F
            0xFFFFFFFF, 0xFFABE7FF, 0xFFC7D7FF, 0xFFD7CBFF,
            0xFFFFC7FF, 0xFFFFC7DB, 0xFFFFBFB3, 0xFFFFDBAB,
            0xFFFFE7A3, 0xFFE3FFA3, 0xFFABF3BF, 0xFFB3FFCF,
            0xFF9FFFF3, 0xFF000000, 0xFF000000, 0xFF000000
        ];

        public static uint ToArgb(byte paletteIndex)
        {
            int index = paletteIndex & 0x3F; // only bottom 6 bits matter
            return Colors[index];
        }
    }
}
