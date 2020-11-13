using OpenTK;
using System;
using System.Collections;

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

        public Camera(Vector3 position, Vector3 direction, int width, int height, float screenDistance = 1, ProjectionType projection = ProjectionType.Perspective)
        {
            this.position = position;
            this.direction = direction;
            this.screenWidth = width;
            this.screenHeight = height;
            this.screenDistance = screenDistance;
            this.projection = projection;

            UpdateCamera();
        }

        //Needs to be called everytime the camera's state changes
        public void UpdateCamera()
        {
            Vector3 up = Vector3.Cross(direction, Vector3.UnitX);
            // Make sure up always points up (unless direction is (0, (-)1, 0)
            if (direction.Z < 0)
                up = -up;
            Vector3 left = Vector3.Cross(direction, up);

            screenCenter = position + direction * screenDistance;

            p0 = screenCenter + up + left;
            p1 = screenCenter + up - left;
            p2 = screenCenter - up + left;
        }

        // Maps screen position to world position, u & v = [0, 1]
        public Vector3 GetPoint(float u, float v)
        {
            return p0 + u * (p1 - p0) + v * (p2 - p0);
        }

        // Maps pixel position to world position, x = [1, screenWidth], y = [1, screenHeight]
        public Vector3 GetPixelPoint(int x, int y)
        {
            float u = (1 / (screenWidth - 1)) * (x - 1);
            float v = (1 / (screenHeight - 1)) * (y - 1);
            return GetPoint(u, v);
        }
    }
}