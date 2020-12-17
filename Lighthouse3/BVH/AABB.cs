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
        public Vector3 center;

        public AABB(Vector3 min, Vector3 max)
        {
            this.min = min;
            this.max = max;
            center = min + ((max - min) / 2f);
        }

        public AABB(Primitive[] primitives)
        {
            min = primitives[0].Min();
            max = primitives[0].Max();

            //AABB firstBounds = primitives[0].bounds;
            //min = firstBounds.min;
            //max = firstBounds.max;
            for (int i = 1; i < primitives.Length; i++)
            {
                AABB bounds = primitives[i].bounds;
                //bounds = Extend(bounds);
                //min = bounds.min;
                //max = bounds.max;
                Vector3 newMin = bounds.min;
                Vector3 newMax = bounds.max;
                for (int xyz = 0; xyz < 3; xyz++)
                {
                    if (newMin[xyz] < min[xyz])
                        min[xyz] = newMin[xyz];
                    if (newMax[xyz] > max[xyz])
                        max[xyz] = newMax[xyz];
                }
            }
            center = min + ((max - min) / 2f);
        }

        public void ResetCenter()
        {
            center = min + ((max - min) / 2f);
        }

        public AABB Extend(AABB other)
        {
            if (min == max)
                return other;
            if (other.min.X < min.X)
                min.X = other.min.X;
            if (other.min.Y < min.Y)
                min.Y = other.min.Y;
            if (other.min.Z < min.Z)
                min.Z = other.min.Z;
            if (other.max.X > max.X)
                max.X = other.max.X;
            if (other.max.Y > max.Y)
                max.Y = other.max.Y;
            if (other.max.Z > max.Z)
                max.Z = other.max.Z;
            return this;
        }

        public float LongestAxis(out int index)
        {
            Vector3 diff = max - min;
            if (diff.X > diff.Y)
            {
                if (diff.X > diff.Z)
                {
                    index = 0;
                    return diff.X;
                }
                index = 2;
                return diff.Z;
            }
            if (diff.Y > diff.Z)
            {
                index = 1;
                return diff.Y;
            }
            index = 2;
            return diff.Z;
        }

        public float SurfaceArea()
        {
            Vector3 diff = max - min;
            return
                diff.X * diff.Y * 2 +
                diff.X * diff.Z * 2 +
                diff.Z * diff.Y * 2;
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

        public AABB[] SplitAABB(Primitive[] primitives)
        {
            AABB[] newAABBs = new AABB[2];
            AABB[] optimalAABBs = new AABB[2];

            float xLength = max.X - min.X;
            float yLength = max.Y - min.Y;
            float zLength = max.Z - min.Z;

            float primitiveCount = primitives.Length;

            float parentNodeCostX = yLength * zLength * primitiveCount;
            float parentNodeCostY = xLength * zLength * primitiveCount;
            float parentNodeCostZ = xLength * yLength * primitiveCount;
            float currentOptimal = float.PositiveInfinity;

            for (int i = 0; i < primitives.Length; i++)
            {
                Vector3 primitiveCenter = primitives[i].Center();

                //Split along x_axis
                newAABBs[0].min = min;
                newAABBs[0].max = new Vector3(primitiveCenter.X, max.Y, max.Z);
                newAABBs[1].min = new Vector3(primitiveCenter.X, min.Y, min.Z);
                newAABBs[1].max = max;

                uint primitivesLeft = newAABBs[0].Contains(primitives);
                uint primitivesRight = newAABBs[1].Contains(primitives);


                float leftCostX = (min.Y + primitiveCenter.Y) * (min.Z + primitiveCenter.Z) * primitivesLeft;
                float rightCostX = (max.Y - primitiveCenter.Y) * (max.Z - primitiveCenter.Z) * primitivesRight;

                if (leftCostX + rightCostX < parentNodeCostX && leftCostX + rightCostX < currentOptimal)
                {
                    currentOptimal = leftCostX + rightCostX;
                    optimalAABBs = newAABBs;
                }

                //Split along y_axis
                newAABBs[0].min = min;
                newAABBs[0].max = new Vector3(max.X, primitiveCenter.Y, max.Z);
                newAABBs[1].min = new Vector3(min.X, primitiveCenter.Y, min.Z);
                newAABBs[1].max = max;


                primitivesLeft = newAABBs[0].Contains(primitives);
                primitivesRight = newAABBs[1].Contains(primitives);

                float leftCostY = (min.X + primitiveCenter.X) * (min.Z + primitiveCenter.Z) * primitivesLeft;
                float rightCostY = (max.X - primitiveCenter.X) * (max.Z - primitiveCenter.Z) * primitivesRight;

                if (leftCostY + rightCostY < parentNodeCostY && leftCostY + rightCostY < currentOptimal)
                {
                    currentOptimal = leftCostY + rightCostY;
                    optimalAABBs = newAABBs;
                }

                //Split along z_axis
                newAABBs[0].min = min;
                newAABBs[0].max = new Vector3(max.X, max.Y, primitiveCenter.Z);
                newAABBs[1].min = new Vector3(min.X, min.Y, primitiveCenter.Z);
                newAABBs[1].max = max;


                primitivesLeft = newAABBs[0].Contains(primitives);
                primitivesRight = newAABBs[1].Contains(primitives);

                float leftCostZ = (min.X + primitiveCenter.X) * (min.Y + primitiveCenter.Y) * primitivesLeft;
                float rightCostZ = (max.X - primitiveCenter.X) * (max.Y - primitiveCenter.Y) * primitivesRight;

                if (leftCostZ + rightCostZ < parentNodeCostZ && leftCostZ + rightCostZ < currentOptimal)
                {
                    currentOptimal = leftCostZ + rightCostZ;
                    optimalAABBs = newAABBs;
                }
            }
            return optimalAABBs;
        }

        public bool Contains(Vector3 p)
        {
            return
                min.X <= p.X && min.Y <= p.Y && min.Z <= p.Z &&
                max.X >= p.X && max.Y >= p.Y && max.Z >= p.Z;
        }

        public uint Contains(Primitive[] primitives)
        {
            uint count = 0;
            for (int i = 0; i < primitives.Length; i++)
            {
                if (Contains(primitives[i].Center()))
                {
                    count++;
                }
            }
            return count;
        }

        public bool Intersect(Ray ray)
        {
            float t1 = (min.X - ray.origin.X) * ray.invDir.X;
            float t2 = (max.X - ray.origin.X) * ray.invDir.X;

            float t3 = (min.Y - ray.origin.Y) * ray.invDir.Y;
            float t4 = (max.Y - ray.origin.Y) * ray.invDir.Y;

            float t5 = (min.Z - ray.origin.Z) * ray.invDir.Z;
            float t6 = (max.Z - ray.origin.Z) * ray.invDir.Z;
            float tmax = Calc.Min(Calc.Min(Calc.Max(t1, t2), Calc.Max(t3, t4)), Calc.Max(t5, t6));

            // if tmax < 0, ray (line) is intersecting AABB, but the whole AABB is behind us
            if (tmax < 0)
            {
                //t = tmax;
                return false;
            }

            float tmin = Calc.Max(Calc.Max(Calc.Min(t1, t2), Calc.Min(t3, t4)), Calc.Min(t5, t6));

            // if tmin > tmax, ray doesn't intersect AABB
            if (tmin > tmax)
            {
                //t = tmax;
                return false;
            }

            //t = tmin;
            return true;
        }
    }
}
