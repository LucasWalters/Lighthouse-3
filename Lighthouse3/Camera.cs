﻿using Lighthouse3.Primitives;
using OpenTK;
using System;
using System.Collections;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Lighthouse3.RayTracers;
using System.Threading;

namespace Lighthouse3
{
    public class Camera
    {
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

        public enum ProjectionType { Perspective, Orthographic }

        //Top left corner of screen
        Vector3 topLeft;
        //Top right corner of screen
        Vector3 topRight;
        //Bottom left corner of screen
        Vector3 bottomLeft;
        Vector3 screenCenter;

        public Vector3[] pixelColors;
        public int[] pixels;
        public bool pixelsChanged = false;
        TracingThread[] threads;
        int numberOfThreads;
        int frames = 0;
        int threadsFinished = 0;
        bool rendering = false;
        bool resetFrame = true;

        private Camera() { }
        public Camera(Vector3 position, Vector3 direction, int width, int height,
            float screenDistance = 1, int raysPerPixel = 1, ProjectionType projection = ProjectionType.Perspective,
            bool gammaCorrection = false, RayTracer rayTracer = RayTracer.Whitted, bool antiAliasing = false)
        {
            this.position = position;
            if (direction.LengthSquared != 0)
                direction = direction.Normalized();
            this.direction = direction;
            this.screenWidth = width;
            this.screenHeight = height;
            this.screenDistance = screenDistance;
            this.projection = projection;
            this.raysPerPixel = raysPerPixel;
            this.gammaCorrection = gammaCorrection;
            this.rayTracer = rayTracer;
            this.antiAliasing = antiAliasing;

            up = Vector3.Cross(direction, Vector3.UnitX).Normalized();
            //Console.WriteLine(up);
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


            pixels = new int[screenWidth * screenHeight];
            UpdateCamera();
        }

        //Needs to be called everytime the camera's state changes
        public void UpdateCamera()
        {
            //foreach (TracingThread thread in threads)
            //{
            //    if (thread != null)
            //        thread.abort = true;
            //}
            //pixelColors = new Vector3[screenWidth * screenHeight];
            //pixels = new int[screenWidth * screenHeight];
            //pixelsChanged = false;
            //frames = 0;

            left = Vector3.Cross(direction, up).Normalized() * ((float)screenWidth / screenHeight);
            //Console.WriteLine(left);
            
            screenCenter = position + direction * screenDistance;

            topLeft = screenCenter + up + left;
            topRight = screenCenter + up - left;
            bottomLeft = screenCenter - up + left;

            // Update Diagonal Length for FOV
            diagonalLength = (bottomLeft - topRight).Length;
            resetFrame = true;
        }

        // Maps screen position to world position, u & v = [0, 1]
        public Vector3 GetPointPos(float u, float v)
        {
            return topLeft + u * (topRight - topLeft) + v * (bottomLeft - topLeft);
        }

        // Maps pixel position to world position, x = [0, screenWidth), y = [0, screenHeight)
        public Vector3 GetPixelPos(int x, int y)
        {
            float u = (1f / (screenWidth - 1)) * x;
            float v = (1f / (screenHeight - 1)) * y;
            return GetPointPos(u, v);
        }

        // Maps pixel position to world position with max 0.5 offset in x or y direction, x = [0, screenWidth), y = [0, screenHeight)
        public Vector3 GetRandomizedPixelPos(int x, int y)
        {
            float u = (1f / (screenWidth - 1)) * (x - 0.5f + Calc.Random());
            float v = (1f / (screenHeight - 1)) * (y - 0.5f + Calc.Random());
            return GetPointPos(u, v);
        }

        public Ray GetPointRay(float u, float v)
        {
            Vector3 point = GetPointPos(u, v);
            Vector3 rayDir = (point - position).Normalized();
            return new Ray(position, rayDir);
        }

        public Ray GetPixelRay(int x, int y)
        {
            Vector3 point = GetPixelPos(x, y);
            Vector3 rayDir = (point - position).Normalized();
            return new Ray(position, rayDir);
        }

        public Ray[] GetPixelRays(int x, int y, int rayCount)
        {
            Ray[] rays = new Ray[rayCount];
            for (int i = 0; i < rayCount; i++)
            {
                rays[i] = GetPixelRay(x, y);
            }
            return rays;
        }

        public Ray GetRandomizedPixelRay(int x, int y)
        {
            Vector3 point = GetRandomizedPixelPos(x, y);
            Vector3 rayDir = (point - position).Normalized();
            return new Ray(position, rayDir);
        }

        public Ray[] GetRandomizedPixelRays(int x, int y, int rayCount)
        {
            Ray[] rays = new Ray[rayCount];
            for (int i = 0; i < rayCount; i++)
            {
                rays[i] = GetRandomizedPixelRay(x, y);
            }
            return rays;
        }

