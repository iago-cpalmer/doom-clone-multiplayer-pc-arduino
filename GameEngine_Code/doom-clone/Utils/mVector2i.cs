using System;
using System.Collections.Generic;
using System.Text;

namespace doom_clone.Utils
{
    class mVector2i
    {
        public static mVector2i Zero { get { return _zero; } }
        private static mVector2i _zero = new mVector2i(0, 0);
        public int X { get; set; }
        public int Y { get; set; }

        public mVector2i() { }
        public mVector2i(int x, int y)
        {
            X = x;
            Y = y;
        }

        public String ToString()
        {
            return "(" + X + "," + Y + ")";
        }
    }
}
