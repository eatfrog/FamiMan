using FamiMan.Core;
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
        IntPtr window;
        IntPtr renderer;
        SDL_CreateWindowAndRenderer(800, 800, SDL_WindowFlags.SDL_WINDOW_RESIZABLE, out window, out renderer);
        SDL_Event e;
        bool quit = false;
        SDL_SetRenderDrawColor(renderer, 0, 0, 0, 0);
        SDL_RenderClear(renderer);
        SDL_SetRenderDrawColor(renderer, 255, 0, 0, 255);

        Bus b;
        Cpu c;
        SetupNes(out b, out c);
        //this opens a font style and sets a size
        IntPtr font = SDL_ttf.TTF_OpenFont("c:\\windows\\fonts\\arial.ttf", 24);
        SDL_Color white = new SDL_Color { r = 255, g = 255, b = 255 };
        SDL_Rect message_rect = new(); //create a rect

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
                        }
                        break;
                    default:
                        break;
                }
            }


            SDL_SetRenderDrawColor(renderer, c.X, 50, 50, 255);
            SDL_RenderDrawPoint(renderer, 1, 1);
            SDL_SetRenderDrawColor(renderer, c.Y, 50, 50, 255);
            SDL_RenderDrawPoint(renderer, 2, 2);
            var memVal = b.Ram[0x044];
            SDL_SetRenderDrawColor(renderer, memVal, 50, 50, 255);
            SDL_RenderDrawPoint(renderer, 2, 2);
            WriteText(message_rect, font, white, "PC: " + c.PC, 0);
            WriteText(message_rect, font, white, "A: " + c.A, 1);
            WriteText(message_rect, font, white, "X: " + c.X, 2);
            WriteText(message_rect, font, white, "Y: " + c.Y, 3);
            WriteText(message_rect, font, white, "S: " + c.S, 4);
            WriteText(message_rect, font, white, "P: " + c.P.AsByte(), 5);

            SDL_RenderPresent(renderer);
            c.Tick();
        }

        void WriteText(SDL_Rect rect, IntPtr font, SDL_Color color, string text, int row)
        {

            if (font == IntPtr.Zero)
            {
                throw new InvalidOperationException(SDL_GetError());
            }
            // this is the color in rgb format,
            // maxing out all would give you the color white,
            // and it will be your text's color

            // as TTF_RenderText_Solid could only be used on
            // SDL_Surface then you have to create the surface first
            IntPtr surfaceMessage =
                SDL_ttf.TTF_RenderText_Solid(font, text, color);

            // now you can convert it into a texture
            IntPtr Message = SDL_CreateTextureFromSurface(renderer, surfaceMessage);

            rect.x = 15;  //controls the rect's x coordinate 
            rect.y = 15 + 60 * row; // controls the rect's y coordinte
            rect.w = 150; // controls the width of the rect
            rect.h = 50; // controls the height of the rect


            SDL_Rect temp;
            temp.x = 0;
            temp.y = 0;
            temp.w = 200;
            temp.h = 100;
            // (0,0) is on the top left of the window/screen,
            // think a rect as the text's box,
            // that way it would be very simple to understand

            // Now since it's a texture, you have to put RenderCopy
            // in your game loop area, the area where the whole code executes

            // you put the renderer's name first, the Message,
            // the crop size (you can ignore this if you don't want
            // to dabble with cropping), and the rect which is the size
            // and coordinate of your texture
            SDL_RenderCopy(renderer, Message, ref temp, ref rect);
        }

        SDL_DestroyRenderer(renderer);
        SDL_DestroyWindow(window);
        SDL_Quit();
    }

    private static void SetupNes(out Bus b, out Cpu c)
    {
        b = new Bus();
        c = new Cpu(b);
        var io = new IO(b);
        io.LoadProgramFromHexString("A9448544E644C544A22DE646A4464C0000", 0);
        c.S = 0xFF;
    }
}