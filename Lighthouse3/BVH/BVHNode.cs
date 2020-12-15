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

    
}
