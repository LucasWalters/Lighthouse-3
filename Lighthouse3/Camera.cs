using Lighthouse3.Primitives;
using OpenTK;
using System;
using System.Collections;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Lighthouse3
{
    public class Camera
    {
        //World position of the camera
        public Vector3 position;
        //View direction of the camera - should be normalized
        public Vector3 direction;
        //Projection type of camera, perspective is default and orthographic means rays are cast parallel to eachother
        public ProjectionType projection;
        //Distance to the screen (focal length)
        public float screenDistance;
        //Number of rays sent per pixel
        public int raysPerPixel;

        public bool gammaCorrection = false;

        //Horizontal field of view of the camera
        //public float fov;
        //Screen pixels
        public int screenWidth;
        public int screenHeight;

        public enum ProjectionType { Perspective, Orthographic }

        //Top left corner of screen
        Vector3 p0;
        //Top right corner of screen
        Vector3 p1;
        //Bottom left corner of screen
        Vector3 p2;
        Vector3 screenCenter;
        private Camera() { }
        public Camera(Vector3 position, Vector3 direction, int width, int height, float screenDistance = 1, int raysPerPixel = 1, ProjectionType projection = ProjectionType.Perspective)
        {
            this.position = position;
            this.direction = direction;
            this.screenWidth = width;
            this.screenHeight = height;
            this.screenDistance = screenDistance;
            this.projection = projection;
            this.raysPerPixel = raysPerPixel;

            UpdateCamera();
        }

        //Needs to be called everytime the camera's state changes
        public void UpdateCamera()
        {
            Vector3 up = Vector3.Cross(direction, Vector3.UnitX).Normalized();
            //Console.WriteLine(up);
            if (Math.Abs(direction.X) == 1f)
            {
                up = Vector3.UnitY;
            }
            // Make sure up always points up (unless direction is (0, (-)1, 0)
            if (direction.Z < 0)
                up = -up;
            Vector3 left = Vector3.Cross(direction, up).Normalized() * ((float)screenWidth / screenHeight);
            //Console.WriteLine(left);

            screenCenter = position + direction * screenDistance;

            p0 = screenCenter + up + left;
            p1 = screenCenter + up - left;
            p2 = screenCenter - up + left;
        }

        // Maps screen position to world position, u & v = [0, 1]
        public Vector3 GetPointPos(float u, float v)
        {
            return p0 + u * (p1 - p0) + v * (p2 - p0);
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

        public Ray[] GetRandomizedPixelRays(int x, int y, int rayCount)
        {
            Ray[] rays = new Ray[rayCount];
            for (int i = 0; i < rayCount; i++)
            {
                Vector3 point = GetRandomizedPixelPos(x, y);
                Vector3 rayDir = (point - position).Normalized();
                rays[i] = new Ray(position, rayDir);
            }
            return rays;
        }

        public int[] Frame(Scene scene)
        {
            int[] pixels = new int[screenWidth * screenHeight];
            for (int x = 0; x < screenWidth; x++)
            {
                for (int y = 0; y < screenHeight; y++)
                {
                    if (raysPerPixel > 1)
                    { 
                        Ray[] pixelRays = GetRandomizedPixelRays(x, y, raysPerPixel);
                        Vector3 combinedColour = Vector3.Zero;
                        for (int i = 0; i < raysPerPixel; i++)
                        {
                            combinedColour += pixelRays[i].Trace(scene);

                        }
                        float divider = 1f / raysPerPixel;
                        pixels[x + y * screenWidth] = ColorToPixel(combinedColour * divider);
                    }
                    else
                    {
                        pixels[x + y * screenWidth] = ColorToPixel(GetPixelRay(x, y).Trace(scene));
                    }
                }
            }
            return pixels;
        }

        private int ColorToPixel(Vector3 color)
        {
            if (gammaCorrection)
                color = color.Sqrt();
            
            color *= 256;
            int r = Calc.Clamp((int)color.X, 0, 255);
            int g = Calc.Clamp((int)color.Y, 0, 255);
            int b = Calc.Clamp((int)color.Z, 0, 255);

            return b + (g << 8) + (r << 16);
        }


    }
}