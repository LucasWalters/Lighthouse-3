using System;
using Lighthouse3.Primitives;
using OpenTK;

namespace Lighthouse3.BVH
{
    public struct BVHNode
    {
        public AABB bounds;
        public int firstOrLeft;
        public int count;

        public static readonly int MaxPrimsPerNode = 3;

        private void Swap (Primitive[] primitives, int i, int j)
        {
            Primitive temp = primitives[i];
            primitives[i] = primitives[j];
            primitives[j] = temp;
        }

        // TODO: poolpointer needs to be static along all calls of Subdivide
        public void Subdivide(Primitive[] primitives, int[] indices, BVHNode[] nodes, ref int nodeIndex)
        {
            if (count < MaxPrimsPerNode) return;

            // Split AABB and assign left and right
            AABB[] newAABBs = bounds.SplitAABB();
            nodes[nodeIndex].bounds = newAABBs[0];
            nodes[nodeIndex + 1].bounds = newAABBs[1];
            
            // Sort primitives based on whether they are in the left bounds or not
            int[] sorted = new int[count];
            int countLeft = 0;
            int countRight = count-1;
            for (int i = 0; i < count; i++)
            {
                bool left = newAABBs[0].Contains(primitives[firstOrLeft + i].Center());
                if (left)
                    sorted[countLeft++] = firstOrLeft + i;
                else
                    sorted[countRight--] = firstOrLeft + i;
            }

            // Overwrite primitives in global array with sorted ones
            for (int i = 0; i < count; i++)
            {
                indices[firstOrLeft + i] = sorted[i];
            }

            // Give first index and count to child nodes
            nodes[nodeIndex].firstOrLeft = firstOrLeft;
            nodes[nodeIndex].count = countLeft;
            nodes[nodeIndex + 1].firstOrLeft = firstOrLeft + countLeft;
            nodes[nodeIndex + 1].count = count - countLeft;

            // first is now left and count is 0 because we no longer have primitives
            firstOrLeft = nodeIndex;
            count = 0;

            // Recursively subdivide child nodes while keeping track of the nodeIndex
            int newIndex = nodeIndex + 2;
            nodes[nodeIndex].Subdivide(primitives, indices, nodes, ref newIndex);
            nodes[nodeIndex + 1].Subdivide(primitives, indices, nodes, ref newIndex);
            nodeIndex = newIndex;
        }
    }
}
