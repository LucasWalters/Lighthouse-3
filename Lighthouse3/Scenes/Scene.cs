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
        public static Scene CURRENT_SCENE = StandardScenes.KajiyaScene();

        public BVHNode[] pool;
        
        public void CalculateBVH()
        {
            int N = primitives.Length;
            uint[] indices = new uint[N];
            for (uint i = 0; i < N; i++)
                indices[i] = i;

            pool = new BVHNode[N * 2 - 1];
            int poolPointer = 2;

            pool[0].leftFirst = 0;
            pool[0].count = N;
            pool[0].bounds = new AABB(primitives);

            pool[0].primitives = primitives; // Optimize later

            pool[0].Subdivide(poolPointer, indices, pool); // poolpointer is always 2?
        }
    }
    
}
