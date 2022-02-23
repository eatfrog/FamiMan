using SDL2;

if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
{
    Console.WriteLine("Unable to init sdl");
    return;
}

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
while (!quit)
{
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


    SDL.SDL_RenderDrawPoint(renderer, r.Next(0, 499), r.Next(0, 499));
    SDL.SDL_RenderPresent(renderer);

}
SDL.SDL_DestroyRenderer(renderer);
SDL.SDL_DestroyWindow(window);
SDL.SDL_Quit();