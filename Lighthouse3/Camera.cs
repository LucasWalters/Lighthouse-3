using Lighthouse3.Primitives;
using OpenTK;
using System;
using System.Collections;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Lighthouse3.RayTracers;
using System.Threading;
using Lighthouse3.Scenes;
using System.Diagnostics;

namespace Lighthouse3
{
    public class Camera
    {
        public static bool timeFrames = true;

        //World position of the camera
        public Vector3 position;
        //View direction of the camera - should be normalized
        public Vector3 direction;
        public Vector3 up;
        public Vector3 left;
        //Projection type of camera, perspective is default and orthographic means rays are cast parallel to eachother
        public ProjectionType projection;
        //Distance to the screen (focal length)
        public float screenDistance;
        //Number of rays sent per pixel
        public int raysPerPixel;

        public RayTracer rayTracer;

        public bool gammaCorrection;

        //Diagonal  of the camera
        public float diagonalLength;
        //Screen pixels
        public int screenWidth;
        public int screenHeight;

        public bool antiAliasing;

        public bool samplesView = false;

        public enum ProjectionType { Perspective, Orthographic, Distortion }

        //Top left corner of screen
        Vector3 topLeft;
        //Top right corner of screen
        Vector3 topRight;
        //Bottom left corner of screen
        Vector3 bottomLeft;
        public Vector3 screenCenter;

        public Pixel[] pixels;
        public Vector3[] illumination;
        public int[] adaptiveSamplingWeights;
        public Ray[] rays;
        public int[] pixelColors;
        public bool pixelsChanged = false;
        public int numberOfThreads;
        public int frames = 0;
        public bool adaptiveSampleNextFrame = false;
        TracingThread[] threads;
        int threadsFinished = 0;
        bool rendering = false;
        bool resetFrame = true;
        float distortion;
        public float vignettingFactor;
        float aspectRatio;
        bool stratification;
        bool blueNoise;
        bool adaptiveSampling;
        int strata = 0;
        float invStrata;
        float invRaysPerPixel;
        float framesDivider;



        float widthPerPixel;
        float heightPerPixel;
        Stopwatch stopwatch;

        private readonly object syncLock = new object();
        private readonly float adaptiveSamplingFactor = 1f;

        private Camera() { }
        public Camera(Vector3 position, Vector3 direction, int width, int height,
            float screenDistance = 1, int raysPerPixel = 1, ProjectionType projection = ProjectionType.Perspective,
            bool gammaCorrection = false, RayTracer rayTracer = RayTracer.Whitted, bool antiAliasing = false, float distortion = 0.2f,
            float vignettingFactor = 0f, bool stratification = false, bool blueNoise = false, bool adaptiveSampling = false)
        {
            this.position = position;
            if (direction.LengthSquared != 0)
                direction = direction.Normalized();
            this.direction = direction;
            this.screenWidth = width;
            this.screenHeight = height;
            aspectRatio = screenWidth / screenHeight;
            this.screenDistance = screenDistance;
            this.projection = projection;
            this.raysPerPixel = raysPerPixel;
            this.gammaCorrection = gammaCorrection;
            this.rayTracer = rayTracer;
            this.antiAliasing = antiAliasing;
            this.distortion = distortion;
            this.vignettingFactor = vignettingFactor;
            this.stratification = stratification;
            this.blueNoise = blueNoise;
            this.adaptiveSampling = adaptiveSampling;

            up = Vector3.Cross(direction, Vector3.UnitX).Normalized();
            if (Math.Abs(direction.X) == 1f)
            {
                up = Vector3.UnitY;
            }
            // Make sure up always points up (unless direction is (0, (-)1, 0)
            if (direction.Z < 0)
                up = -up;


            numberOfThreads = Environment.ProcessorCount - 1;
            if (numberOfThreads > 0)
                threads = new TracingThread[numberOfThreads];

            rays = new Ray[screenWidth * screenHeight * raysPerPixel];
            for (int i = 0; i < screenWidth * screenHeight * raysPerPixel; i++)
                rays[i] = new Ray();

            pixelColors = new int[screenWidth * screenHeight];
            UpdateCamera();
        }

