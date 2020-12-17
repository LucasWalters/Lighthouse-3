using System;
using Lighthouse3.Lights;
using Lighthouse3.Primitives;
using OpenTK;
using OpenTK.Graphics;

namespace Lighthouse3.Scenes
{
    public class StandardScenes
    {
        // Scene with an "impossible" triangle tried to put into a persepctive where it actually looks impossible
        public static Scene OBJScene()
        {
            Scene scene = new Scene();
            RayTracers.RayTracer rayTracer = RayTracers.RayTracer.Whitted;

            scene.mainCamera =
                new Camera(
                    position: new Vector3(-100, -100, -100),
                    direction: new Vector3(1, 1, 1),
                    Game.SCREEN_WIDTH, Game.SCREEN_HEIGHT,
                    projection: Camera.ProjectionType.Perspective,
                    screenDistance: 15f,
                    raysPerPixel: 1,
                    rayTracer: rayTracer,
                    antiAliasing: false,
                    vignettingFactor: 2f,
                    gammaCorrection: false,
                    distortion: 0f, // Only if projection is set to Distortion
                    stratification: true
                );
            scene.backgroundColor = Color.Black;

            if (rayTracer == RayTracers.RayTracer.Whitted)
                scene.lights = new Light[] { new PointLight(new Vector3(-8, 5, -9), Color.White, 300f) };
            else
                scene.lights = new Light[] { new AreaLight(new Vector3(20, 10, 20), new Vector3(-20, 10, 20), new Vector3(20, 10, -20), Color.White, 0.2f) };

            scene.primitives = ObjectLoader.GetObjTriangles("../../assets/impossible_triangle.obj");
            scene.CalculateBVH();
            return scene;
        }


        public static Scene TeapotScene()
        {
            Scene scene = new Scene();
            RayTracers.RayTracer rayTracer = RayTracers.RayTracer.PathTracer;

            scene.mainCamera =
                new Camera(
                    position: new Vector3(0, 1, -5),
                    direction: new Vector3(0, 0, 1),
                    Game.SCREEN_WIDTH, Game.SCREEN_HEIGHT,
                    projection: Camera.ProjectionType.Perspective,
                    screenDistance: 1.5f,
                    raysPerPixel: 1,
                    rayTracer: rayTracer,
                    antiAliasing: false,
                    vignettingFactor: 2f,
                    gammaCorrection: false,
                    distortion: 0f, // Only if projection is set to Distortion
                    stratification: true
                );
            scene.backgroundColor = Color.Black;

            if (rayTracer == RayTracers.RayTracer.Whitted)
                scene.lights = new Light[] { new PointLight(new Vector3(0, 3, -3), Color.White, 50) };
            else
                scene.lights = new Light[] { new AreaLight(new Vector3(10, 6, 0), new Vector3(-10, 6, 0), new Vector3(10, 6, -10), Color.White, 1f) };

            scene.primitives = ObjectLoader.GetObjTriangles("../../assets/teapot.obj");
            scene.CalculateBVH();
            return scene;
        }

        public static Scene KajiyaScene()
        {
            Scene scene = new Scene();
            scene.mainCamera =
                new Camera(
                    position: new Vector3(0, 0, -5),
                    direction: new Vector3(0, 0, 1),
                    Game.SCREEN_WIDTH, Game.SCREEN_HEIGHT,
                    projection: Camera.ProjectionType.Perspective,
                    screenDistance: 1.5f,
                    raysPerPixel: 1,
                    rayTracer: RayTracers.RayTracer.PathTracer,
                    antiAliasing: true,
                    vignettingFactor: 2f,
                    gammaCorrection: false,
                    distortion: 0f, // Only if projection is set to Distortion
                    stratification: false
                );
            scene.backgroundColor = Color.Black;
            scene.lights = new Light[]
            {
                //new PointLight(new Vector3(0, 10, -5), Color.White, 500f),
                new AreaLight(new Vector3(10,6,10), new Vector3(-10,6,10), new Vector3(10,6,-10),  Color.White, 0.5f),
            };
            Material checkerboard = new Material(Color4.Gray);
            checkerboard.isCheckerboard = true;
            scene.planes = new Plane[]
            {
                new Plane(new Vector3(-20, 0, 0), new Vector3(1, 0, 0), Material.BlueViolet),
                new Plane(new Vector3(0, -2, 0), new Vector3(0, 1, 0), Material.Gray),
                new Plane(new Vector3(20, 0, 0), new Vector3(-1, 0, 0), Material.MediumVioletRed),
                new Plane(new Vector3(0, 0, -10), new Vector3(0, 0, 1), Material.DarkOliveGreen),
                new Plane(new Vector3(0, 0, 10), new Vector3(0, 0, -1), Material.YellowGreen),
            };
            scene.primitives = new Primitive[]
            {
                new Sphere(new Vector3(-4, 0, 6), 2f, Material.Glass),
                new Sphere(new Vector3(4, 0, 6), 2f, Material.Blue),
                new Sphere(new Vector3(-8, 0, 4), 2f, Material.Mirror),
                new Sphere(new Vector3(8, 0, 4), 2f, Material.GlossyMirror),
            };
            return scene;
        }

        public static Scene WhittedScene()
        {
            Scene scene = new Scene(); scene.mainCamera =
                 new Camera(
                     position: new Vector3(0, 0, -3),
                     direction: new Vector3(0, 0, 1),
                     Game.SCREEN_WIDTH, Game.SCREEN_HEIGHT,
                     projection: Camera.ProjectionType.Perspective,
                     screenDistance: 1.5f,
                     raysPerPixel: 1,
                     rayTracer: RayTracers.RayTracer.Whitted,
                     antiAliasing: true,
                     vignettingFactor: 2f,
                     gammaCorrection: false,
                    distortion: 0f, // Only if projection is set to Distortion
                    stratification: true
                 );
            scene.backgroundColor = Color.Black;
            scene.lights = new Light[]
            {
                new PointLight(new Vector3(0, 3, -3), Color.White, 50)
                
                //new AreaLight(new Vector3(3,5,3), new Vector3(-3,5,3), new Vector3(3,3,-3),  Color.White, 0.2f)
            };
            Material checkerboard = new Material(Color4.Gray);
            checkerboard.isCheckerboard = true;
            scene.planes = new Plane[]
            {
                new Plane(new Vector3(-5, 0, 0), new Vector3(1, 0, 0), Material.BlueViolet),
                new Plane(new Vector3(0, 0, 5), new Vector3(0, 0, -1), checkerboard),
                new Plane(new Vector3(5, 0, 0), new Vector3(-1, 0, 0), Material.MediumVioletRed),
                new Plane(new Vector3(0, -1f, 0), new Vector3(0, 1, 0), checkerboard),
                new Plane(new Vector3(0, 10, 0), new Vector3(0, -1, 0), Material.YellowGreen),
            };

            scene.primitives = new Primitive[]
            {
                new Sphere(new Vector3(2, 0, 3), 1f, Material.Yellow),
                new Sphere(new Vector3(3, 0, 1), 1f, Material.Mirror),
                new Sphere(new Vector3(-1, 0, 2), 1f, Material.Glass),
            };

            return scene;

        }
    }
}
