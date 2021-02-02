using OpenTK;
using OpenTK.Graphics;
using System;

namespace Lighthouse3
{
    public static class Calc
    {
        // Seed used to generate random numbers
        [ThreadStatic] private static uint seed;

        public static float Epsilon = 0.0005f;
        public static float InvPi { get; private set; }
        public static float Inv2Pi { get; private set; }
        public static float Pi { get; private set; }

        //Init should be called separately for every thread
        public static void Init()
        {
            seed = (uint)(new Random().Next() * uint.MaxValue);

            Pi = (float)Math.PI;
            InvPi = 1f / Pi;
            Inv2Pi = 1f / (2 * Pi);
            Sqrt(5);

        }

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

        public static float Max(float a, float b, float c)
        {
            if (a > b)
            {
                if (a > c)
                    return a;
                return c;
            }
            if (b > c)
                return b;
            return c;
        }

        public static int Floor(float value)
        {
            return (int)value;
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

        // From https://gist.github.com/badboy/6267743
        public static uint WangHash(uint seed)
        {
            seed = (seed ^ 61) ^ (seed >> 16);
            seed *= 9;
            seed ^= (seed >> 4);
            seed *= 0x27d4eb2d;
            seed ^= (seed >> 15);
            return seed;
        }

        public static uint XORshift(uint state)
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return state;
        }

        // Range [min, max)
        public static int RandomInt(int min, int max)
        {
            float x;
            do
            {
                x = Random();
            } while (x == 1f);
            return (int)(x * (max - min)) + min;
        }

        // Range [0, 1]
        public static float Random()
        {
            return XORshift(WangHash(++seed)) * 2.3283064365387e-10f;
        }

        public static Vector3 RandomOnUnitSphere()
        {
            return RandomInUnitSphere().Normalized();
        }

        public static Vector3 RandomInUnitSphere()
        {
            Vector3 point;
            do
            {
                point = RandomInUnitCube();
            } while (point.LengthSquared > 1);
            return point;
        }

        public static Vector3 RandomOnHalfSphereCosineWeighted()
        {

            float x;
            float y;
            float xS;
            float yS;
            do
            {
                x = Random() * 2f - 1f;
                y = Random() * 2f - 1f;
                xS = x * x;
                yS = y * y;
            } while (xS + yS > 1);
            float z = Sqrt(1 - (xS + yS));
            return new Vector3(x, y, z);
        }

        public static Vector3 RandomInUnitCube()
        {
            return new Vector3(Random() * 2f - 1f, Random() * 2f - 1f, Random() * 2f - 1f);
        }

        // From our predecessor in RenderSystem/common_functions.h
        public static Vector3 WorldToTangent(Vector3 V, Vector3 N)
        {
            float sign = Sign(N.Z);
            float a = -1f / (sign + N.Z);
            float b = N.X * N.Y * a;
            Vector3 B = new Vector3(1f + sign * N.X * N.X * a, sign * b, -sign * N.X);
            Vector3 T = new Vector3(b, sign + N.Y * N.Y * a, -N.Y);
            return new Vector3(Vector3.Dot(V, T), Vector3.Dot(V, B), Vector3.Dot(V, N));
        }
        public static float Sqrt(float n)
        {
            return (float)Math.Sqrt(n);
        }

        public static float Sign(float n)
        {
            return n < 0 ? -1f : 1f;
        }

        public static float Pow(float x, int pow)
        {
            float result = x;
            for (int i = 1; i < pow; i++)
                result *= x;
            return result;
        }

        public static Vector3 Vector3MaxValue()
        {
            return new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        }

        public static Vector3 Vector3MinValue()
        {
            return new Vector3(float.MinValue, float.MinValue, float.MinValue);
        }

        // From https://github.com/mitsuba-renderer/mitsuba, Power of 2 determined empirically by Veach
        public static float PowerHeuristic(float pdfA, float pdfB)
        {
            pdfA *= pdfA;
            pdfB *= pdfB;
            return pdfA / (pdfA + pdfB);
        }

        public static float KLDistance(float[] p, float[] q)
        {
            float sum = 0;
            for (int i = 0; i < p.Length; i++)
            {
                if (p[i] == 0f)
                    continue;
                sum += p[i] * (float)Math.Log(p[i] / q[i]);
            }
            return sum;
        }

        public static float Chi2Distance(float[] p, float[] q)
        {
            float sum = 0;
            for (int i = 0; i < p.Length; i++)
            {
                float diff = p[i] - q[i];
                sum += diff * diff / q[i];
            }
            return sum;
        }

        public static float HellingerDistance(float[] p, float[] q)
        {
            float sum = 0;
            for (int i = 0; i < p.Length; i++)
            {
                float sqrDiff = Sqrt(p[i]) - Sqrt(q[i]);
                sum += sqrDiff * sqrDiff;
            }
            return 0.5f * sum;
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
            float x = Calc.Sqrt(vector.X);
            float y = Calc.Sqrt(vector.Y);
            float z = Calc.Sqrt(vector.Z);
            return new Vector3(x, y, z);
        }

        public static Vector3 Divide(this Vector3 vector, Vector3 other)
        {
            vector.X /= other.X;
            vector.Y /= other.Y;
            vector.Z /= other.Z;
            return vector;
        }

    }
}