        //Needs to be called everytime the camera's state changes
        public void UpdateCamera()
        {
            if (threads != null)
                foreach (TracingThread thread in threads)
                    if (thread != null)
                        thread.abort = true;

            if (stratification)
            {
                double sqrt = Math.Sqrt(raysPerPixel);
                if (sqrt % 1 == 0)
                {
                    strata = (int)sqrt;
                    invStrata = 1f / strata;
                }
                else
                    Console.WriteLine("Invalid rays per pixel for stratification!");
            }
            else
            {
                strata = 0;
                invStrata = 0f;
            }

            if (adaptiveSampling && !antiAliasing)
                Console.WriteLine("Error! Adaptive Sampling set without Anti Aliasing!");

            invRaysPerPixel = 1f / raysPerPixel;

            widthPerPixel = 1f / (screenWidth - 1);
            heightPerPixel = 1f / (screenHeight - 1);

            left = Vector3.Cross(direction, up).Normalized() * ((float)screenWidth / screenHeight);

            screenCenter = position + direction * screenDistance;

            topLeft = screenCenter + up + left;
            topRight = screenCenter + up - left;
            bottomLeft = screenCenter - up + left;

            // Update Diagonal Length for FOV
            diagonalLength = (bottomLeft - topRight).Length;

            for (int x = 0; x < screenWidth; x++)
            {
                for (int y = 0; y < screenHeight; y++)
                {
                    UpdatePixelRays(x, y);
                }
            }
            resetFrame = true;
            rendering = false;
        }

        // Maps screen position to world position, u & v = [0, 1]
        public Vector3 GetPointPos(float u, float v)
        {
            if (projection == ProjectionType.Distortion)
            {
                Vector2 uv = DistortPointPos(u, v);
                u = uv.X;
                v = uv.Y;
            }

            return topLeft + u * (topRight - topLeft) + v * (bottomLeft - topLeft);
        }

        // Maps pixel position to world position, x = [0, screenWidth), y = [0, screenHeight)
        public Vector3 GetPixelPos(int x, int y, int index = 0)
        {
            if (stratification)
            {
                float u = (index % strata) * invStrata;
                float v = (index / strata) * invStrata;
                return GetPointPos(widthPerPixel * (x + u), heightPerPixel * (y + v));
            }

            return GetPointPos(widthPerPixel * x, heightPerPixel * y);
        }

        // Maps pixel position to world position with max 0.5 offset in x or y direction, x = [0, screenWidth), y = [0, screenHeight)
        public Vector3 GetRandomizedPixelPos(int x, int y, int index = 0)
        {
            float u, v;

            if (blueNoise && frames < 8)
            {
                Vector2 noise = Noise.Read(x, y, frames - 1);
                u = noise.X;
                v = noise.Y;
            }
            else
            {
                u = Calc.Random();
                v = Calc.Random();
            }
            if (stratification)
            {
                u = (index % strata) * invStrata + u * invStrata;
                v = (index / strata) * invStrata + v * invStrata;
            }
            return GetPointPos(widthPerPixel * (x + u), heightPerPixel * (y + v));
        }

        public void UpdatePixelRays(int x, int y)
        {
            if (raysPerPixel > 1)
                for (int i = 0; i < raysPerPixel; i++)
                    UpdateRay(x * raysPerPixel + y * screenWidth * raysPerPixel + i, GetPixelPos(x, y));
            else
                UpdateRay(x + y * screenWidth, GetPixelPos(x, y));
        }

        public void UpdateRandomizedPixelRays(int x, int y)
        {
            if (raysPerPixel > 1)
                for (int i = 0; i < raysPerPixel; i++)
                    UpdateRay(x * raysPerPixel + y * screenWidth * raysPerPixel + i, GetRandomizedPixelPos(x, y));
            else
                UpdateRay(x + y * screenWidth, GetRandomizedPixelPos(x, y));
        }

        private void UpdateRay(int index, Vector3 origin)
        {
            rays[index].origin = origin;
            rays[index].SetDirection((origin - position).Normalized());
        }

        public float GetVignetteFactor(int x, int y)
        {
            if (vignettingFactor == 0f)
                return 1f;
            float diff = (screenWidth - screenHeight) / 2;
            float divider = 1f / screenWidth;
            Vector2 point = new Vector2(x * divider, (y * aspectRatio + diff) * divider);
            float dist = (point - new Vector2(0.5f, 0.5f)).LengthSquared;
            return 1 - Calc.Clamp((vignettingFactor * dist), 0, 1);
        }

        public int GetFinalColor(int x, int y, bool debug = false)
        {
            Pixel pixel = pixels[x + y * screenWidth];

            Vector3 color = pixel.color;

            if (adaptiveSampling)
                color /= pixel.samples;
            else
                color *= framesDivider + (debug ? 1 : 0);

            if (gammaCorrection)
                color = color.Sqrt();

            return Color.ToARGB(color * GetVignetteFactor(x, y));
        }

        public void RenderFrame(Scene scene)
        {
            if (rendering)
                return;
            rendering = true;

            if (timeFrames)
                stopwatch = Stopwatch.StartNew();

            if (resetFrame)
            {
                pixels = new Pixel[screenWidth * screenHeight];
                adaptiveSampleNextFrame = false;

                for (int x = 0; x < screenWidth; x++)
                {
                    for (int y = 0; y < screenHeight; y++)
                    {
                        pixels[x + y * screenWidth] = new Pixel
                        {
                            x = x,
                            y = y
                        };
                    }
                }

                illumination = new Vector3[screenWidth * screenHeight];
                frames = 0;
                resetFrame = false;
            }

            framesDivider = 1f / ++frames;

            if (numberOfThreads < 2)
            {
                MainThreadFrame(scene);
                FinishFrame();
            }
            else
            {
                MultithreadedFrame(scene);
            }
        }

