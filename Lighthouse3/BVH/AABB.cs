using Lighthouse3.Primitives;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse3.BVH
{
    public struct AABB
    {
        public Vector3 min;
        public Vector3 max;

        public AABB(Vector3 min, Vector3 max)
        {
            this.min = min;
            this.max = max;
        }

        public AABB(Primitive[] primitives)
        {
            min = primitives[0].Min();
            max = primitives[0].Max();
            for (int i = 1; i < primitives.Length; i++)
            {
                Vector3 newMin = primitives[i].Min();
                Vector3 newMax = primitives[i].Max();
                for (int xyz = 0; xyz < 3; xyz++)
                {
                    if (newMin[xyz] < min[xyz])
                        min[xyz] = newMin[xyz];
                    if (newMax[xyz] > max[xyz])
                        max[xyz] = newMax[xyz];
                }
            }
        }

        public AABB[] SplitAABB()
        {
            AABB[] newAABBs = new AABB[2];

            float xLength = max.X - min.X;
            float yLength = max.Y - min.Y;
            float zLength = max.Z - min.Z;

            if (xLength > yLength && xLength > zLength)
            {
                //Split along x_axis
                newAABBs[0].min = min;
                newAABBs[0].max = new Vector3(max.X - (xLength / 2f), max.Y, max.Z);
                newAABBs[1].min = new Vector3(min.X + (xLength / 2f), min.Y, min.Z);
                newAABBs[1].max = max;
            }

            else if (yLength > zLength)
            {
                //Split along y_axis
                newAABBs[0].min = min;
                newAABBs[0].max = new Vector3(max.X, max.Y - (yLength / 2f), max.Z);
                newAABBs[1].min = new Vector3(min.X, min.Y + (yLength / 2f), min.Z);
                newAABBs[1].max = max;
            }

            else
            {
                //Split along z_axis
                newAABBs[0].min = min;
                newAABBs[0].max = new Vector3(max.X, max.Y, max.Z - (zLength / 2f));
                newAABBs[1].min = new Vector3(min.X, min.Y, min.Z + (zLength / 2f));
                newAABBs[1].max = max;
            }
            return newAABBs;
        }

        public bool Contains(Vector3 p)
        {
            return 
                min.X <= p.X && min.Y <= p.Y && min.Z <= p.Z &&
                max.X >= p.X && max.Y >= p.Y && max.Z >= p.Z;
        }
    }
}
