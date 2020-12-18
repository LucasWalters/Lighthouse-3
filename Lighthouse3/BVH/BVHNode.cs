using System;
using Lighthouse3.Primitives;
using OpenTK;

namespace Lighthouse3.BVH
{
    public struct BVHNode
    {
        public AABB bounds;
        public AABB centroidBounds;
        public int firstOrLeft;
        public int count;
        public int[] childIndices;

        public static readonly int numberOfSplitPlanes = 8;
        public static readonly float invNumberOfSplitPlanes = 1f / (numberOfSplitPlanes + 1);
        //public static readonly int maxPrimsPerNode = 5;

        private void Swap (Primitive[] primitives, int i, int j)
        {
            Primitive temp = primitives[i];
            primitives[i] = primitives[j];
            primitives[j] = temp;
        }

        public AABB CentroidAABB(AABB[] primBounds, int[] indices)
        {
            AABB r = new AABB();
            r.min = Vector3.Zero;
            r.max = Vector3.Zero;
            for (int i = 0; i < count; i++)
            {
                Vector3 center = primBounds[indices[firstOrLeft + i]].center;
                if (i == 0)
                {
                    r.min = center;
                    r.max = center;
                }
                else
                {
                    for (int xyz = 0; xyz < 3; xyz++)
                    {
                        if (center[xyz] < r.min[xyz])
                            r.min[xyz] = center[xyz];
                        if (center[xyz] > r.max[xyz])
                            r.max[xyz] = center[xyz];
                    }
                }
            }
            return r.ResetCenter();
        }

        public void Subdivide(AABB[] primBounds, int[] indices, BVHNode[] nodes, ref int nodeIndex)
        {
            //if (count <= maxPrimsPerNode) return;
            childIndices = new int[4];
            int axis;
            float axisSize = centroidBounds.LongestAxis(out axis);
            if (axisSize < numberOfSplitPlanes * Calc.Epsilon)
                return;
            float currentCost = bounds.SurfaceArea() * count;
            int bestCountLeft = 0;
            float bestSplitCost = float.MaxValue;
            AABB bestLeftBounds = new AABB();
            AABB bestRightBounds = new AABB();
            int[] bestSorted = null;
            bool splitting = false;
            int[] totalSorted = new int[count];
            for (int i = 0; i < count; i++)
            {
                totalSorted[i] = indices[firstOrLeft + i];
            }




            for (int splitIndex = 0; splitIndex < numberOfSplitPlanes; splitIndex++)
            {
                float splitPoint = centroidBounds.min[axis] + axisSize * invNumberOfSplitPlanes * (splitIndex + 1);

                AABB leftBounds = new AABB();
                AABB rightBounds = new AABB();
                int countLeft = 0;
                int countRight = count - 1;
                int[] sorted = new int[count];

                for (int i = 0; i < count; i++)
                {
                    int index = totalSorted[i];

                    if (primBounds[index].center[axis] < splitPoint)
                    {
                        leftBounds = leftBounds.Extend(primBounds[index]);
                        sorted[countLeft++] = index;
                    }
                    else
                    {
                        rightBounds = rightBounds.Extend(primBounds[index]);
                        sorted[countRight--] = index;
                    }
                }
                //for (int i = countLeft; i < count; i++)
                //{
                //    totalSorted[i] = sorted[i];
                //}

                float leftCost = countLeft * leftBounds.SurfaceArea();
                float rightCost = (count - countLeft) * rightBounds.SurfaceArea();

                if (leftCost + rightCost >= currentCost - Calc.Epsilon)
                    continue;

                splitting = true;

                if (leftCost + rightCost < bestSplitCost)
                {
                    bestCountLeft = countLeft;
                    bestSplitCost = leftCost + rightCost;
                    bestLeftBounds = leftBounds;
                    bestRightBounds = rightBounds;
                    bestSorted = sorted;
                }
            }

            // If one of the children is empty, change bounds and resplit
            if (!splitting)
            {
                //bounds = countLeft == 0 ? rightBounds : leftBounds;
                //Subdivide(primitives, indices, nodes, ref nodeIndex);
                return;
            }
            //int bestCountLeft = 0;
            //int bestCountRight = count - 1;
            //for (int i = countLeft; i < count; i++)
            //{
            //    int index = indices[firstOrLeft + i];
            //    if (primBounds[index].center[axis] < bestSplitPoint)
            //        sorted[bestCountLeft++] = index;
            //    else
            //        sorted[bestCountRight--] = index;
            //}

            // Overwrite primitives in global array with sorted ones
            for (int i = 0; i < count; i++)
            {
                indices[firstOrLeft + i] = bestSorted[i];
            }

            // Give first index and count to child nodes
            nodes[nodeIndex].firstOrLeft = firstOrLeft;
            nodes[nodeIndex].count = bestCountLeft;
            nodes[nodeIndex].bounds = bestLeftBounds;
            nodes[nodeIndex].centroidBounds = nodes[nodeIndex].CentroidAABB(primBounds, indices);
            nodes[nodeIndex + 1].firstOrLeft = firstOrLeft + bestCountLeft;
            nodes[nodeIndex + 1].count = count - bestCountLeft;
            nodes[nodeIndex + 1].bounds = bestRightBounds;
            nodes[nodeIndex + 1].centroidBounds = nodes[nodeIndex + 1].CentroidAABB(primBounds, indices);


            childIndices[0] = nodeIndex;
            childIndices[1] = nodeIndex + 1;

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

        public void CollapseBVH(BVHNode[] nodes, int depth)
        {
            if(depth % 2 != 0)
            {
                nodes[firstOrLeft].CollapseBVH(nodes, depth + 1);
                nodes[firstOrLeft + 1].CollapseBVH(nodes, depth + 1);
            }
            
            BVHNode childLeft = nodes[firstOrLeft];
            BVHNode childRight = nodes[firstOrLeft + 1];

            //if(childLeft.count < 0)
            {
                childIndices[0] = childLeft.firstOrLeft;
                childIndices[1] = childLeft.firstOrLeft + 1;
            }

            //if (childRight.count < 0)
            {
                childIndices[2] = childRight.firstOrLeft;
                childIndices[3] = childRight.firstOrLeft + 1;
            }

        }
    }
}
