﻿using Lighthouse3.Lights;
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

        // Doesn't work because of position
        public static Scene BasicScene()
        {
            Scene scene = new Scene();
            scene.mainCamera = new Camera(new Vector3(-50, 75, -10), new Vector3(1, 0, 0).Normalized(), Game.SCREEN_WIDTH, Game.SCREEN_HEIGHT, 1f);
            scene.backgroundColor = Color.Black;
            scene.lights = new Light[]
            {
                //new PointLight(new Vector3(20, 75, -10), Color.White, 1)
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
            scene.mainCamera = new Camera(new Vector3(0, 0, -5), new Vector3(0, 0, 1), Game.SCREEN_WIDTH, Game.SCREEN_HEIGHT, 1.5f, 1, rayTracer: RayTracers.RayTracer.Kajiya, antiAliasing: false, vignettingFactor: 2f);
            scene.backgroundColor = Color.Black;
            scene.lights = new Light[]
            {
                //new PointLight(new Vector3(0, 10, -5), Color.White, 500f),
                new AreaLight(new Vector3(20,10,20), new Vector3(-20,10,20), new Vector3(20,10,-20),  Color.White, 0.2f),
            };
            Material checkerboard = new Material(Color4.Gray);
            checkerboard.isCheckerboard = true;

            scene.primitives = new Primitive[]
            {
                new Plane(new Vector3(-20, 0, 0), new Vector3(1, 0, 0), Material.BlueViolet),
                new Plane(new Vector3(0, -2, 0), new Vector3(0, 1, 0), checkerboard),
                new Plane(new Vector3(20, 0, 0), new Vector3(-1, 0, 0), Material.MediumVioletRed),
                new Plane(new Vector3(0, 0, -10), new Vector3(0, 0, 1), Material.DarkOliveGreen),
                new Plane(new Vector3(0, 0, 20), new Vector3(0, 0, -1), Material.YellowGreen),

                new Sphere(new Vector3(-4, 0, 6), 2f, Material.Glass),
                new Sphere(new Vector3(4, 0, 6), 2f, Material.Blue),
                new Sphere(new Vector3(-8, 0, 4), 2f, Material.Mirror),
                new Sphere(new Vector3(8, 0, 4), 2f, Material.GlossyMirror),
            };
            // Uncomment to add a box in front of the camera
            //scene.primitives = scene.primitives.Concat(ObjectLoader.GetObjTriangles("../../assets/square.obj")).ToArray();
            return scene;
        }

        public static Scene SphereScene()
        {
            Scene scene = new Scene();
            scene.mainCamera = new Camera(new Vector3(0, 0, -3), new Vector3(0, 0, 1), Game.SCREEN_WIDTH, Game.SCREEN_HEIGHT, 1.5f, 1, rayTracer: RayTracers.RayTracer.Kajiya, antiAliasing: true);
            scene.backgroundColor = Color.Black;
            scene.lights = new Light[]
            {
                //new PointLight(new Vector3(0, 3, -3), Color.White, 50)
                
                new AreaLight(new Vector3(3,5,3), new Vector3(-3,5,3), new Vector3(3,3,-3),  Color.White, 0.2f)
            };
            Material checkerboard = new Material(Color4.Gray);
            checkerboard.isCheckerboard = true;

            scene.primitives = new Primitive[]
            { 
				new Plane(new Vector3(-5, 0, 0), new Vector3(1, 0, 0), Material.BlueViolet),
                new Plane(new Vector3(0, 0, 5), new Vector3(0, 0, -1), checkerboard),
                new Plane(new Vector3(5, 0, 0), new Vector3(-1, 0, 0), Material.MediumVioletRed),
                new Plane(new Vector3(0, -1.1f, 0), new Vector3(0, 1, 0), Material.DarkOliveGreen),
                new Plane(new Vector3(0, 10, 0), new Vector3(0, -1, 0), Material.YellowGreen),


                new Sphere(new Vector3(2, 0, 3), 1f, Material.Yellow),
                new Sphere(new Vector3(3, 0, 1), 1f, Material.GlossyMirror),
                new Sphere(new Vector3(-1, 0, 2), 1f, Material.Glass),
			};

            // Uncomment to add a box in front of the camera
            //scene.primitives = scene.primitives.Concat(ObjectLoader.GetObjTriangles("../../assets/square.obj")).ToArray();
            return scene;

        }
    }
}
