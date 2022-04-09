using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Canvas
{
    public class Pixel
    {
        public int Red { get; set; }
        public int Green { get; set; }
        public int Blue { get; set; }
        public string LastModifier { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public Pixel(int red, int green, int blue, string lastModifier, int x, int y)
        {
            Red = red;
            Green = green;
            Blue = blue;
            LastModifier = lastModifier;
            X = x;
            Y = y;
        }
    }
}
