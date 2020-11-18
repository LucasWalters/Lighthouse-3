using System;
using System.IO;
using ObjLoader.Loader.Loaders;
using OpenTK;
using ObjLoader.Loader.Data.VertexData;
using Lighthouse3.Primitives;
using ObjLoader.Loader.Data.Elements;

namespace Lighthouse3
{
    public class ObjectLoader
    {
        public static LoadResult LoadObj(string path)
        {
            var objLoaderFactory = new ObjLoaderFactory();
            var objLoader = objLoaderFactory.Create();
            LoadResult result = objLoader.Load(File.OpenRead(path));
            return result;
        }

        public static Triangle[] GetObjTriangles(string path)
        {
            LoadResult loadResult = LoadObj(path);

            int amount = loadResult.Groups[0].Faces.Count;
            Triangle[] triangles = new Triangle[amount];
            for (int i = 0; i < amount; i++)
            {

                Face face = loadResult.Groups[0].Faces[i];
                Vector3 point1 = ObjectLoader.VertexToVector3(loadResult.Vertices[face[0].VertexIndex - 1]);
                Vector3 point2 = ObjectLoader.VertexToVector3(loadResult.Vertices[face[1].VertexIndex - 1]);
                Vector3 point3 = ObjectLoader.VertexToVector3(loadResult.Vertices[face[2].VertexIndex - 1]);
                triangles[i] = new Triangle(point1, point2, point3, Material.Red);
            }
            return triangles;
        }

        public static Vector3 VertexToVector3(Vertex vertex)
        {
            return new Vector3(vertex.X, vertex.Y, vertex.Z);
        }
    }
}
