using Lighthouse3.Primitives;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections;

namespace Lighthouse3
{
    public class Ray
    {
        public Vector3 origin;
        public Vector3 direction;

        // Direction should be normalized
        public Ray(Vector3 startPosition, Vector3 direction)
        {
            this.origin = startPosition;
            this.direction = direction;
        }

        public Vector3 GetPoint(float distance)
        {
            return origin + direction * distance;
        }

        public Intersection NearestIntersection(Primitive[] primitives)
        {
            Intersection intersection = null;
            foreach(Primitive primitive in primitives)
            {
                Intersection i = primitive.Intersect(this);
                if (i != null && (intersection == null || i.distance < intersection.distance))
                    intersection = i;
            }
            return intersection;
        }

        public Color4 Trace(Scene scene)
        {
            Intersection intersection = NearestIntersection(scene.primitives);
            if (intersection == null)
                return scene.backgroundColor;

            Color4 color = Color4.Black;

            //Handle diffuse color
            if (intersection.material.diffuse > 0)
            {
                Color4 illumination = Color4.Black;
                foreach (Light light in scene.lights)
                {
                    Color4 lightColor = light.DirectIllumination(intersection, scene);
                    illumination = illumination.Add(lightColor);
                }
                color = color.Add(intersection.material.color.Multiply(illumination.Multiply(intersection.material.diffuse)));
            }

            //Handle specularity
            if (intersection.material.specularity > 0)
            {
                Ray reflection = Reflect(intersection.distance, intersection.normal);
                color = color.Add(reflection.Trace(scene).Multiply(intersection.material.specularity));
            }

            return color;
        }

        public Ray Reflect(float distance, Vector3 normal)
        {
            Vector3 rOrigin = GetPoint(distance);
            Vector3 rDirection = direction - 2 * normal * Vector3.Dot(direction, normal);
            return new Ray(rOrigin, rDirection.Normalized());
        }
    }
    public class Intersection
    {
        public float distance = -1;
        public Ray ray;
        public Vector3 normal;
        public Material material;
        
        public Intersection(float distance, Ray ray, Vector3 normal, Material material)
        {
            this.distance = distance;
            this.ray = ray;
            this.normal = normal;
            this.material = material;
        }
    }
}