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

        public static readonly int numberOfBins = 8;
        //public static readonly float invNumberOfSplitPlanes = 1f / (numberOfBins + 1);
        public static readonly int maxPrimsPerNode = 1;

        private float k1;
        private float k0;
        private int splittingAxis;
        private float axisSize;

        public BVHNode(AABB bounds, AABB centroidBounds, int firstOrLeft, int count)
        {
            this.bounds = bounds;
            this.centroidBounds = centroidBounds;
            this.firstOrLeft = firstOrLeft;
            this.count = count;
            axisSize = centroidBounds.LongestAxis(out splittingAxis);

            k1 = numberOfBins * (1f - Calc.Epsilon) / axisSize;
            k0 = centroidBounds.min[splittingAxis];
            childIndices = new int[4];
        }

        public BVHNode InitSplittingAxis()
        {
            axisSize = centroidBounds.LongestAxis(out splittingAxis);

            k1 = numberOfBins * (1f - Calc.Epsilon) / axisSize;
            k0 = centroidBounds.min[splittingAxis];
            childIndices = new int[4];
            return this;
        }

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

        // Subdivide based on: https://www.researchgate.net/publication/4278645_On_Fast_Construction_of_SAH_based_Bounding_Volume_Hierarchies
        public void Subdivide(AABB[] primBounds, int[] indices, BVHNode[] nodes, ref int nodeIndex)
        {
            if (count <= maxPrimsPerNode) return;


            if (axisSize < numberOfBins * Calc.Epsilon)
                return;

            float optimalSplitCost = bounds.SurfaceArea() * count + Calc.Epsilon;
            AABB optimalLeftBounds = new AABB();
            AABB optimalRightBounds = new AABB();
            int optimalLeftCount = 0;
            int[] optimalIndices = new int[count];

            bool splitting = false;

            int leftTrigs = 0;
            AABB leftBounds = new AABB();

            for (int binIndex = 1; binIndex < numberOfBins + 1; binIndex++)
            {
                AABB rightBounds = new AABB();
                int rightIndex = count - 1;
                for (int i = leftTrigs; i <= rightIndex; i++)
                {
                    Vector3 centroid1 = primBounds[indices[i + firstOrLeft]].center;
                    float binID1 = k1 * (centroid1[splittingAxis] - k0);

                    //Belongs to the left
                    if (binID1 < binIndex)
                    {
                        leftBounds = leftBounds.Extend(primBounds[indices[i + firstOrLeft]]);
                        leftTrigs++;
                    }
                    else
                    {
                        for (int j = rightIndex; j >= i; j--)
                        {
                            if (j == i)
                            {
                                rightBounds = rightBounds.Extend(primBounds[indices[j + firstOrLeft]]);
                                break;
                            }
                            rightIndex = j - 1;
                            Vector3 centroid2 = primBounds[indices[j + firstOrLeft]].center;
                            float binID2 = k1 * (centroid2[splittingAxis] - k0);


                            //Belongs to the left, swap 
                            if (binID2 < binIndex)
                            {
                                int temp = indices[i + firstOrLeft];
                                indices[i + firstOrLeft] = indices[j + firstOrLeft];
                                indices[j + firstOrLeft] = temp;

                                leftBounds = leftBounds.Extend(primBounds[indices[i + firstOrLeft]]);
                                leftTrigs++;
                                rightBounds = rightBounds.Extend(primBounds[indices[j + firstOrLeft]]);
                                break;
                            }
                            else
                            {
                                rightBounds = rightBounds.Extend(primBounds[indices[j + firstOrLeft]]);
                            }

                        }
                    }
                }

                if (leftTrigs <= 0 || leftTrigs >= count)
                    continue;

                float leftCost = leftTrigs * leftBounds.SurfaceArea();
                float rightCost = (count - leftTrigs) * rightBounds.SurfaceArea();
                float cost = leftCost + rightCost;

                if (cost < optimalSplitCost)
                {
                    splitting = true;
                    optimalLeftCount = leftTrigs;
                    optimalSplitCost = cost + Calc.Epsilon;
                    optimalLeftBounds = leftBounds;
                    optimalRightBounds = rightBounds;
                    for (int i = 0; i < count; i++)
                    {
                        optimalIndices[i] = indices[firstOrLeft + i];
                    }
                }
            }

            // If one of the children is empty, change bounds and resplit
            if (!splitting)
            {
                return;
            }

            // Overwrite primitives in global array with sorted ones
            for (int i = 0; i < count; i++)
            {
                indices[firstOrLeft + i] = optimalIndices[i];
            }
            // Give first index and count to child nodes
            nodes[nodeIndex].firstOrLeft = firstOrLeft;
            nodes[nodeIndex].count = optimalLeftCount;
            nodes[nodeIndex].bounds = optimalLeftBounds;
            nodes[nodeIndex].centroidBounds = nodes[nodeIndex].CentroidAABB(primBounds, indices);
            nodes[nodeIndex] = nodes[nodeIndex].InitSplittingAxis();
            nodes[nodeIndex + 1].firstOrLeft = firstOrLeft + optimalLeftCount;
            nodes[nodeIndex + 1].count = count - optimalLeftCount;
            nodes[nodeIndex + 1].bounds = optimalRightBounds;
            nodes[nodeIndex + 1].centroidBounds = nodes[nodeIndex + 1].CentroidAABB(primBounds, indices);
            nodes[nodeIndex + 1] = nodes[nodeIndex + 1].InitSplittingAxis();


            childIndices[0] = nodeIndex;
            childIndices[1] = nodeIndex + 1;

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
