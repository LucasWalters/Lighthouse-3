using System;
using System.Diagnostics;
using Lighthouse3.Lights;
using Lighthouse3.Primitives;
using OpenTK;
using OpenTK.Graphics;

namespace Lighthouse3.Scenes
{
    public class StandardScenes
    {
        public static Scene CURRENT_SCENE = CornellBoxScene();

        // Scene with an "impossible" triangle tried to put into a persepctive where it actually looks impossible
        public static Scene SimpleOBJScene()
        {
            Scene scene = new Scene();
            RayTracers.RayTracer rayTracer = RayTracers.RayTracer.Whitted;

            scene.mainCamera =
                new Camera(
                    scene,
                    position: new Vector3(-100, -100, -100),
                    direction: new Vector3(1, 1, 1),
                    Game.SCREEN_WIDTH, Game.SCREEN_HEIGHT,
                    projection: Camera.ProjectionType.Perspective,
                    screenDistance: 15f,
                    raysPerPixel: 1,
                    rayTracer: rayTracer,
                    antiAliasing: true,
                    vignettingFactor: 2f,
                    gammaCorrection: false,
                    distortion: 0f, // Only if projection is set to Distortion
                    stratification: true,
                    blueNoise: false,
                    adaptiveSampling: Camera.AdaptiveSamplingMethod.None
                );
            scene.backgroundColor = Color.Black;

            if (rayTracer == RayTracers.RayTracer.Whitted)
                scene.lights = new Light[] { new PointLight(new Vector3(-8, 5, -9), Color.White, 300f) };
            else
                scene.lights = new Light[] { new AreaLight(new Vector3(20, 10, 20), new Vector3(-20, 10, 20), new Vector3(20, 10, -20), Color.White, 0.2f) };

            scene.primitives = ObjectLoader.GetObjTriangles("../../assets/impossible_triangle.obj");
            scene.CalculateBVH(stats: false, collapsed4Way: false);
            return scene;
        }


        public static Scene TeapotScene()
        {
            Scene scene = new Scene();
            RayTracers.RayTracer rayTracer = RayTracers.RayTracer.PathTracer;

            scene.mainCamera =
                new Camera(
                    scene,
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
                    stratification: true,
                    blueNoise: false,
                    adaptiveSampling: Camera.AdaptiveSamplingMethod.None
                );
            scene.backgroundColor = Color.Black;

            if (rayTracer == RayTracers.RayTracer.Whitted)
                scene.lights = new Light[] { new PointLight(new Vector3(0, 3, -3), Color.White, 50) };
            else
                scene.lights = new Light[] { new AreaLight(new Vector3(10, 6, 0), new Vector3(-10, 6, 0), new Vector3(10, 6, -10), Color.White, 1f) };

            scene.primitives = ObjectLoader.GetObjTriangles("../../assets/teapot.obj");

            scene.CalculateBVH(stats: true, collapsed4Way: true);
            return scene;
        }
        public static Scene KnightScene()
        {
            Scene scene = new Scene();
            RayTracers.RayTracer rayTracer = RayTracers.RayTracer.Whitted;

            scene.mainCamera =
                new Camera(
                    scene,
                    position: new Vector3(0, 50, 10),
                    direction: new Vector3(0, -1, 0.005f).Normalized(),
                    Game.SCREEN_WIDTH, Game.SCREEN_HEIGHT,
                    projection: Camera.ProjectionType.Perspective,
                    screenDistance: 1.5f,
                    raysPerPixel: 1,
                    rayTracer: rayTracer,
                    antiAliasing: false,
                    vignettingFactor: 2f,
                    gammaCorrection: false,
                    distortion: 0f, // Only if projection is set to Distortion
                    stratification: true,
                    blueNoise: false,
                    adaptiveSampling: Camera.AdaptiveSamplingMethod.None
                );
            scene.backgroundColor = Color.Black;

            if (rayTracer == RayTracers.RayTracer.Whitted)
                scene.lights = new Light[] { new PointLight(new Vector3(0, 50, 0), Color.White, 5000) };
            else
                scene.lights = new Light[] { new AreaLight(new Vector3(20, 15, 20), new Vector3(0, 15, 20), new Vector3(20, 15, 0), Color.White, 1f) };

            scene.primitives = ObjectLoader.GetObjTriangles("../../assets/knight/180212_Erik_XIV_Rustning_2.obj");

            scene.CalculateBVH(stats: true, collapsed4Way: true);
            return scene;
        }

