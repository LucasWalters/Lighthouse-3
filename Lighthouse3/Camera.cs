using Lighthouse3.Primitives;
using OpenTK;
using System;
using System.Collections;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Lighthouse3.RayTracers;

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

        public enum ProjectionType { Perspective, Orthographic }

        //Top left corner of screen
        Vector3 topLeft;
        //Top right corner of screen
        Vector3 topRight;
        //Bottom left corner of screen
        Vector3 bottomLeft;
        Vector3 screenCenter;
        private Camera() { }
        public Camera(Vector3 position, Vector3 direction, int width, int height, 
            float screenDistance = 1, int raysPerPixel = 1, ProjectionType projection = ProjectionType.Perspective, 
            bool gammaCorrection = false, RayTracer rayTracer = RayTracer.Whitted)
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

            up = Vector3.Cross(direction, Vector3.UnitX).Normalized();
            //Console.WriteLine(up);
            if (Math.Abs(direction.X) == 1f)
            {
                up = Vector3.UnitY;
            }
            // Make sure up always points up (unless direction is (0, (-)1, 0)
            if (direction.Z < 0)
                up = -up;
            UpdateCamera();
        }

        //Needs to be called everytime the camera's state changes
        public void UpdateCamera()
        {
            left = Vector3.Cross(direction, up).Normalized() * ((float)screenWidth / screenHeight);
            //Console.WriteLine(left);
            
            screenCenter = position + direction * screenDistance;

            topLeft = screenCenter + up + left;
            topRight = screenCenter + up - left;
            bottomLeft = screenCenter - up + left;

            // Update Diagonal Length for FOV
            diagonalLength = (bottomLeft - topRight).Length;
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
            float divider = 1f / raysPerPixel;
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
                            combinedColour += TraceRay(pixelRays[i], scene);
                        }
                        pixels[x + y * screenWidth] = ColorToPixel(combinedColour * divider);
                    }
                    else
                    {
                        pixels[x + y * screenWidth] = ColorToPixel(TraceRay(GetPixelRay(x, y), scene));
                    }
                }
            }
            return pixels;
        }

        public int DebugRay(Scene scene, int x, int y)
        {
            if (raysPerPixel > 1)
            {
                Ray[] pixelRays = GetRandomizedPixelRays(x, y, raysPerPixel);
                Vector3 combinedColour = Vector3.Zero;
                for (int i = 0; i < raysPerPixel; i++)
                {
                    combinedColour += TraceRay(pixelRays[i], scene, true);
                }
                float divider = 1f / raysPerPixel;
                return ColorToPixel(combinedColour * divider);
            }
            else
            {
                return ColorToPixel(TraceRay(GetPixelRay(x, y), scene, true));
            }
        }

        private Vector3 TraceRay(Ray ray, Scene scene, bool debug = false)
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
}