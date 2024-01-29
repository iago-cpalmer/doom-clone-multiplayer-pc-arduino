using OpenTK.Mathematics;
using System;
using System.Text;

namespace doom_clone
{
    class Serializer
    {
        public static byte[] Serialize(Vector3 vector)
        {
            byte[] data = new byte[sizeof(float) * 3];
            Serialize(ref data, vector.X, 0);
            Serialize(ref data, vector.Y, 4);
            Serialize(ref data, vector.Z, 8);
            return data;
        }
        public static byte[] Serialize(Vector2 vector)
        {
            byte[] data = new byte[sizeof(float) * 2];
            Serialize(ref data, vector.X, 0);
            Serialize(ref data, vector.Y, 4);
            return data;
        }
        public static void Serialize(ref byte[] data, Vector3 vector, int startIndex)
        {
            byte[] v = new byte[sizeof(float) * 3];
            Serialize(ref data, vector.X, 0);
            Serialize(ref data, vector.Y, 4);
            Serialize(ref data, vector.Z, 8);
            for (int i = 0; i < v.Length; i++)
            {
                data[i + startIndex] = v[i];
            }
        }
        public static void Serialize(ref byte[] data, Vector2 vector, int startIndex)
        {
            byte[] v = new byte[sizeof(float) * 2];
            Serialize(ref data, vector.X, 0);
            Serialize(ref data, vector.Y, 4);
            for (int i = 0; i < v.Length; i++)
            {
                data[i + startIndex] = v[i];
            }
        }

        public static byte[] Serialize(float f)
        {
            return BitConverter.GetBytes(f);
        }
        public static void Serialize(ref byte[] data, float f, int startIndex)
        {
            byte[] b = Serialize(f);
            for(int i = 0; i < b.Length; i++)
            {
                data[i + startIndex] = b[i];
            }
        }

        public static byte[] Serialize(int x)
        {
            byte[] intBytes = BitConverter.GetBytes(x);
            /*
            if (BitConverter.IsLittleEndian)
                Array.Reverse(intBytes);*/
            return intBytes;
        }
        public static void Serialize(ref byte[] data, int x, int startIndex)
        {
            byte[] intBytes = BitConverter.GetBytes(x);
            /*
            if (BitConverter.IsLittleEndian)
                Array.Reverse(intBytes);*/
            for (int i = 0; i < intBytes.Length; i++)
            {
                data[i + startIndex] = intBytes[intBytes.Length-1-i];
            }
        }

        public static void Serialize(ref byte[] data, short x, int startIndex)
        {
            byte[] shortBytes = BitConverter.GetBytes(x);
            /*
            if (BitConverter.IsLittleEndian)
                Array.Reverse(intBytes);*/
            for (int i = 0; i < shortBytes.Length; i++)
            {
                data[i + startIndex] = shortBytes[shortBytes.Length - 1 - i];
            }
        }

        public static float DeserializeFloat(byte[] data, int startIndex)
        {
            return BitConverter.ToSingle(data, startIndex);
        }

        public static Vector3 DeserializeVector3(byte[] data, int startIndex)
        {
            return new Vector3(DeserializeFloat(data, 0+startIndex), DeserializeFloat(data, 4 + startIndex), DeserializeFloat(data, 8 + startIndex));
        }

        public static Vector2 DeserializeVector2(byte[] data, int startIndex)
        {
            return new Vector2(DeserializeFloat(data, 0 + startIndex), DeserializeFloat(data, 4 + startIndex));
        }

        public static int DeserializeInt(byte[] data, int startIndex)
        {
            byte[] x = new byte[4];
            Array.Copy(data, startIndex, x, 0, 4);
            
            if (BitConverter.IsLittleEndian)
                Array.Reverse(x);
            return BitConverter.ToInt32(x, 0);
        }
        public static int DeserializeIntReversed(byte[] data, int startIndex)
        {
            byte[] x = new byte[4];
            Array.Copy(data, startIndex, x, 0, 4);
            Array.Reverse(x);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(x);
            return BitConverter.ToInt32(x, 0);
        }
        public static short DeserializeShort(byte[] data, int startIndex)
        {
            byte[] x = new byte[2];
            Array.Copy(data, startIndex, x, 0, 2);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(x);
            return BitConverter.ToInt16(x, 0);
        }
    }
}
