using FamiMan.Core;
using SDL2;
using System.Reflection;

if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
{
    Console.WriteLine("Unable to init sdl");
    return;
}

SDL_ttf.TTF_Init();

var window = IntPtr.Zero;
var renderer = IntPtr.Zero;
//window = SDL.SDL_CreateWindow("Window", SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED, 500, 500, SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
//renderer = SDL.SDL_CreateRenderer(window, 0, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);

SDL.SDL_CreateWindowAndRenderer(500, 500, SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE, out window, out renderer);
SDL.SDL_Event e;
bool quit = false;
SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 0);
SDL.SDL_RenderClear(renderer);
SDL.SDL_SetRenderDrawColor(renderer, 255, 0, 0, 255);
var r = new Random();

var b = new Bus();
var c = new Cpu(b);
var io = new IO(b);
io.LoadProgramFromHexString("A9448544E644A22D4C0000", 0);
c.S = 0xFF;

while (!quit)
{
    SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 0);
    SDL.SDL_RenderClear(renderer);

    while (SDL.SDL_PollEvent(out e) != 0)
    {
        switch (e.type)
        {
            case SDL.SDL_EventType.SDL_QUIT:
                quit = true;
                break;
            case SDL.SDL_EventType.SDL_KEYDOWN:
                switch (e.key.keysym.sym)
                {
                    case SDL.SDL_Keycode.SDLK_q:
                        quit = true;
                        break;
                }
                break;
            default:
                break;
        }
    }


    SDL.SDL_SetRenderDrawColor(renderer, c.X, 50, 50, 255);
    SDL.SDL_RenderDrawPoint(renderer, 1, 1);
    SDL.SDL_SetRenderDrawColor(renderer, c.Y, 50, 50, 255);
    SDL.SDL_RenderDrawPoint(renderer, 2, 2);
    var memVal = b.Ram[0x044];
    SDL.SDL_SetRenderDrawColor(renderer, memVal, 50, 50 , 255);
    SDL.SDL_RenderDrawPoint(renderer, 2, 2);
    WriteText("PC: " + c.PC);
    SDL.SDL_RenderPresent(renderer);
    c.Tick();
}

void WriteText(string text)
{
    string execPath = AppDomain.CurrentDomain.BaseDirectory;

    //this opens a font style and sets a size
    IntPtr font = SDL_ttf.TTF_OpenFont("c:\\windows\\fonts\\arial.ttf", 24);

    if (font == IntPtr.Zero)
    {
        throw new InvalidOperationException(SDL.SDL_GetError());
    }
    // this is the color in rgb format,
    // maxing out all would give you the color white,
    // and it will be your text's color
    SDL.SDL_Color White = new SDL.SDL_Color{ r = 255, g = 255, b = 255 };

    // as TTF_RenderText_Solid could only be used on
    // SDL_Surface then you have to create the surface first
    IntPtr surfaceMessage =
        SDL_ttf.TTF_RenderText_Solid(font, text, White);

    // now you can convert it into a texture
    IntPtr Message = SDL.SDL_CreateTextureFromSurface(renderer, surfaceMessage);

    SDL.SDL_Rect Message_rect; //create a rect
    Message_rect.x = 15;  //controls the rect's x coordinate 
    Message_rect.y = 15; // controls the rect's y coordinte
    Message_rect.w = 200; // controls the width of the rect
    Message_rect.h = 100; // controls the height of the rect

    SDL.SDL_Rect temp;
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
    SDL.SDL_RenderCopy(renderer, Message, ref temp, ref Message_rect);
}

SDL.SDL_DestroyRenderer(renderer);
SDL.SDL_DestroyWindow(window);
SDL.SDL_Quit();