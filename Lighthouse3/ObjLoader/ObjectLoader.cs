using System;
using ObjLoader.Loader;

namespace Lighthouse3
{
    public class ObjectLoader
    {
        public static readFile(string path)
        {
            var objLoaderFactory = new ObjectLoaderFactory();
            var objLoader = objLoaderFactory.Create();
            var fileStream = new FileStream(path);
            LoadResult result = objLoader.Load(fileStream);
            return result;
        }
    }
}
