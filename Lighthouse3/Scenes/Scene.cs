using Lighthouse3.BVH;
using Lighthouse3.Lights;
using Lighthouse3.Primitives;
using ObjLoader.Loader.Data.Elements;
using ObjLoader.Loader.Loaders;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse3.Scenes
{
    public class Scene
    {
        public Camera mainCamera;
        public Vector3 backgroundColor;
        public Light[] lights;
        public Primitive[] primitives;
        public int[] indices;
        //public static Scene CURRENT_SCENE = StandardScenes.OBJScene();

        public BVHNode[] nodes;
        public bool hasBVH = false;
        
        public void CalculateBVH()
        {
            int N = primitives.Length;
            indices = new int[N];
            AABB[] primBounds = new AABB[N];
            nodes = new BVHNode[N * 2 - 1];
            nodes[0].firstOrLeft = 0;
            nodes[0].count = N;
            //nodes[0].bounds = new AABB(primitives);
            for (int i = 0; i < N; i++)
            {
                indices[i] = i;
                primBounds[i] = primitives[i].bounds;
                nodes[0].bounds = nodes[0].bounds.Extend(primitives[i].bounds);
            }

            int index = 1; // Index to first child node

            nodes[0].Subdivide(primBounds, indices, nodes, ref index);
            hasBVH = true;

            Console.WriteLine("BVH Done");
        }
    }
    
}
