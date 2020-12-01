using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections;

namespace Lighthouse3
{
    public static class Calc
    {

        private static readonly Random globalRandom = new Random();
        [ThreadStatic] private static Random random;
        // From https://stackoverflow.com/a/11109361
        private static void CheckRandom()
        {
            if (random == null)
            {
                lock (globalRandom)
                {
                    if (random == null)
                    {
                        int seed = globalRandom.Next();
                        random = new Random(seed);
                    }
                }
            }
        }

        public static float Epsilon = 0.0005f;
        public static float InvPi 
        { 
            get 
            {
                if (invPi == 0f)
                    invPi = 1f / (float)Math.PI;
                return invPi;
            }
        }
        private static float invPi;
        public static float Pi
        {
            get
            {
                if (pi == 0f)
                    pi = (float)Math.PI;
                return pi;
            }
        }
        private static float pi;

        //Inverse lerp, maps value to [0, 1] based on min and max values
        public static float ILerp(float min, float max, float at)
        {
            return (at - min) / (max - min);
        }
        
        public static float Lerp(float min, float max, float t)
        {
            return min + (max - min) * t;
        }

        public static float Clamp(float value, float min = 0, float max = 1)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        public static int Clamp(int value, int min = 0, int max = 1)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        public static float Min(float a, float b)
        {
            if (a > b)
                return b;
            return a;
        }

        public static float MinPositive(float a, float b)
        {
            if (a <= 0)
                return b;
            if (b <= 0)
                return a;
            return Min(a, b);
        }

        public static float Max(float a, float b)
        {
            if (a < b)
                return b;
            return a;
        }

        public static int MaxDimension(Vector3 vector)
        {
            return (vector.X > vector.Y) ? ((vector.X > vector.Z) ? 0 : 2) : ((vector.Y > vector.Z) ? 1 : 2);
        }

        public static Vector3 Abs(Vector3 vector)
        {
            return new Vector3(Math.Abs(vector.X), Math.Abs(vector.Y), Math.Abs(vector.Z));
        }

        public static Vector3 Permute(Vector3 vector, int x, int y, int z)
        {
            return new Vector3(vector[x], vector[y], vector[z]);
        }

        public static float Random()
        {
            CheckRandom();
            return (float)random.NextDouble();
        }

        public static int RandomInt(int min, int max)
        {
            CheckRandom();
            return random.Next(min, max);
        }

        public static Vector3 RandomOnUnitSphere()
        {
            Vector3 point;
            do
            {
                point = RandomInUnitCube();
            } while (point.LengthSquared <= 1);
            return point.Normalized();
        }

        public static Vector3 RandomInUnitSphere()
        {
            Vector3 point;
            do
            {
                point = RandomInUnitCube();
            } while (point.LengthSquared <= 1);
            return point;
        }

        public static Vector3 RandomInUnitCube()
        {
            return new Vector3(Random() * 2f - 1f, Random() * 2f - 1f, Random() * 2f - 1f);
        }
    }

    public static class Extensions
    {
        public static Color4 Add(this Color4 left, Color4 right)
        {
            float r = Calc.Max(left.R + right.R, 0);
            float g = Calc.Max(left.G + right.G, 0);
            float b = Calc.Max(left.B + right.B, 0);
            return new Color4(r, g, b, left.A);
        }

        public static Color4 Subtract(this Color4 left, Color4 right)
        {
            float r = Calc.Max(left.R - right.R, 0);
            float g = Calc.Max(left.G - right.G, 0);
            float b = Calc.Max(left.B - right.B, 0);
            return new Color4(r, g, b, left.A);
        }

        public static Color4 Multiply(this Color4 left, Color4 right)
        {
            float r = Calc.Max(left.R * right.R, 0);
            float g = Calc.Max(left.G * right.G, 0);
            float b = Calc.Max(left.B * right.B, 0);
            return new Color4(r, g, b, left.A);
        }

        public static Color4 Multiply(this Color4 color, float scalar)
        {
            float r = Calc.Max(color.R * scalar, 0);
            float g = Calc.Max(color.G * scalar, 0);
            float b = Calc.Max(color.B * scalar, 0);
            return new Color4(r, g, b, color.A);
        }

        public static Vector3 Sqrt(this Vector3 vector)
        {
            float x = (float)Math.Sqrt(vector.X);
            float y = (float)Math.Sqrt(vector.Y);
            float z = (float)Math.Sqrt(vector.Z);
            return new Vector3(x, y, z);
        }

        public static Vector3 RotateX(this Vector3 vector, float angles)
        {
            float rad = (float)(Math.PI / 180) * angles;
            float sin = (float)Math.Sin(rad);
            float cos = (float)Math.Cos(rad);
            Matrix4 matrix = new Matrix4(
                new Vector4(1, 0, 0, 0),
                new Vector4(0, cos, -sin, 0),
                new Vector4(0, sin, cos, 0),
                new Vector4(0, 0, 0, 1)
            );
            return Vector3.Transform(vector, matrix);
        }

        public static Vector3 RotateY(this Vector3 vector, float angles)
        {
            float rad = (float)(Math.PI / 180) * angles;
            float sin = (float)Math.Sin(rad);
            float cos = (float)Math.Cos(rad);
            Matrix4 matrix = new Matrix4(
                new Vector4(cos, 0, sin, 0),
                new Vector4(0, 1, 0, 0),
                new Vector4(-sin, 0, cos, 0),
                new Vector4(0, 0, 0, 1)
            );
            return Vector3.Transform(vector, matrix);
        }

        public static Vector3 RotateZ(this Vector3 vector, float angles)
        {
            float rad = (float)(Math.PI / 180) * angles;
            float sin = (float)Math.Sin(rad);
            float cos = (float)Math.Cos(rad);
            Matrix4 matrix = new Matrix4(
                new Vector4(cos, -sin, 0, 0),
                new Vector4(sin, cos, 0, 0),
                new Vector4(0, 0, 1, 0),
                new Vector4(0, 0, 0, 1)
            );
            return Vector3.Transform(vector, matrix);
        }

    }
}