        public void FinishFrame()
        {
            if (adaptiveSampling)
            {
                if (adaptiveSampleNextFrame)
                    adaptiveSampleNextFrame = false;
                else
                    SetAdaptiveSamplingWeights();

                if (samplesView)
                {
                    int max = int.MinValue;
                    int min = int.MaxValue;
                    for (int x = 0; x < screenWidth; x++)
                    {
                        for (int y = 0; y < screenHeight; y++)
                        {
                            Pixel pixel = pixels[x + y * screenWidth];
                            if (pixel.samples < min)
                                min = pixel.samples;
                            if (pixel.samples > max)
                                max = pixel.samples;
                        }
                    }

                    if (min != max)
                    {
                        for (int x = 0; x < screenWidth; x++)
                        {
                            for (int y = 0; y < screenHeight; y++)
                            {
                                Pixel pixel = pixels[x + y * screenWidth];
                                float ratio = 2f * (float) (pixel.samples - min) / (float) (max - min);
                                float b = Calc.Max(0, (1f - ratio));
                                float r = Calc.Max(0, (ratio - 1f));
                                float g = 1f - b - r;
                                pixelColors[x + y * screenWidth] = Color.ToARGB(new Vector3(r, g, b));
                            }
                        }
                    }
                }
            }

            if (timeFrames)
            {
                stopwatch.Stop();
                Console.WriteLine("Frame finished after " + stopwatch.ElapsedMilliseconds + " ms");
            }

            pixelsChanged = true;
            rendering = false;
        }


        public void SetAdaptiveSamplingWeights()
        {
            adaptiveSamplingWeights = new int[screenWidth * screenHeight];

            for (int x = 0; x < screenWidth; x++)
            {
                for (int y = 0; y < screenHeight; y++)
                {
                    Vector3 max = Calc.Vector3MinValue();
                    Vector3 min = Calc.Vector3MaxValue();

                    for (int dx = x - 1; dx <= x + 1; dx++)
                    {
                        if ((x == 0 && dx == x - 1) || (x == screenWidth - 1 && dx == x + 1))
                            continue;

                        for (int dy = y - 1; dy <= y + 1; dy++)
                        {
                            if ((y == 0 && dy == y - 1) || (y == screenHeight - 1 && dy == y + 1))
                                continue;

                            Pixel pixel = pixels[dx + dy * screenWidth];
                            Vector3 color = pixel.color / pixel.samples;

                            for (int xyz = 0; xyz < 3; xyz++)
                            {
                                if (color[xyz] > max[xyz])
                                    max[xyz] = color[xyz];

                                if (color[xyz] < min[xyz])
                                    min[xyz] = color[xyz];
                            }

                        }
                    }

                    bool supersample = false;

                    // From: https://dl-acm-org.proxy.library.uu.nl/doi/pdf/10.1145/37401.37410
                    Vector3 thresholds = new Vector3(0.4f, 0.3f, 0.6f);

                    for (int xyz = 0; xyz < 3; xyz++)
                    {
                        float c = (max[xyz] - min[xyz]) / (max[xyz] + min[xyz]);
                        if (c > thresholds[xyz])
                        {
                            supersample = true;
                            adaptiveSampleNextFrame = true;
                            break;
                        }
                    }

                    if (supersample)
                    {
                        for (int dx = x - 1; dx <= x + 1; dx++)
                        {
                            if ((x == 0 && dx == x - 1) || (x == screenWidth - 1 && dx == x + 1))
                                continue;

                            for (int dy = y - 1; dy <= y + 1; dy++)
                            {
                                if ((y == 0 && dy == y - 1) || (y == screenHeight - 1 && dy == y + 1))
                                    continue;

                                adaptiveSamplingWeights[dx + dy * screenWidth]++;
                            }
                        }
                    }
                }
            }
        }

        public void MainThreadFrame(Scene scene)
        {
            for (int x = 0; x < screenWidth; x++)
            {
                for (int y = 0; y < screenHeight; y++)
                {
                    if (adaptiveSampleNextFrame)
                        AdaptiveSampling(x, y, scene);
                    else
                        CalculateRay(x, y, scene);

                    pixelColors[x + y * screenWidth] = GetFinalColor(x, y);
                }
            }
        }

        public void MultithreadedFrame(Scene scene)
        {
            threadsFinished = 0;

            for (int t = 0; t < numberOfThreads; t++)
            {
                threads[t] = new TracingThread(t, this, scene, (threadIndex) =>
                {
                    lock (syncLock)
                    {
                        if (++threadsFinished == numberOfThreads)
                        {
                            FinishFrame();
                        }
                    }
                });
                Thread th = new Thread(new ThreadStart(threads[t].ThreadProc));
                th.Start();
            }
        }

