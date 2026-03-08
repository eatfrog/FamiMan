using FamiMan.Core;
using FamiMan.GUI.UI;
using SDL2;
using static SDL2.SDL;

internal class Program
{
    private static void Main(string[] args)
    {
        if (SDL_Init(SDL_INIT_VIDEO) < 0)
        {
            Console.WriteLine("Unable to init sdl");
            return;
        }

        SDL_ttf.TTF_Init();
        SDL_CreateWindowAndRenderer(800, 800, SDL_WindowFlags.SDL_WINDOW_RESIZABLE, out IntPtr window, out IntPtr renderer);

        SDL_SetRenderDrawColor(renderer, 0, 0, 0, 0);
        SDL_RenderClear(renderer);
        SDL_SetRenderDrawColor(renderer, 255, 0, 0, 255);

        Bus bus = SetupNes();
        var cpu = bus.Cpu;

        IntPtr font = SDL_ttf.TTF_OpenFont("c:\\windows\\fonts\\arial.ttf", 24);
        SDL_Color white = new() { r = 255, g = 255, b = 255 };
        SDL_Rect messageRect = new();
        var debugOverlay = new DebugOverlay();

        SDL_Event e;
        bool quit = false;

        while (!quit)
        {
            SDL_SetRenderDrawColor(renderer, 0, 0, 0, 0);
            SDL_RenderClear(renderer);

            cpu.Tick();
            debugOverlay.UpdateInstructionDebug(bus, cpu);

            while (SDL_PollEvent(out e) != 0)
            {
                switch (e.type)
                {
                    case SDL_EventType.SDL_QUIT:
                        quit = true;
                        break;
                    case SDL_EventType.SDL_KEYDOWN:
                        if (e.key.keysym.sym == SDL_Keycode.SDLK_q)
                        {
                            quit = true;
                            break;
                        }

                        debugOverlay.HandleKeyDown(e.key.keysym.sym);
                        break;
                }
            }

            debugOverlay.Render(renderer, messageRect, font, white, bus, cpu);
            SDL_RenderPresent(renderer);
            Thread.Sleep(debugOverlay.WaitTime);
        }

        SDL_DestroyRenderer(renderer);
        SDL_DestroyWindow(window);
        SDL_Quit();
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
