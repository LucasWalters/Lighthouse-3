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


        public static readonly int MaxPrimsPerNode = 5;

        private void Swap (Primitive[] primitives, int i, int j)
        {
            Primitive temp = primitives[i];
            primitives[i] = primitives[j];
            primitives[j] = temp;
        }

        public void Subdivide(Primitive[] primitives, int[] indices, BVHNode[] nodes, ref int nodeIndex)
        {
            if (count <= MaxPrimsPerNode) return;

            //TODO split place instead of AABB
            AABB[] newAABBs = bounds.SplitAABB(primitives);
            //Console.WriteLine("Bounds: " + bounds.min + ", " + bounds.max);
            //Console.WriteLine("Left: " + newAABBs[0].min + ", " + newAABBs[0].max);
            //Console.WriteLine("Right: " + newAABBs[1].min + ", " + newAABBs[1].max);

            // Sort primitives based on whether they are in the left bounds or not
            int[] sorted = new int[count];
            int countLeft = 0;
            int countRight = count-1;
            AABB leftBounds = newAABBs[0];
            AABB rightBounds = newAABBs[1];
            for (int i = 0; i < count; i++)
            {
                int index = indices[firstOrLeft + i];
                Vector3 center = primitives[index].Center();
                bool left = newAABBs[0].Contains(center);
                bool right = newAABBs[1].Contains(center);
                if (left)
                {
                    sorted[countLeft++] = index;
                    leftBounds = leftBounds.Extend(primitives[index].bounds);
                }
                else if (right)
                {
                    sorted[countRight--] = index;
                    rightBounds = rightBounds.Extend(primitives[index].bounds);

                }
                else
                {
                    Console.WriteLine("NOT IN LEFT OR RIGHT!");
                    Console.WriteLine("Center " + index + ": " + center);
                }
            }

            // If one of the children is empty, change bounds and resplit
            if (countLeft == count || countLeft == 0)
            {
                //bounds = countLeft == 0 ? rightBounds : leftBounds;
                //Subdivide(primitives, indices, nodes, ref nodeIndex);
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
            nodes[nodeIndex].bounds = leftBounds;
            nodes[nodeIndex + 1].firstOrLeft = firstOrLeft + countLeft;
            nodes[nodeIndex + 1].count = count - countLeft;
            nodes[nodeIndex + 1].bounds = rightBounds;

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
