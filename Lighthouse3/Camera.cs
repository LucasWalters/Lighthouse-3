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
using System.Collections.Generic;
using System.Runtime.InteropServices;


namespace Lighthouse3
{
    public class Camera
    {
        public static bool timeFrames = true;
        public static int maxFrames = 0;

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

        public enum AdaptiveSamplingMethod { None, Contrast, SqrtKullbackLeibler, SqrtChiSquare, SqrtHellinger }

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
        public AdaptiveSamplingMethod adaptiveSampling;
        TracingThread[] threads;
        Thread[] ths;
        Scene scene;
        bool rendering = false;
        bool resetFrame = true;
        float distortion;
        public float vignettingFactor;
        float aspectRatio;
        bool stratification;
        bool blueNoise;
        int strata = 0;
        float invStrata;
        float invRaysPerPixel;
        float framesDivider = 1f;

        bool lastSamplesView = false;
        bool threadsStarted = false;

        float widthPerPixel;
        float heightPerPixel;
        Stopwatch stopwatch;

        private readonly object syncLock = new object();
        private readonly float adaptiveSamplingFactor = 0.5f;

        Barrier barrier;


        private Camera() { }
        public Camera(Scene scene, Vector3 position, Vector3 direction, int width, int height,
            float screenDistance = 1, int raysPerPixel = 1, ProjectionType projection = ProjectionType.Perspective,
            bool gammaCorrection = false, RayTracer rayTracer = RayTracer.Whitted, bool antiAliasing = false, float distortion = 0.2f,
            float vignettingFactor = 0f, bool stratification = false, bool blueNoise = false, AdaptiveSamplingMethod adaptiveSampling = AdaptiveSamplingMethod.None)
        {
            this.scene = scene;
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
            stopwatch = Stopwatch.StartNew();

            Console.WriteLine("Cores: " + Environment.ProcessorCount);
            numberOfThreads = Environment.ProcessorCount * 2;
            if (numberOfThreads > 0)
            {
                barrier = new Barrier(numberOfThreads, barrier => FinishFrame());
                threads = new TracingThread[numberOfThreads];
                ths = new Thread[numberOfThreads];
                for (int t = 0; t < numberOfThreads; t++)
                {
                    threads[t] = new TracingThread(t, this, barrier);
                    ths[t] = new Thread(new ThreadStart(threads[t].ThreadProc));
                    //ths[t].Priority = ThreadPriority.Highest;
                }
            }

            rays = new Ray[screenWidth * screenHeight * raysPerPixel];
            for (int i = 0; i < screenWidth * screenHeight * raysPerPixel; i++)
                rays[i] = new Ray();

            pixelColors = new int[screenWidth * screenHeight];
            UpdateCamera();
        }

        //Needs to be called everytime the camera's state changes
        public void UpdateCamera()
        {
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

            if (adaptiveSampling != AdaptiveSamplingMethod.None && !antiAliasing)
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

            if (adaptiveSampling != AdaptiveSamplingMethod.None)
                color /= pixel.samples;
            else
                color *= framesDivider + (debug ? 1 : 0);

            if (gammaCorrection)
                color = color.Sqrt();

            return Color.ToARGB(color * GetVignetteFactor(x, y));
        }

        public bool MaxFramesReached()
        {
            if (maxFrames > 0 && frames >= maxFrames)
            {
                if (samplesView != lastSamplesView)
                {
                    lastSamplesView = samplesView;
                    if (samplesView)
                        ShowPixelSampleDepth();
                    else
                    {
                        for (int x = 0; x < screenWidth; x++)
                        {
                            for (int y = 0; y < screenHeight; y++)
                            {
                                pixelColors[x + y * screenWidth] = GetFinalColor(x, y);
                            }
                        }
                    }
                    pixelsChanged = true;
                }
                return true;
            }
            return false;
        }

