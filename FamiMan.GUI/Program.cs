using FamiMan.Core;
using FamiMan.GUI.UI;
using FamiMan.Platform;

internal class Program
{
    private static void Main(string[] args)
    {
        string fontPath = Path.Combine(AppContext.BaseDirectory, "Sans.ttf");
        using var window = new GameWindow("FamiMan", 800, 800, fontPath);

        Bus bus = SetupNes();
        var cpu = bus.Cpu;
        var debugOverlay = new DebugOverlay();
        bool quit = false;

        while (!quit)
        {
            window.Clear(Color.Black);

            cpu.Tick();
            debugOverlay.UpdateInstructionDebug(bus, cpu);

            while (cpu.Waiting)
                cpu.Tick();

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

            debugOverlay.Render(window, bus, cpu);
            window.Present();
            Thread.Sleep(debugOverlay.WaitTime);
        }
    }

    private static Bus SetupNes()
    {
        var b = new Bus();

        var rom = b.IO.LoadINesRomFile(Directory.GetCurrentDirectory() + "\\files\\nestest.nes");
        b.Cpu.Reset();
        b.Cpu.PC = 0xC000;
        return b;
    }
}