        public int[] Frame(Scene scene)
        {
            float raysDivider = 1f / raysPerPixel;
            float framesDivider = 1f / ++frames;
            for (int x = 0; x < screenWidth; x++)
            {
                for (int y = 0; y < screenHeight; y++)
                {
                    if (raysPerPixel > 1)
                    { 
                        Ray[] pixelRays = antiAliasing ? GetRandomizedPixelRays(x, y, raysPerPixel) : GetPixelRays(x, y, raysPerPixel);
                        Vector3 combinedColour = Vector3.Zero;
                        for (int i = 0; i < raysPerPixel; i++)
                        {
                            combinedColour += TraceRay(pixelRays[i], scene);
                        }
                        pixelColors[x + y * screenWidth] += combinedColour * raysDivider;
                    }
                    else
                    {
                        pixelColors[x + y * screenWidth] += TraceRay(antiAliasing ? GetRandomizedPixelRay(x, y) : GetPixelRay(x, y), scene);
                    }
                    pixels[x + y * screenWidth] = ColorToPixel(pixelColors[x + y * screenWidth] * framesDivider);
                }
            }
            pixelsChanged = true;
            return pixels;
        }
        public void MultithreadedFrame(Scene scene)
        {
            if (numberOfThreads < 2)
            {
                Frame(scene);
                return;
            }
            if (rendering)
                return;
            rendering = true;
            if (resetFrame)
            {
                pixelColors = new Vector3[screenWidth * screenHeight];
                frames = 0;
                resetFrame = false;
            }
            threadsFinished = 0;

            float framesDivider = 1f / ++frames;
            int perThread = (int)Math.Floor((float)screenWidth / (float)numberOfThreads);
            for (int t = 0; t < numberOfThreads; t++)
            {
                int start = t * perThread;
                int end = (t < numberOfThreads - 1) ? start + perThread : screenWidth;
                int current = t;
                threads[t] = new TracingThread(this, scene, start, end, (Vector3[] output) =>
                {
                    for (int x = start; x < end; x++)
                    {
                        for (int y = 0; y < screenHeight; y++)
                        {
                            int index = x + y * screenWidth;
                            pixelColors[index] += output[(x - start) + y * (end - start)];
                            pixels[index] = ColorToPixel(pixelColors[index] * framesDivider);
                        }
                    }
                    if (++threadsFinished == numberOfThreads)
                    {
                        pixelsChanged = true;
                        rendering = false;
                    }
                });
                Thread th = new Thread(new ThreadStart(threads[t].ThreadProc));
                th.Start();
            }
        }

        public void DebugRay(Scene scene, int x, int y)
        {
            Vector3 color = pixelColors[x + y * screenWidth];
            if (raysPerPixel > 1)
            {
                Ray[] pixelRays = GetRandomizedPixelRays(x, y, raysPerPixel);
                Vector3 combinedColour = Vector3.Zero;
                for (int i = 0; i < raysPerPixel; i++)
                {
                    combinedColour += TraceRay(pixelRays[i], scene, true);
                }
                float divider = 1f / raysPerPixel;
                color += combinedColour* divider;
            }
            else
            {
                color += TraceRay(GetPixelRay(x, y), scene, true);
            }
            color /= frames + 1;
            pixels[x + y * screenWidth] = ColorToPixel(color);
            pixelsChanged = true;
        }

        public Vector3 TraceRay(Ray ray, Scene scene, bool debug = false)
        {
            if (rayTracer == RayTracer.Whitted)
                return Whitted.TraceRay(ray, scene, debug: debug);
            if (rayTracer == RayTracer.Kajiya)
                return Kajiya.TraceRay(ray, scene, debug: debug);
            return Color.Black;
        }

        private int ColorToPixel(Vector3 color)
        {
            if (gammaCorrection)
                color = color.Sqrt();
            return Color.ToARGB(color);
        }

        public void RotateX(float angle)
        {
            angle = angle * (float)Math.PI / 180;
            Quaternion q = Quaternion.FromAxisAngle(-left, angle);
            direction = Vector3.Transform(direction, q);
            up = Vector3.Transform(up, q);
        }

        public void RotateY(float angle)
        {
            angle = angle * (float)Math.PI / 180;
            Quaternion q = Quaternion.FromAxisAngle(up, angle);
            direction = Vector3.Transform(direction, q);
            //up = Vector3.Transform(up, q);
        }

        public void RotateZ(float angle)
        {
            angle = angle * (float)Math.PI / 180;
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
    }

    public delegate void TacingThreadCallback(Vector3[] pixelColors);

    public class TracingThread
    {
        private readonly Camera camera;
        private readonly Scene scene;
        private readonly int start;
        private readonly int end;
        public bool abort = false;

        private TacingThreadCallback callback;

        public TracingThread(Camera camera, Scene scene, int start, int end, TacingThreadCallback callback)
        {
            this.camera = camera;
            this.scene = scene;
            this.start = start;
            this.end = end;
            this.callback = callback;
        }

        public void ThreadProc()
        {
            Vector3[] pixelColors = new Vector3[(end - start) * camera.screenHeight];
            float raysDivider = 1f / camera.raysPerPixel;
            for (int x = start; x < end; x++)
            {
                for (int y = 0; y < camera.screenHeight; y++)
                {
                    if (abort)
                        return;
                    if (camera.raysPerPixel > 1)
                    {
                        Ray[] pixelRays = camera.antiAliasing ? camera.GetRandomizedPixelRays(x, y, camera.raysPerPixel) : camera.GetPixelRays(x, y, camera.raysPerPixel);
                        Vector3 combinedColour = Vector3.Zero;
                        for (int i = 0; i < camera.raysPerPixel; i++)
                        {
                            combinedColour += camera.TraceRay(pixelRays[i], scene);
                        }
                        pixelColors[(x - start) + y * (end - start)] += combinedColour * raysDivider;
                    }
                    else
                    {
                        pixelColors[(x - start) + y * (end - start)] += camera.TraceRay(camera.antiAliasing ? camera.GetRandomizedPixelRay(x, y) : camera.GetPixelRay(x, y), scene);
                    }
                }
            }


            if (!abort)
                callback?.Invoke(pixelColors);
        }
    }
}