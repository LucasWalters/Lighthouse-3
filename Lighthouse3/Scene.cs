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
        public Camera mainCamera;
        public Vector3 backgroundColor;
        public Light[] lights;
        public Primitive[] primitives;

        public static Scene BasicScene()
        {
            Scene scene = new Scene();
            scene.mainCamera = new Camera(new Vector3(-50, 75, -10), new Vector3(1, 0, 0).Normalized(), Game.SCREEN_WIDTH, Game.SCREEN_HEIGHT, 1f);
            scene.backgroundColor = Color.Black;
            scene.lights = new Light[]
            {
                new PointLight(new Vector3(20, 75, -10), Color.White, 1)
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

        public static Scene MirrorScene()
        {
            Scene scene = new Scene();
            scene.mainCamera = new Camera(new Vector3(0, 0, 0), new Vector3(0, 0, 1), Game.SCREEN_WIDTH, Game.SCREEN_HEIGHT, 1.5f, 16);
            scene.backgroundColor = Color.Black;
            scene.lights = new Light[]
            {
                new PointLight(new Vector3(0, 5, 0), Color.White, 100)
            };
            Material checkerboard = Material.Yellow;
            checkerboard.isCheckerboard = true;

            scene.primitives = new Primitive[]
            { 
				//new Sphere(new Vector3(4, 0, 7), 2),
				//new Sphere(new Vector3(-3, 3.5f, 8), 1.5f, red),
				new Plane(new Vector3(-10, 0, 0), new Vector3(1, 0, 0), Material.Blue),
                new Plane(new Vector3(0, 0, 10), new Vector3(0, 0, -1), checkerboard),
                new Plane(new Vector3(10, 0, 0), new Vector3(-1, 0, 0), Material.Red),
                new Plane(new Vector3(0, -5, 0), new Vector3(0, 1, 0), Material.Green),
                new Sphere(new Vector3(0, 0, 5), 2f, Material.Mirror)

                //new Sphere(new Vector3(0, 0, 5), 3f, new Material(Color.Red, 1))
                //new Triangle(new Vector3(2, 0, 10), new Vector3(-2, 0, 10), new Vector3(0, 3, 10), red),
                //new Triangle(new Vector3(-2, 3, 10), new Vector3(-1, 1, 10), new Vector3(1, 2, 10), red)
				//new Sphere(new Vector3(0, 5, 20), 1),
				//new Sphere(new Vector3(3, 3, 10), 4)
			};
            return scene;

        }
    }
}
