namespace FamiMan.Platform;

public readonly struct Color
{
    public static Color Black => new(0, 0, 0);
    public static Color White => new(255, 255, 255);

    public Color(byte red, byte green, byte blue, byte alpha = 255)
    {
        Red = red;
        Green = green;
        Blue = blue;
        Alpha = alpha;
    }

    public byte Red { get; }
    public byte Green { get; }
    public byte Blue { get; }
    public byte Alpha { get; }
}
