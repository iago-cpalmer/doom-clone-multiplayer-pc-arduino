using doom_clone.Utils;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace doom_clone.WorldClasses
{
    class Line
    {
        public mVector2i A { get; set; }
        public mVector2i B { get; set; }

        public int Type { get; set; }
        public Line() { }
        public Line(mVector2i a, mVector2i b, int type)
        {
            A = a;
            B = b;
            Type = type;
        }

        public String ToString()
        {
            return "A: " + A?.ToString() + ", B:" + B?.ToString() + ", Type:" + Type;
        }
    }

}
