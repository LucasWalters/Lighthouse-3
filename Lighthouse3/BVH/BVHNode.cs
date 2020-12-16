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

        public void Subdivide(Primitive[] primitives, int[] indices, BVHNode[] nodes, ref int nodeIndex)
        {
            if (count <= MaxPrimsPerNode) return;

            // Split AABB and assign left and right
            AABB[] newAABBs = bounds.SplitAABB();
            nodes[nodeIndex].bounds = newAABBs[0];
            nodes[nodeIndex + 1].bounds = newAABBs[1];
            //Console.WriteLine("Bounds: " + bounds.min + ", " + bounds.max);
            //Console.WriteLine("Left: " + newAABBs[0].min + ", " + newAABBs[0].max);
            //Console.WriteLine("Right: " + newAABBs[1].min + ", " + newAABBs[1].max);

            // Sort primitives based on whether they are in the left bounds or not
            int[] sorted = new int[count];
            int countLeft = 0;
            int countRight = count-1;
            for (int i = 0; i < count; i++)
            {
                int index = indices[firstOrLeft + i];
                bool left = newAABBs[0].Contains(primitives[index].Center());
                //bool right = newAABBs[1].Contains(center);
                if (left)
                    sorted[countLeft++] = index;
                else
                    sorted[countRight--] = index;
                //else
                //{
                //    Console.WriteLine("NOT IN LEFT OR RIGHT!");
                //    Console.WriteLine("Center " + index + ": " + center);
                //}
            }

            // If one of the children is empty, change bounds and resplit
            if (countLeft == count || countLeft == 0)
            {
                bounds = newAABBs[countLeft == 0 ? 1 : 0];
                Subdivide(primitives, indices, nodes, ref nodeIndex);
                return;
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

            //Console.WriteLine("Allocated " + countLeft + " to left");
            //Console.WriteLine("Allocated " + (count - countLeft) + " to right");

            // first is now left and count is -1 because we no longer have primitives
            firstOrLeft = nodeIndex;
            count = -1;

            // Recursively subdivide child nodes while keeping track of the nodeIndex
            int newIndex = nodeIndex + 2;
            nodes[nodeIndex].Subdivide(primitives, indices, nodes, ref newIndex);
            nodes[nodeIndex + 1].Subdivide(primitives, indices, nodes, ref newIndex);

            nodeIndex = newIndex;
        }
    }
}