        public static Scene KajiyaScene()
        {
            Scene scene = new Scene();
            scene.mainCamera =
                new Camera(
                    scene,
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
                    stratification: false,
                    blueNoise: true,
                    adaptiveSampling: Camera.AdaptiveSamplingMethod.None
                );
            scene.backgroundColor = Color.Black;
            scene.lights = new Light[]
            {
                //new PointLight(new Vector3(0, 10, -5), Color.White, 500f),
                new AreaLight(new Vector3(10,6,10), new Vector3(-10,6,10), new Vector3(10,6,-10),  Color.White, 2f),
            };
            Material checkerboard = new Material(Color4.Gray);
            checkerboard.checkerboard = 1f;
            scene.planes = new Plane[]
            {
                new Plane(new Vector3(-10, 0, 0), new Vector3(1, 0, 0), Material.BlueViolet),
                new Plane(new Vector3(0, -2, 0), new Vector3(0, 1, 0), Material.Gray),
                new Plane(new Vector3(10, 0, 0), new Vector3(-1, 0, 0), Material.MediumVioletRed),
                new Plane(new Vector3(0, 0, -10), new Vector3(0, 0, 1), Material.DarkOliveGreen),
                new Plane(new Vector3(0, 0, 10), new Vector3(0, 0, -1), checkerboard),
            };
            scene.primitives = new Primitive[]
            {
                new Sphere(new Vector3(-2, 0, 3), 2f, Material.Glass),
                new Sphere(new Vector3(2, 0, 3), 2f, Material.Blue),
                new Sphere(new Vector3(-6, 0, 3), 2f, Material.Mirror),
                new Sphere(new Vector3(6, 0, 3), 2f, Material.GlossyMirror),
            };
            return scene;
        }

        public static Scene WhittedScene()
        {
            Scene scene = new Scene(); 
            scene.mainCamera =
                 new Camera(
                    scene,
                     position: new Vector3(0, 0.5f, -3),
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
                    stratification: true,
                    blueNoise: false,
                    adaptiveSampling: Camera.AdaptiveSamplingMethod.None
                 );
            scene.backgroundColor = Color.Black;
            scene.lights = new Light[]
            {
                new PointLight(new Vector3(0, 5, -3), Color.White, 50)
                
                //new AreaLight(new Vector3(3,5,3), new Vector3(-3,5,3), new Vector3(3,3,-3),  Color.White, 0.2f)
            };
            Material checkerboard = new Material(Color4.Gray);
            checkerboard.checkerboard = 1f;
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
                new Sphere(new Vector3(-2.5f, 0, 0f), 1f, Material.Yellow),
                new Sphere(new Vector3(2.5f, 0, 0.5f), 1f, Material.Mirror),
                new Sphere(new Vector3(0, 0f, 0.5f), 1f, Material.Glass),
            };

            return scene;

        }

