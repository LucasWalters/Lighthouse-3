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

            // Iterate over each object in the OBJ file
            for (int i = 0; i < loadResult.Groups.Count; i++)
            {

                // Iterate over all faces of the object
                int amount = loadResult.Groups[i].Faces.Count;
                for (int j = 0; j < amount; j++)
                {
                    List<Vector3> points = new List<Vector3>();

                    //Iterate over each face
                    Face face = loadResult.Groups[i].Faces[j];
                    for (int k = 0; k < face.Count-1; k++)
                    {
                        Vertex point = loadResult.Vertices[face[k].VertexIndex - 1];
                        triangles.Add(new Triangle(ObjectLoader.VertexToVector3(loadResult.Vertices[face[0].VertexIndex - 1]), 
                            ObjectLoader.VertexToVector3(loadResult.Vertices[face[k].VertexIndex - 1]), 
                            ObjectLoader.VertexToVector3(loadResult.Vertices[face[k+1].VertexIndex - 1]), MtlToMaterial(loadResult.Groups[i].Material)));
                    } 
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
            return new Material(material.DiffuseColor.X, material.DiffuseColor.Y, material.DiffuseColor.Z, 0);
        }

    }
}
