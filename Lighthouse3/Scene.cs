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

namespace Lighthouse3
{
    public class Scene
    {
        public Vector3 backgroundColor;
        public Light[] lights;
        public Primitive[] primitives;

        public static Scene BasicScene()
        {
            Scene scene = new Scene();
            scene.backgroundColor = Color.Black;
            scene.lights = new Light[]
            {
                new PointLight(new Vector3(20, 75, -10), Color.White, 2000)
            };
            Material yellow = Material.Yellow;
            yellow.isCheckerboard = true;

            scene.primitives = new Primitive[]
            { 
				//new Sphere(new Vector3(4, 0, 7), 2),
				//new Sphere(new Vector3(-3, 3.5f, 8), 1.5f, red),
				//new Plane(new Vector3(0, -5, 0), new Vector3(0, 1, 0), Material.Blue),
                //new Plane(new Vector3(0, 0, 10), new Vector3(0, 0, -1), yellow),
                //new Plane(new Vector3(0, 0, 11), new Vector3(0, 0, -1), green),

                //new Sphere(new Vector3(0, 0, 5), 3f, new Material(Color.Red, 1))
                //new Triangle(new Vector3(2, 0, 10), new Vector3(-2, 0, 10), new Vector3(0, 3, 10), red),
                //new Triangle(new Vector3(-2, 3, 10), new Vector3(-1, 1, 10), new Vector3(1, 2, 10), red)
				//new Sphere(new Vector3(0, 5, 20), 1),
				//new Sphere(new Vector3(3, 3, 10), 4)
			};
            scene.primitives = scene.primitives.Concat(ObjectLoader.GetObjTriangles("../../assets/basic_box.obj")).ToArray();
            //scene.primitives = scene.primitives.Concat(ObjectLoader.GetObjTriangles("../../assets/complex_box.obj")).ToArray(); // Same as basic_box but with a torus on top of the red box

            return scene;
        }
    }
}