        public static Scene CornellBoxScene()
        {
            Scene scene = new Scene();
            scene.mainCamera =
                new Camera(
                    scene,
                    position: new Vector3(278, 273, -800),
                    direction: new Vector3(0, 0, 1),
                    Game.SCREEN_WIDTH, Game.SCREEN_HEIGHT,
                    projection: Camera.ProjectionType.Perspective,
                    screenDistance: 3f,
                    raysPerPixel: 1,
                    rayTracer: RayTracers.RayTracer.PathTracer,
                    antiAliasing: true,
                    vignettingFactor: 0f,
                    gammaCorrection: false,
                    distortion: 0f, // Only if projection is set to Distortion
                    stratification: false,
                    blueNoise: false,
                    adaptiveSampling: Camera.AdaptiveSamplingMethod.None
                );
            scene.backgroundColor = Color.Black;

            float value = 180f / 255f;
            Material red = new Material(value, 0.05f, 0.05f);
            Material green = new Material(0.05f, value, 0.05f);
            Material white = new Material(value, value, value);
            Vector3 lightColor = new Vector3(1, 1, 1);// new Vector3(0.95f, 0.8f, 0.6f);

            scene.lights = new Light[]
            {
                new AreaLight(new Vector3(343f, 548.8f, 332f), new Vector3(213.0f, 548.8f, 332f), new Vector3(343f, 548.8f, 227f), lightColor, 60f),
                //new AreaLight(new Vector3(556, 548.8f, 559.2f), new Vector3(0, 548.8f, 559.2f), new Vector3(556, 548.8f, 0), Color.White, 1f),


                new AreaLight(new Vector3(328, 323, -810), new Vector3(228, 323, -810), new Vector3(328, 223, -810), lightColor, 60f),
            };
            Material checkerboard = new Material(value, value, value);
            checkerboard.checkerboard = 70f;


            // From Cornell box data http://www.graphics.cornell.edu/online/box/data.html, vector order: 2, 3, 1
            scene.primitives = new Primitive[]
            {
                new Rectangle(new Vector3(0, 0, 0), new Vector3(0, 0, 559.2f), new Vector3(556f, 0, 0), white), // Floor

                new Rectangle(new Vector3(556f, 548.8f, 559.2f), new Vector3(343, 548.8f, 559.2f), new Vector3(556, 548.8f, 0), white), // Ceiling
                new Rectangle(new Vector3(343, 548.8f, 559.2f), new Vector3(213, 548.8f, 559.2f), new Vector3(343, 548.8f, 332), white), // Ceiling
                new Rectangle(new Vector3(343, 548.8f, 227), new Vector3(213, 548.8f, 227), new Vector3(343, 548.8f, 0), white), // Ceiling
                new Rectangle(new Vector3(213, 548.8f, 559.2f), new Vector3(0, 548.8f, 559.2f), new Vector3(213, 548.8f, 0), white), // Ceiling

                new Rectangle(new Vector3(0, 0, 559.2f), new Vector3(0, 548.8f, 559.2f), new Vector3(556f, 0, 559.2f), checkerboard), //Back wall
                new Rectangle(new Vector3(0, 0, 0), new Vector3(0, 548.8f, 0), new Vector3(0, 0, 559.2f), green), // Right wall
                new Rectangle(new Vector3(556f, 0, 559.2f), new Vector3(556f, 548.8f, 559.2f), new Vector3(556f, 0, 0), red), // Left wall

                new Rectangle(new Vector3(82, 165, 225), new Vector3(240, 165, 272), new Vector3(130, 165, 65), white), // Box
                new Rectangle(new Vector3(288, 165, 112), new Vector3(240, 165, 272), new Vector3(288, 0, 112), white), // Box
                new Rectangle(new Vector3(130, 165, 65), new Vector3(288, 165, 112), new Vector3(130, 0, 65), white), // Box
                new Rectangle(new Vector3(82, 165, 225), new Vector3(130, 165, 65), new Vector3(82, 0, 225), white), // Box
                new Rectangle(new Vector3(240, 165, 272), new Vector3(82, 165, 225), new Vector3(240, 0, 272), white), // Box

                
                new Sphere(new Vector3(400f, 150f, 250f), 100f, Material.Glass),
                new Sphere(new Vector3(185, 225, 168.5f), 60f, Material.Mirror),
                //new Sphere(new Vector3(2, 0, 3), 2f, Material.Blue),
                //new Sphere(new Vector3(-6, 0, 3), 2f, Material.Mirror),
                //new Sphere(new Vector3(6, 0, 3), 2f, Material.GlossyMirror),
            };
            return scene;
        }

    }
}
