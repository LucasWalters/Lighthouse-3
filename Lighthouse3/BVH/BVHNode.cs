using System;
using Lighthouse3.Primitives;
using OpenTK;

namespace Lighthouse3.BVH
{
    public struct BVHNode
    {
        public AABB bounds;
        public int leftFirst;
        public int count;
        public Primitive[] primitives;

        // TODO: poolpointer needs to be static along all calls of Subdivide
        public void Subdivide(int poolPointer, uint[] indices, BVHNode[] pool)
        {
            if (count > 3) return;
            this.leftFirst = poolPointer++;
            poolPointer++; // Poolpointer needs to be incremented twice to account for the value on the right

            // Split AABB and assign left and right
            AABB[] newAABBs = bounds.SplitAABB();
            pool[leftFirst].bounds = newAABBs[0];
            pool[leftFirst + 1].bounds = newAABBs[1];


            // Partition
            Primitive[] leftPrimitives = new Primitive[primitives.Length / 2 + 1];
            Primitive[] rightPrimitives = new Primitive[primitives.Length / 2 + 1];

            for (int i = 0; i < primitives.Length; i++)
            {
                if (pool[leftFirst].bounds.Contains(primitives[i].Center()))
                {
                    leftPrimitives[i] = primitives[i];
                } else
                {
                    rightPrimitives[i] = primitives[i];
                }
            }
            pool[leftFirst].primitives = leftPrimitives;
            pool[leftFirst].count = leftPrimitives.Length;;
            pool[leftFirst + 1].primitives = rightPrimitives;
            pool[leftFirst + 1].count = rightPrimitives.Length;

            pool[leftFirst].Subdivide(poolPointer, indices, pool);
            pool[leftFirst+1].Subdivide(poolPointer, indices, pool);
        }
        
    }

    
}
