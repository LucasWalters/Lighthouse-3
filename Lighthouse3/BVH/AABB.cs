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
        }

        public AABB Extend(AABB other)
        {
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

        public bool Intersect(Ray ray)
        {
            //for (int xyz = 0; xyz < 3; xyz++)
            //{
            //    float invD = 1f / ray.direction[xyz];
            //    float t0 = (min[xyz] - ray.origin[xyz]) * invD;
            //    float t1 = (max[xyz] - ray.origin[xyz]) * invD;

            //    if (invD < 0)
            //    {
            //        float temp = t1;
            //        t1 = t0;
            //        t0 = temp;
            //    }

            //    if (t1 <= t0)
            //        return false;
            //}
            //return true;

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






            //float tmin = (min.X - ray.origin.X) / ray.direction.X;
            //float tmax = (max.X - ray.origin.X) / ray.direction.X;

            //if (tmin > tmax)
            //{
            //    float temp = tmin;
            //    tmin = tmax;
            //    tmax = temp;
            //}

            //float tymin = (min.Y - ray.origin.Y) / ray.direction.Y;
            //float tymax = (max.Y - ray.origin.Y) / ray.direction.Y;

            //if (tymin > tymax)
            //{
            //    float temp = tymin;
            //    tymin = tymax;
            //    tymax = temp;
            //}

            //if ((tmin > tymax) || (tymin > tmax))
            //    return false;

            //if (tymin > tmin)
            //    tmin = tymin;

            //if (tymax < tmax)
            //    tmax = tymax;

            //float tzmin = (min.Z - ray.origin.Z) / ray.direction.Z;
            //float tzmax = (max.Z - ray.origin.Z) / ray.direction.Z;

            //if (tzmin > tzmax)
            //{
            //    float temp = tzmin;
            //    tzmin = tzmax;
            //    tzmax = temp;
            //}

            //if ((tmin > tzmax) || (tzmin > tmax))
            //    return false;

            ////if (tzmin > tmin)
            ////    tmin = tzmin;

            ////if (tzmax < tmax)
            ////    tmax = tzmax;

            //return true;
        }
    }
}
