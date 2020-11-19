using System;
using System.IO;
using ObjLoader.Loader.Loaders;
using OpenTK;
using ObjLoader.Loader.Data.VertexData;
using Lighthouse3.Primitives;
using ObjLoader.Loader.Data.Elements;
using System.Collections.Generic;

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


            List<Triangle> triangles = new List<Triangle>();
            for (int i = 0; i < loadResult.Groups.Count; i++)
            {
                Console.WriteLine(loadResult.Groups[i].Faces.Count);
                int amount = loadResult.Groups[i].Faces.Count;
                for (int j = 0; j < amount; j++)
                {

                    Face face = loadResult.Groups[i].Faces[j];
                    Vector3 point1 = ObjectLoader.VertexToVector3(loadResult.Vertices[face[0].VertexIndex - 1]);
                    Vector3 point2 = ObjectLoader.VertexToVector3(loadResult.Vertices[face[1].VertexIndex - 1]);
                    Vector3 point3 = ObjectLoader.VertexToVector3(loadResult.Vertices[face[2].VertexIndex - 1]);
                    triangles.Add(new Triangle(point1, point2, point3, MtlToMaterial(loadResult.Groups[i].Material)));
                }
            }
            
            return triangles.ToArray();
        }

        public static Vector3 VertexToVector3(Vertex vertex)
        {
            return new Vector3(vertex.X, vertex.Y, vertex.Z);
        }

        private static Material MtlToMaterial(ObjLoader.Loader.Data.Material material)
        {
            Console.WriteLine(material.DiffuseColor.X);
            return new Material(material.DiffuseColor.X, material.DiffuseColor.Y, material.DiffuseColor.Z, 0);
        }
    }
}