        public void AdaptiveSampling(int x, int y, Scene scene)
        {
            int extraRays = Calc.Floor(adaptiveSamplingWeights[x + y * screenWidth] * adaptiveSamplingFactor);
            if (extraRays <= 0)
            {
                return;

            }

            for (int i = 0; i < extraRays; i++)
                CalculateRay(x, y, scene);
        }

        public void CalculateRay(int x, int y, Scene scene, bool debug = false)
        {
            int index = x + y * screenWidth;

            if (antiAliasing)
            {
                UpdateRandomizedPixelRays(x, y);
            }

            if (raysPerPixel > 1)
            {
                Vector3 combinedColor = Vector3.Zero;
                for (int i = 0; i < raysPerPixel; i++)
                {
                    combinedColor += TraceRay(rays[x * raysPerPixel + y * screenWidth * raysPerPixel + i], scene, debug);
                }
                if (!adaptiveSampling)
                    combinedColor *= invRaysPerPixel;
                else
                    pixels[index].samples += raysPerPixel;

                pixels[index].color += combinedColor;
            }
            else
            {
                pixels[index].color += TraceRay(rays[index], scene, debug);
                if (adaptiveSampling)
                    pixels[index].samples++;
            }
        }

        public void DebugRay(Scene scene, int x, int y)
        {
            if (x <= 0 || y <= 0)
                return;
            CalculateRay(x, y, scene, true);
            pixelColors[x + y * screenWidth] = GetFinalColor(x, y, debug: true);
            pixelsChanged = true;
        }

        public Vector3 TraceRay(Ray ray, Scene scene, bool debug = false)
        {
            if (rayTracer == RayTracer.Whitted)
                return Whitted.TraceRay(ray, scene, debug: debug);
            if (rayTracer == RayTracer.Kajiya)
                return Kajiya.TraceRay(ray, scene, debug: debug);
            if (rayTracer == RayTracer.PathTracer)
                return PathTracer.TraceRay(ray, scene, debug: debug);
            return Color.Black;
        }


        public void RotateX(float angle)
        {
            angle = angle * Calc.Pi / 180f;
            Quaternion q = Quaternion.FromAxisAngle(-left, angle);
            direction = Vector3.Transform(direction, q);
            up = Vector3.Transform(up, q);
        }

        public void RotateY(float angle)
        {
            angle = angle * Calc.Pi / 180f;
            Quaternion q = Quaternion.FromAxisAngle(up, angle);
            direction = Vector3.Transform(direction, q);
            //up = Vector3.Transform(up, q);
        }

        public void RotateZ(float angle)
        {
            angle = angle * Calc.Pi / 180f;
            Quaternion q = Quaternion.FromAxisAngle(direction, angle);
            //direction = Vector3.Transform(direction, q);
            up = Vector3.Transform(up, q);
        }

        public void MoveX(float movement)
        {
            position = position + left * movement;
        }

        public void MoveY(float movement)
        {
            position = position + up * movement;
        }

        public void MoveZ(float movement)
        {
            position = position + direction * movement;
        }

        public Vector2 DistortPointPos(float u, float v)
        {
            u = u * 2 - 1;
            v = v * 2 - 1;


            float rSquared = (u * u) + (v * v);

            u = u * (1 + distortion * rSquared);
            v = v * (1 + distortion * rSquared);

            u = (u + 1) / 2;
            v = (v + 1) / 2;

            return new Vector2(u, v);
        }

    }

    public delegate void TacingThreadCallback(int threadIndex);

    public class TracingThread
    {
        private readonly int threadIndex;
        private readonly Camera camera;
        private readonly Scene scene;
        public bool abort = false;

        private TacingThreadCallback callback;

        public TracingThread(int threadIndex, Camera camera, Scene scene, TacingThreadCallback callback)
        {
            this.threadIndex = threadIndex;
            this.camera = camera;
            this.scene = scene;
            this.callback = callback;
        }

        public void ThreadProc()
        {
            //Initializes random seed in calc
            Calc.Init();
            for (int x = threadIndex; x < camera.screenWidth; x += camera.numberOfThreads)
            {
                for (int y = 0; y < camera.screenHeight; y++)
                {
                    if (abort)
                        return;
                    if (camera.adaptiveSampleNextFrame)
                        camera.AdaptiveSampling(x, y, scene);
                    else
                        camera.CalculateRay(x, y, scene);
                    camera.pixelColors[x + y * camera.screenWidth] = camera.GetFinalColor(x, y);
                }
            }


            if (!abort)
                callback?.Invoke(threadIndex);
        }
    }
}