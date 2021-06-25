using Lighthouse3.BVH;
using Lighthouse3.Lights;
using Lighthouse3.Primitives;
using ObjLoader.Loader.Data.Elements;
using ObjLoader.Loader.Loaders;
using System.Numerics;
using OpenTK.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse3.Scenes
{
    public class Scene
    {
        public Camera mainCamera;
        public Vector3 backgroundColor;
        public Light[] lights = new Light[0];
        public Primitive[] primitives = new Primitive[0];
        public PlanePrim[] planes = new PlanePrim[0];
        public int[] indices;

        public BVHNode[] nodes;
        public bool hasBVH = false;
        
        public void CalculateBVH(bool collapsed4Way, bool stats)
        {
            int N = primitives.Length;
            Stopwatch sw = null;
            if (stats)
            {
                Console.WriteLine("Starting BVH generation. Scene size: " + N + " primitives.");
                sw = Stopwatch.StartNew();
            }
            indices = new int[N];
            AABB[] primBounds = new AABB[N];
            nodes = new BVHNode[N * 2 - 1];
            nodes[0].firstOrLeft = 0;
            nodes[0].count = N;
            for (int i = 0; i < N; i++)
            {
                indices[i] = i;
                primBounds[i] = primitives[i].bounds;
                nodes[0].bounds = nodes[0].bounds.Extend(primBounds[i]);
            }
            int index = 1; // Index to first child node
            nodes[0].bounds = nodes[0].bounds.ResetCenter();
            nodes[0].centroidBounds = nodes[0].CentroidAABB(primBounds, indices);
            nodes[0] = nodes[0].InitSplittingAxis();
            nodes[0].Subdivide(primBounds, indices, nodes, ref index);

            // Collapse into 4-way BVH
            if (collapsed4Way)
                nodes[0].CollapseBVH(nodes, 0);
            hasBVH = true;

            if (stats)
            {
                sw.Stop();
                Console.WriteLine("Finished BVH generation after: " + sw.ElapsedMilliseconds + " ms");
                int nrOfLeafs = 0;
                int nr = 0;
                foreach (BVHNode node in nodes)
                {
                    if (node.count > 0)
                    {
                        nrOfLeafs++;
                        nr += node.count;
                    }
                }

                Console.WriteLine("Prims: " + N);
                Console.WriteLine("BVH: ");
                Console.WriteLine("Leafs: " + nrOfLeafs);
            }
        }
    }
    
}
