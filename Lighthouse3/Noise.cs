using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse3
{
    public static class Noise
    {
        static Surface[] blueNoiseMaps;

        static readonly int blueNoiseMapsAmount = 8;
        static readonly string blueNoisePrefix = "../../assets/noise/LDR_RG01_";

        public static void Init()
        {
            blueNoiseMaps = new Surface[blueNoiseMapsAmount];
            for (int i = 0; i < blueNoiseMapsAmount; i++)
            {
                blueNoiseMaps[i] = new Surface(blueNoisePrefix + i + ".png");
            }
        }

        public static Vector2 Read(int x, int y, int mapIndex = 0)
        {
            Surface blueNoise = blueNoiseMaps[mapIndex % 8];
            x %= blueNoise.width;
            y %= blueNoise.height;
            Vector3 color = Color.FromARBG(blueNoise.pixels[x + y * blueNoise.width]);
            return new Vector2(color.X, color.Y);
        }

    }
}
