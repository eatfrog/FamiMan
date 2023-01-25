using FamiMan.Core;
using FamiMan.GUI.UI;
using SDL2;
using System.Text;
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
        IntPtr window;
        IntPtr renderer;
        SDL_CreateWindowAndRenderer(800, 800, SDL_WindowFlags.SDL_WINDOW_RESIZABLE, out window, out renderer);
        SDL_Event e;
        bool quit = false;
        SDL_SetRenderDrawColor(renderer, 0, 0, 0, 0);
        SDL_RenderClear(renderer);
        SDL_SetRenderDrawColor(renderer, 255, 0, 0, 255);

        Bus b;
        b = SetupNes();
        var c = b.Cpu;
        //this opens a font style and sets a size
        IntPtr font = SDL_ttf.TTF_OpenFont("c:\\windows\\fonts\\arial.ttf", 24);
        SDL_Color white = new() { r = 255, g = 255, b = 255 };
        SDL_Rect message_rect = new(); //create a rect
        int waitTime = 0;
        while (!quit)
        {
            SDL_SetRenderDrawColor(renderer, 0, 0, 0, 0);
            SDL_RenderClear(renderer);

            while (SDL_PollEvent(out e) != 0)
            {
                switch (e.type)
                {
                    case SDL_EventType.SDL_QUIT:
                        quit = true;
                        break;
                    case SDL_EventType.SDL_KEYDOWN:
                        switch (e.key.keysym.sym)
                        {
                            case SDL_Keycode.SDLK_q:
                                quit = true;
                                break;
                            case SDL_Keycode.SDLK_DOWN:
                                waitTime++;
                                UI.WriteText(renderer, message_rect, font, white, "Wait: " + waitTime, 6);
                                break;
                            case SDL_Keycode.SDLK_UP:
                                if (waitTime > 0) { waitTime--; }
                                UI.WriteText(renderer, message_rect, font, white, "Wait: " + waitTime, 6);
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }

            UI.WriteText(renderer, message_rect, font, white, "PC: " + c.PC.ToString("X") + " - " + c.CurrentInstructionName, 0);
            UI.WriteText(renderer, message_rect, font, white, "A: " + c.A, 1);
            UI.WriteText(renderer, message_rect, font, white, "X: " + c.X, 2);
            UI.WriteText(renderer, message_rect, font, white, "Y: " + c.Y, 3);
            UI.WriteText(renderer, message_rect, font, white, "S: " + c.S, 4);
            UI.WriteText(renderer, message_rect, font, white, "P: " + c.P.AsByte(), 5);

            //Span<byte> mem = b.Ram.AsSpan()[0x6000..0x6017];
            //string memString = Encoding.Default.GetString(mem.ToArray());
            //UI.WriteText(renderer, message_rect, font, white, "0x6000: " + memString, 6);

            SDL_RenderPresent(renderer);
            c.Tick();
            Thread.Sleep(waitTime);
        }



        SDL_DestroyRenderer(renderer);
        SDL_DestroyWindow(window);
        SDL_Quit();
    }

    private static Bus SetupNes()
    {
        var b = new Bus();

        //io.LoadProgramFromHexString("A9448544E644C544A22DE646A4464C0000", 0);
        //c.S = 0xFF;
        var rom = b.IO.LoadINesRomFile(Directory.GetCurrentDirectory() + "\\files\\01-implied.nes");
        b.Cpu.Reset();
        return b;
    }
}