using FamiMan.Core;
using FamiMan.GUI.UI;
using FamiMan.Platform;

internal class Program
{
    private const int NesWidth = 256;
    private const int NesHeight = 240;
    private const int MaximumCpuClocksPerFrame = 100_000;

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
        bool emulationHalted = false;

        while (!quit)
        {
            if (!emulationHalted)
            {
                try
                {
                    RunUntilNextFrame(bus);
                    CopyFrameToArgb(bus.Ppu, framebuffer);
                }
                catch (Exception exception)
                {
                    emulationHalted = true;
                    debugOverlay.ShowFailure(exception, bus, cpu);
                }
            }

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

                        SetControllerButton(bus.Controller1, windowEvent.Key, pressed: true);
                        debugOverlay.HandleKeyDown(windowEvent.Key);
                        break;
                    case WindowEventType.KeyUp:
                        SetControllerButton(bus.Controller1, windowEvent.Key, pressed: false);
                        break;
                }
            }

            window.DrawFrame(framebuffer, NesWidth, NesHeight, debugOverlay.LeftInset);
            debugOverlay.Render(window, bus, cpu);
            window.Present();
            debugOverlay.FramePresented();
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
        int clocks = 0;

        do
        {
            bus.Clock();

            clocks++;
            if (clocks > MaximumCpuClocksPerFrame)
            {
                throw new InvalidOperationException(
                    $"The PPU did not complete a frame after {MaximumCpuClocksPerFrame:N0} CPU clocks. " +
                    $"Current PPU position is scanline {bus.Ppu.Scanline}, cycle {bus.Ppu.Cycle}.");
            }
        }
        while (!bus.Ppu.ConsumeFrameComplete());
    }

    private static void CopyFrameToArgb(Ppu ppu, uint[] destination)
    {
        byte[] paletteIndices = ppu.RenderFrame();

        for (int i = 0; i < paletteIndices.Length; i++)
            destination[i] = NesSystemPalette.ToArgb(paletteIndices[i]);
    }

    private static void SetControllerButton(
        NesController controller,
        Key key,
        bool pressed)
    {
        ControllerButton? button = key switch
        {
            Key.Z => ControllerButton.A,
            Key.X => ControllerButton.B,
            Key.RightShift => ControllerButton.Select,
            Key.Enter => ControllerButton.Start,
            Key.Up => ControllerButton.Up,
            Key.Down => ControllerButton.Down,
            Key.Left => ControllerButton.Left,
            Key.Right => ControllerButton.Right,
            _ => null
        };

        if (button is ControllerButton mappedButton)
            controller.SetButton(mappedButton, pressed);
    }
}
