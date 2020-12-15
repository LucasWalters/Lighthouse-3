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

        public bool Contains(Vector3 p)
        {
            //TODO
            return false;
        }
    }
}
