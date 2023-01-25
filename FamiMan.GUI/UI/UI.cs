using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SDL2.SDL;

namespace FamiMan.GUI.UI
{
    internal static class UI
    {
        internal static void WriteText(IntPtr renderer, SDL_Rect rect, IntPtr font, SDL_Color color, string text, int row)
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
                SDL_ttf.TTF_RenderUTF8_Solid(font, text, color);

            // now you can convert it into a texture
            IntPtr messageTexture = SDL_CreateTextureFromSurface(renderer, surfaceMessage);

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
            SDL_RenderCopy(renderer, messageTexture, ref temp, ref rect);
            SDL_DestroyTexture(messageTexture);
            SDL_FreeSurface(surfaceMessage);
        }
    }
}
