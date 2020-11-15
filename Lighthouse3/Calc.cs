﻿using OpenTK;
using System.Collections;

namespace Lighthouse3
{
    public class Calc
    {
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
    }
}