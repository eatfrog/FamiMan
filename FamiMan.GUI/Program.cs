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
        uint[] framebuffer = new uint[NesWidth * NesHeight];
        bool quit = false;

        while (!quit)
        {
            RunUntilNextFrame(bus);
            CopyBackgroundFrameToArgb(bus.Ppu, framebuffer);

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

    private static void RunUntilNextFrame(Bus bus)
    {
        do
        {
            bus.Clock();
        }
        while (!bus.Ppu.ConsumeFrameComplete());
    }

    private static void CopyBackgroundFrameToArgb(Ppu ppu, uint[] destination)
    {
        byte[] paletteIndices = ppu.RenderBackgroundFrame();

        for (int i = 0; i < paletteIndices.Length; i++)
            destination[i] = NesSystemPalette.ToArgb(paletteIndices[i]);
    }
}
