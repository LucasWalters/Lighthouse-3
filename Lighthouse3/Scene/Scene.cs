using Lighthouse3.BVH;
using Lighthouse3.Lights;
using Lighthouse3.Primitives;
using ObjLoader.Loader.Data.Elements;
using ObjLoader.Loader.Loaders;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse3.Scene
{
    public class Scene
    {
        public Camera mainCamera;
        public Vector3 backgroundColor;
        public Light[] lights;
        public Primitive[] primitives;

        public static Scene CURRENT_SCENE = StandardScenes.KajiyaScene();


        public BVHNode[] nodes;



        public void CalculateBVH()
        {
            int N = primitives.Length;
            uint[] indices = new uint[N];
            for (uint i = 0; i < N; i++)
                indices[i] = i;

            nodes = new BVHNode[N * 2 - 1];
        }
    }
}
