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

        public AABB(Primitive[] primitives)
        {
            min = primitives[0].Min();
            max = primitives[0].Max();
            for (int i = 1; i < primitives.Length; i++)
            {
                Vector3 newMin = primitives[i].Min();
                Vector3 newMax = primitives[i].Max();
                for (int j = 0; j < 3; j++)
                {
                    if (newMin[j] < min[j])
                        min[j] = newMin[j];
                    if (newMax[j] > max[j])
                        max[j] = newMax[j];
                }
            }
        }

        public AABB[] SplitAABB()
        {
            AABB[] newAABBs = new AABB[2];

            float xLength = max.X - min.X;
            float yLength = max.Y - min.Y;
            float zLength = max.Z - min.Z;

            if (xLength >= yLength && xLength >= zLength)
            {
                //Split along x_axis
                newAABBs[0].min = min;
                newAABBs[0].max = new Vector3(max.X - (xLength / 2), max.Y, max.Z);
                newAABBs[1].min = new Vector3(min.X + (xLength / 2), min.Y, min.Z);
                newAABBs[1].max = max;
            }

            if (yLength >= xLength && yLength >= zLength)
            {
                //Split along y_axis
                newAABBs[0].min = min;
                newAABBs[0].max = new Vector3(max.X, max.Y - (yLength / 2), max.Z);
                newAABBs[1].min = new Vector3(min.X, min.Y + (yLength / 2), min.Z);
                newAABBs[1].max = max;
            }

            if (zLength >= xLength && zLength >= yLength)
            {
                //Split along z_axis
                newAABBs[0].min = min;
                newAABBs[0].max = new Vector3(max.X, max.Y, max.Z - (zLength / 2));
                newAABBs[1].min = new Vector3(min.X, min.Y, min.Z + (zLength / 2));
                newAABBs[1].max = max;
            }
            return newAABBs;
        }

        public bool Contains(Vector3 p)
        {
            //TODO
            return false;
        }
    }
}
