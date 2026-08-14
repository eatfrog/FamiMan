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
        public static uint ToArgb(byte paletteIndex)
        {
            throw new NotImplementedException();
        }
    }
}
