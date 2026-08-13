using FamiMan.Core;
using FamiMan.GUI.UI;
using FamiMan.Platform;

internal class Program
{
    private const int NesWidth = 256;
    private const int NesHeight = 240;

    private static void Main(string[] args)
    {
        string fontPath = Path.Combine(AppContext.BaseDirectory, "Sans.ttf");
        using var window = new GameWindow("FamiMan", 800 + DebugOverlay.SidebarWidth, 800, fontPath);

        string romPath = GetRomPath(args);
        Bus bus = SetupNes(romPath);
        var cpu = bus.Cpu;
        var debugOverlay = new DebugOverlay();
        uint[] framebuffer = BuildPatternTablePreview(bus.Ppu);
        bool quit = false;

        while (!quit)
        {
            window.Clear(Color.Black);

            while (window.PollEvent(out WindowEvent windowEvent))
            {
                switch (windowEvent.Type)
                {
                    case WindowEventType.Quit:
                        quit = true;
                        break;
                    case WindowEventType.KeyDown:
                        if (windowEvent.Key is Key.Q or Key.Escape)
                        {
                            quit = true;
                            break;
                        }

                        debugOverlay.HandleKeyDown(windowEvent.Key);
                        break;
                }
            }

            window.DrawFrame(framebuffer, NesWidth, NesHeight, DebugOverlay.SidebarWidth);
            debugOverlay.Render(window, bus, cpu);
            window.Present();
            Thread.Sleep(16);
        }
    }

    private static string GetRomPath(string[] args)
    {
        if (args.Length == 0)
            throw new ArgumentException(
                "Pass the path to an iNES ROM. Example: " +
                "dotnet run --project FamiMan.GUI -- C:\\Code\\FamiMan\\smb.nes");

        return Path.GetFullPath(args[0]);
    }

    private static Bus SetupNes(string romPath)
    {
        var b = new Bus();

        b.IO.LoadINesRomFile(romPath);
        b.Cpu.Reset();
        return b;
    }

    /// <summary>
    /// Displays both 4 KiB CHR pattern tables from the loaded cartridge. This
    /// is a temporary visual checkpoint: CPU writes to PPU registers still need
    /// to go through side-effect-aware bus methods before a game can build its
    /// real nametables and palette.
    /// </summary>
    private static uint[] BuildPatternTablePreview(Ppu ppu)
    {
        var pixels = new uint[NesWidth * NesHeight];
        Array.Fill(pixels, 0xFF101010u);

        uint[] previewColors =
        {
            0xFF101010u,
            0xFF686868u,
            0xFFB0B0B0u,
            0xFFFFFFFFu
        };

        for (int patternTable = 0; patternTable < 2; patternTable++)
        {
            byte ppuCtrl = patternTable == 0 ? (byte)0x00 : (byte)0x10;
            ppu.WriteCpuRegister(0x2000, ppuCtrl);

            for (int tileNumber = 0; tileNumber < 256; tileNumber++)
            {
                int tileColumn = tileNumber % 16;
                int tileRow = tileNumber / 16;
                int screenStartX = patternTable * 128 + tileColumn * 8;
                int screenStartY = tileRow * 8;

                for (int y = 0; y < 8; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        byte colorIndex = ppu.GetTilePixelColorIndex(
                            (byte)tileNumber,
                            x,
                            y);

                        int screenX = screenStartX + x;
                        int screenY = screenStartY + y;
                        pixels[screenY * NesWidth + screenX] = previewColors[colorIndex];
                    }
                }
            }
        }

        return pixels;
    }
}
