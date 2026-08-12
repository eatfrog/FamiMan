using SDL2;
using static SDL2.SDL;

namespace FamiMan.Platform;

public sealed unsafe class GameWindow : IDisposable
{
    private IntPtr _window;
    private IntPtr _renderer;
    private IntPtr _font;
    private IntPtr _frameTexture;
    private int _frameWidth;
    private int _frameHeight;
    private bool _sdlInitialized;
    private bool _fontInitialized;
    private bool _disposed;

    public GameWindow(string title, int width, int height, string? fontPath = null, int fontSize = 24)
    {
        try
        {
            if (SDL_Init(SDL_INIT_VIDEO) < 0)
                throw CreateSdlException("Could not initialize video");

            _sdlInitialized = true;
            SDL_SetHint(SDL_HINT_RENDER_SCALE_QUALITY, "0");

            if (SDL_CreateWindowAndRenderer(
                    width,
                    height,
                    SDL_WindowFlags.SDL_WINDOW_RESIZABLE,
                    out _window,
                    out _renderer) < 0)
            {
                throw CreateSdlException("Could not create the window and renderer");
            }

            SDL_SetWindowTitle(_window, title);

            if (fontPath is not null)
                OpenFont(fontPath, fontSize);
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    public bool PollEvent(out WindowEvent windowEvent)
    {
        if (SDL_PollEvent(out SDL_Event sdlEvent) == 0)
        {
            windowEvent = default;
            return false;
        }

        windowEvent = sdlEvent.type switch
        {
            SDL_EventType.SDL_QUIT => new WindowEvent(WindowEventType.Quit),
            SDL_EventType.SDL_KEYDOWN => new WindowEvent(WindowEventType.KeyDown, MapKey(sdlEvent.key.keysym.sym)),
            SDL_EventType.SDL_KEYUP => new WindowEvent(WindowEventType.KeyUp, MapKey(sdlEvent.key.keysym.sym)),
            _ => default
        };

        return true;
    }

    public void Clear(Color color)
    {
        ThrowIfDisposed();
        EnsureSuccess(SDL_SetRenderDrawColor(_renderer, color.Red, color.Green, color.Blue, color.Alpha), "Could not set the clear color");
        EnsureSuccess(SDL_RenderClear(_renderer), "Could not clear the window");
    }

    public void DrawText(string text, int x, int y, Color color)
    {
        ThrowIfDisposed();
        if (_font == IntPtr.Zero)
            throw new InvalidOperationException("This window was created without a font.");

        IntPtr surface = SDL_ttf.TTF_RenderUTF8_Solid(
            _font,
            text,
            new SDL_Color { r = color.Red, g = color.Green, b = color.Blue, a = color.Alpha });

        if (surface == IntPtr.Zero)
            throw CreateSdlException("Could not render text");

        IntPtr texture = IntPtr.Zero;
        try
        {
            texture = SDL_CreateTextureFromSurface(_renderer, surface);
            if (texture == IntPtr.Zero)
                throw CreateSdlException("Could not create a text texture");

            EnsureSuccess(SDL_ttf.TTF_SizeUTF8(_font, text, out int width, out int height), "Could not measure text");
            var destination = new SDL_Rect { x = x, y = y, w = width, h = height };
            EnsureSuccess(SDL_RenderCopy(_renderer, texture, IntPtr.Zero, ref destination), "Could not draw text");
        }
        finally
        {
            if (texture != IntPtr.Zero)
                SDL_DestroyTexture(texture);
            SDL_FreeSurface(surface);
        }
    }

    /// <summary>
    /// Draws a complete ARGB8888 framebuffer. The image is centered and scaled by
    /// a whole-number factor so emulator pixels remain sharp.
    /// </summary>
    public void DrawFrame(ReadOnlySpan<uint> pixels, int width, int height)
    {
        ThrowIfDisposed();
        if (width <= 0 || height <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), "Frame dimensions must be positive.");
        if (pixels.Length != checked(width * height))
            throw new ArgumentException("The pixel count does not match the frame dimensions.", nameof(pixels));

        EnsureFrameTexture(width, height);

        fixed (uint* pixelPointer = pixels)
        {
            EnsureSuccess(
                SDL_UpdateTexture(_frameTexture, IntPtr.Zero, (IntPtr)pixelPointer, checked(width * sizeof(uint))),
                "Could not upload the framebuffer");
        }

        EnsureSuccess(SDL_GetRendererOutputSize(_renderer, out int outputWidth, out int outputHeight), "Could not read the window size");
        int scale = Math.Max(1, Math.Min(outputWidth / width, outputHeight / height));
        int renderedWidth = Math.Min(outputWidth, checked(width * scale));
        int renderedHeight = Math.Min(outputHeight, checked(height * scale));
        var destination = new SDL_Rect
        {
            x = (outputWidth - renderedWidth) / 2,
            y = (outputHeight - renderedHeight) / 2,
            w = renderedWidth,
            h = renderedHeight
        };

        EnsureSuccess(SDL_RenderCopy(_renderer, _frameTexture, IntPtr.Zero, ref destination), "Could not draw the framebuffer");
    }

    public void Present()
    {
        ThrowIfDisposed();
        SDL_RenderPresent(_renderer);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        if (_frameTexture != IntPtr.Zero)
            SDL_DestroyTexture(_frameTexture);
        if (_font != IntPtr.Zero)
            SDL_ttf.TTF_CloseFont(_font);
        if (_fontInitialized)
            SDL_ttf.TTF_Quit();
        if (_renderer != IntPtr.Zero)
            SDL_DestroyRenderer(_renderer);
        if (_window != IntPtr.Zero)
            SDL_DestroyWindow(_window);
        if (_sdlInitialized)
            SDL_Quit();

        _disposed = true;
    }

    private void OpenFont(string fontPath, int fontSize)
    {
        if (!File.Exists(fontPath))
            throw new FileNotFoundException("The window font could not be found.", fontPath);
        if (SDL_ttf.TTF_Init() < 0)
            throw CreateSdlException("Could not initialize font rendering");

        _fontInitialized = true;
        _font = SDL_ttf.TTF_OpenFont(fontPath, fontSize);
        if (_font == IntPtr.Zero)
            throw CreateSdlException("Could not open the window font");
    }

    private void EnsureFrameTexture(int width, int height)
    {
        if (_frameTexture != IntPtr.Zero && _frameWidth == width && _frameHeight == height)
            return;

        if (_frameTexture != IntPtr.Zero)
            SDL_DestroyTexture(_frameTexture);

        _frameTexture = SDL_CreateTexture(
            _renderer,
            SDL_PIXELFORMAT_ARGB8888,
            (int)SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING,
            width,
            height);

        if (_frameTexture == IntPtr.Zero)
            throw CreateSdlException("Could not create the framebuffer texture");

        _frameWidth = width;
        _frameHeight = height;
    }

    private static Key MapKey(SDL_Keycode keycode) => keycode switch
    {
        SDL_Keycode.SDLK_q => Key.Q,
        SDL_Keycode.SDLK_ESCAPE => Key.Escape,
        SDL_Keycode.SDLK_UP => Key.Up,
        SDL_Keycode.SDLK_DOWN => Key.Down,
        SDL_Keycode.SDLK_LEFT => Key.Left,
        SDL_Keycode.SDLK_RIGHT => Key.Right,
        SDL_Keycode.SDLK_z => Key.Z,
        SDL_Keycode.SDLK_x => Key.X,
        SDL_Keycode.SDLK_RETURN => Key.Enter,
        SDL_Keycode.SDLK_RSHIFT => Key.RightShift,
        _ => Key.Unknown
    };

    private static InvalidOperationException CreateSdlException(string message) =>
        new($"{message}: {SDL_GetError()}");

    private static void EnsureSuccess(int result, string message)
    {
        if (result < 0)
            throw CreateSdlException(message);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(GameWindow));
    }
}
