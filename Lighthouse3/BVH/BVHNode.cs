using System;
using OpenTK;

namespace Lighthouse3.BVH
{
    public struct BVHNode
    {
        public AABB bounds;
        public int leftFirst;
        public int count;
    }

    public struct AABB
    {
        public Vector3 topLeft;
        public Vector3 bottomRight;

        public bool Contains(Vector3 p)
        {
            //TODO
            return false;
        }
    }
}
