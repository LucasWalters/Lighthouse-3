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
            min = Vector3.Zero;
            max = Vector3.Zero;
            for (int i = 0; i < primitives.Length; i++)
            {
                Vector3 newMin = primitives[i].Min();
                Vector3 newMax = primitives[i].Max();
                if (i == 0)
                {
                    min = newMin;
                    max = newMax;
                    continue;
                }
                for (int j = 0; j < 3; j++)
                {
                    if (newMin[j] < min[j])
                        min[j] = newMin[j];
                    if (newMax[j] < max[j])
                        max[j] = newMax[j];
                }
            }
        }

        public bool Contains(Vector3 p)
        {
            //TODO
            return false;
        }
    }
}
