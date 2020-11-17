using Lighthouse3.Lights;
using Lighthouse3.Primitives;
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
        public Color4 backgroundColor;
        public Light[] lights;
        public Primitive[] primitives;

        public static Scene BasicScene()
        {
            Scene scene = new Scene();
            scene.backgroundColor = Color4.Black;
            scene.lights = new Light[]
            {
                new PointLight(new Vector3(0, 8, 8), Color4.White, 1)
            };

            Material red = new Material(Color4.Red, 0);
            Material yellow = new Material(Color4.Yellow, 0);
            Material green = new Material(Color4.Green, 0);
            Material blue = new Material(Color4.PowderBlue, 0);

            scene.primitives = new Primitive[]
            { 
				//new Sphere(new Vector3(4, 0, 7), 2),
				//new Sphere(new Vector3(-3, 3.5f, 8), 1.5f, red),
				new Plane(new Vector3(0, -5, 0), new Vector3(0, 1, 0), blue),
                new Plane(new Vector3(-5, 0, 0), new Vector3(1, 0, 0), yellow),
                new Plane(new Vector3(0, 0, 11), new Vector3(0, 0, -1), green),

                // new Sphere(new Vector3(0, 0, 10), 3f, red)
                new Triangle(new Vector3(-2, 3, 10), new Vector3(-1, 1, 10), new Vector3(1, 2, 10), red)
				//new Sphere(new Vector3(0, 5, 20), 1),
				//new Sphere(new Vector3(3, 3, 10), 4)
			};

            return scene;
        }
    }
}
