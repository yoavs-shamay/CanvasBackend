using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Canvas
{
    public class Canvas
    {
        public Pixel[,] Pixels { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }


        public Canvas(int width, int height, Pixel[,] pixels)
        {
            Width = width;
            Height = height;
            Pixels = pixels;
        }
    }
}
