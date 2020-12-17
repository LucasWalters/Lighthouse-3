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

        public static readonly int numberOfSplitPlanes = 8;
        public static readonly float invNumberOfSplitPlanes = 1f / (numberOfSplitPlanes + 1);
        public static readonly int maxPrimsPerNode = 5;

        private void Swap (Primitive[] primitives, int i, int j)
        {
            Primitive temp = primitives[i];
            primitives[i] = primitives[j];
            primitives[j] = temp;
        }

        public void Subdivide(AABB[] primBounds, int[] indices, BVHNode[] nodes, ref int nodeIndex)
        {
            if (count <= maxPrimsPerNode) return;

            int axis;
            float axisSize = bounds.LongestAxis(out axis);
            float currentCost = bounds.SurfaceArea() * count;
            float bestSplitPoint = 0f;
            float bestSplitCost = float.MaxValue;
            AABB bestLeftBounds = new AABB();
            AABB bestRightBounds = new AABB();
            int countLeft = 0;
            AABB leftBounds = new AABB();
            bool splitting = false;
            int[] sorted = new int[count];
            for (int i = 0; i < count; i++)
                sorted[i] = indices[firstOrLeft + i];


            for (int splitIndex = 0; splitIndex < numberOfSplitPlanes; splitIndex++)
            {
                float splitPoint = bounds.min[axis] + axisSize * invNumberOfSplitPlanes * (splitIndex + 1);

                AABB rightBounds = new AABB();
                int countRight = count - 1;

                for (int i = countLeft; i < count; i++)
                {
                    int index = sorted[i];

                    if (primBounds[index].center[axis] < splitPoint)
                    {
                        leftBounds = leftBounds.Extend(primBounds[index]);
                        sorted[i] = sorted[countLeft];
                        sorted[countLeft] = index;
                        countLeft++;
                    }
                    else
                    {
                        rightBounds = rightBounds.Extend(primBounds[index]);
                        sorted[i] = sorted[countRight];
                        sorted[countRight] = index;
                        countRight--;
                    }
                }

                float leftCost = countLeft * leftBounds.SurfaceArea();
                float rightCost = (count - countLeft) * rightBounds.SurfaceArea();

                if (leftCost + rightCost >= currentCost)
                    break;

                splitting = true;

                if (leftCost + rightCost < bestSplitCost)
                {
                    bestSplitPoint = splitPoint;
                    bestSplitCost = leftCost + rightCost;
                    bestLeftBounds = leftBounds;
                    bestRightBounds = rightBounds;
                }

            }

            // If one of the children is empty, change bounds and resplit
            if (!splitting)
            {
                //bounds = countLeft == 0 ? rightBounds : leftBounds;
                //Subdivide(primitives, indices, nodes, ref nodeIndex);
                return;
            }
            int bestCountLeft = 0;
            int bestCountRight = count - 1;
            for (int i = countLeft; i < count; i++)
            {
                int index = indices[firstOrLeft + i];
                if (primBounds[index].center[axis] < bestSplitPoint)
                    sorted[bestCountLeft++] = index;
                else
                    sorted[bestCountRight--] = index;
            }

            // Overwrite primitives in global array with sorted ones
            for (int i = 0; i < count; i++)
            {
                indices[firstOrLeft + i] = sorted[i];
            }

            // Give first index and count to child nodes
            nodes[nodeIndex].firstOrLeft = firstOrLeft;
            nodes[nodeIndex].count = countLeft;
            nodes[nodeIndex].bounds = bestLeftBounds;
            nodes[nodeIndex + 1].firstOrLeft = firstOrLeft + countLeft;
            nodes[nodeIndex + 1].count = count - countLeft;
            nodes[nodeIndex + 1].bounds = bestRightBounds;

            //Console.WriteLine("Allocated " + countLeft + " to left");
            //Console.WriteLine("Allocated " + (count - countLeft) + " to right");

            // first is now left and count is -1 because we no longer have primitives
            firstOrLeft = nodeIndex;
            count = -1;

            // Recursively subdivide child nodes while keeping track of the nodeIndex
            int newIndex = nodeIndex + 2;
            nodes[nodeIndex].Subdivide(primBounds, indices, nodes, ref newIndex);
            nodes[nodeIndex + 1].Subdivide(primBounds, indices, nodes, ref newIndex);

            nodeIndex = newIndex;
        }

        public Primitive[] GetPrimitivesOfNode(Primitive[] primitives)
        {
            Primitive[] nodePrimitives = new Primitive[count];
            for(int i = 0; i < count; i++)
            {
                nodePrimitives[i] = primitives[firstOrLeft + i];
            }
            return nodePrimitives;
        }
    }
}