        public void RenderFrame()
        {


            if (resetFrame)
            {
                pixels = new Pixel[screenWidth * screenHeight];

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


            if (numberOfThreads < 2)
            {
                MainThreadFrame();
                FinishFrame();
            }
            else
            {
                MultithreadedFrame();
            }
        }

        public void ShowPixelSampleDepth()
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
                        float ratio = 2f * (float)(pixel.samples - min) / (float)(max - min);
                        float b = Calc.Max(0, (1f - ratio));
                        float r = Calc.Max(0, (ratio - 1f));
                        float g = 1f - b - r;
                        pixelColors[x + y * screenWidth] = Color.ToARGB(new Vector3(r, g, b));
                    }
                }
            }
        }

        public void FinishFrame()
        {

            framesDivider = 1f / ++frames;

            if (adaptiveSampling != AdaptiveSamplingMethod.None)
            {
                SetAdaptiveSamplingWeights();

                if (samplesView)
                    ShowPixelSampleDepth();
            }

            if (timeFrames)
            {
                stopwatch.Stop();
                Console.WriteLine("Frame finished after " + stopwatch.ElapsedMilliseconds + " ms");
                stopwatch.Restart();
            }

            TracingThread.workLeft = screenWidth * screenHeight;
            pixelsChanged = true;
        }

        public bool ShouldSuperSampleFDivergence(int x, int y)
        {
            float totalL = 0f;
            int totalSamples = 0;

            for (int dx = x - 1; dx <= x + 1; dx++)
            {
                if ((x == 0 && dx == x - 1) || (x == screenWidth - 1 && dx == x + 1))
                    continue;

                for (int dy = y - 1; dy <= y + 1; dy++)
                {
                    if ((y == 0 && dy == y - 1) || (y == screenHeight - 1 && dy == y + 1))
                        continue;

                    Pixel pixel = pixels[dx + dy * screenWidth];
                    Vector3 color = pixel.color / (float)pixel.samples;

                    totalL += Color.Luminance(color);
                    totalSamples++;
                }
            }

            if (totalL < Calc.Epsilon)
                return false;

            //From https://www.researchgate.net/publication/220853064_Refinement_Criteria_Based_on_f-Divergences

            float averageL = totalL / (float)totalSamples;
            float[] p = new float[totalSamples];
            float[] q = new float[totalSamples];
            int index = 0;
            float qVal = 1f / (float)totalSamples;


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

                    p[index] = Color.Luminance(color) / totalL;
                    q[index] = qVal;

                    index++;
                }
            }

            float distance = 0f;
            float threshold = 0f;

            switch (adaptiveSampling)
            {
                case AdaptiveSamplingMethod.SqrtKullbackLeibler:
                    distance = Calc.KLDistance(p, q);
                    threshold = 0.005f;
                    break;
                case AdaptiveSamplingMethod.SqrtChiSquare:
                    distance = Calc.Chi2Distance(p, q);
                    threshold = 0.007f;
                    break;
                case AdaptiveSamplingMethod.SqrtHellinger:
                    distance = Calc.HellingerDistance(p, q);
                    threshold = 0.003f;
                    break;
                default:
                    Console.WriteLine("Error! Adaptive sampling method not recognised!");
                    break;
            }

            float result = qVal * averageL * Calc.Sqrt(distance);
            return result > threshold;
        }

        public bool ShouldSuperSampleContrast(int x, int y)
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

            // From: https://dl-acm-org.proxy.library.uu.nl/doi/pdf/10.1145/37401.37410
            Vector3 thresholds = new Vector3(0.4f, 0.3f, 0.6f);

            for (int xyz = 0; xyz < 3; xyz++)
            {
                float c = (max[xyz] - min[xyz]) / (max[xyz] + min[xyz]);
                if (c > thresholds[xyz])
                {
                    return true;
                }
            }
            return false;
        }


        public void SetAdaptiveSamplingWeights()
        {
            adaptiveSamplingWeights = new int[screenWidth * screenHeight];

            for (int x = 0; x < screenWidth; x++)
            {
                for (int y = 0; y < screenHeight; y++)
                {
                    bool supersample;
                    if (adaptiveSampling == AdaptiveSamplingMethod.Contrast)
                        supersample = ShouldSuperSampleContrast(x, y);
                    else
                        supersample = ShouldSuperSampleFDivergence(x, y);


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

        public void MainThreadFrame()
        {
            for (int x = 0; x < screenWidth; x++)
            {
                for (int y = 0; y < screenHeight; y++)
                {
                    CalculateRay(x, y);
                    if (adaptiveSampling != AdaptiveSamplingMethod.None)
                        AdaptiveSampling(x, y);

                    pixelColors[x + y * screenWidth] = GetFinalColor(x, y);
                }
            }
        }

        public void MultithreadedFrame()
        {

            if (!threadsStarted)
            {
                threadsStarted = true;
                for (int t = 0; t < numberOfThreads; t++)
                {
                    ths[t].Start();
                }
            } 
        }

        public void AdaptiveSampling(int x, int y)
        {
            if (adaptiveSamplingWeights == null)
                return;

            int extraRays = Calc.Floor(adaptiveSamplingWeights[x + y * screenWidth] * adaptiveSamplingFactor);
            if (extraRays <= 0)
            {
                return;

            }

            for (int i = 0; i < extraRays; i++)
                CalculateRay(x, y);
        }

        public void CalculateRay(int x, int y, bool debug = false)
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
                    combinedColor += TraceRay(rays[x * raysPerPixel + y * screenWidth * raysPerPixel + i], debug);
                }
                if (adaptiveSampling != AdaptiveSamplingMethod.None)
                    pixels[index].samples += raysPerPixel;
                else
                    combinedColor *= invRaysPerPixel;

                pixels[index].color += combinedColor;
            }
            else
            {
                pixels[index].color += TraceRay(rays[index], debug);
                if (adaptiveSampling != AdaptiveSamplingMethod.None)
                    pixels[index].samples++;
            }
        }

        public void DebugRay(int x, int y)
        {
            if (x <= 0 || y <= 0)
                return;
            CalculateRay(x, y, true);
            pixelColors[x + y * screenWidth] = GetFinalColor(x, y, debug: true);
            pixelsChanged = true;
        }

        public Vector3 TraceRay(Ray ray, bool debug = false)
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

        public static int workLeft;
        private Barrier barrier;

        [DllImport("kernel32")]
        static extern int GetCurrentThreadId();

        public TracingThread(int threadIndex, Camera camera, Barrier barrier)
        {
            this.threadIndex = threadIndex;
            this.camera = camera;
            this.barrier = barrier;
            foreach (ProcessThread pt in Process.GetCurrentProcess().Threads)
            {
                int utid = GetCurrentThreadId();
                if (utid == pt.Id)
                {
                    long processor = 0x0000 + threadIndex / 2 + 1;
                    pt.ProcessorAffinity = (IntPtr)processor;
                    Console.WriteLine("Thread " + threadIndex + " set to processor " + processor);
                }
            }
        }

        public void ThreadProc()
        {
            Calc.Init();

            while (true)
            {
                int index = Interlocked.Decrement(ref workLeft);

                if (index < 0)
                {
                    barrier.SignalAndWait();
                    continue;
                }

                //Console.WriteLine("Thread " + threadIndex + " grabbing work " + index);

                int x = index % camera.screenWidth;
                int y = index / camera.screenWidth;

                camera.CalculateRay(x, y);
                if (camera.adaptiveSampling != Camera.AdaptiveSamplingMethod.None)
                    camera.AdaptiveSampling(x, y);

                camera.pixelColors[x + y * camera.screenWidth] = camera.GetFinalColor(x, y);
            }
        }
    }
}