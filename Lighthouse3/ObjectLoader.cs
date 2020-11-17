using System;
using System.IO;
using ObjLoader.Loader.Loaders;
using ObjLoader.Loader.Common;
using ObjLoader.Loader.Data;
using ObjLoader.Loader.TypeParsers;

namespace Lighthouse3
{
    public class ObjectLoader
    {
        public static LoadResult LoadObj(string path)
        {
            var objLoaderFactory = new ObjLoaderFactory();
            var objLoader = objLoaderFactory.Create();
            LoadResult result = objLoader.Load(File.OpenRead("../../assets/teapot.obj"));
            Console.WriteLine(result.Vertices.Count);
            Console.WriteLine(result.Textures.Count);
            Console.WriteLine(result.Normals.Count);
            Console.WriteLine(result.Groups.Count);
            Console.WriteLine(result.Materials.Count);
            return result;
        }
    }
}
