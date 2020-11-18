﻿using Lighthouse3.Primitives;
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
                {
                    intersection = i;
                }
            }
            return intersection;
        }

        public Vector3 Trace(Scene scene)
        {
            Intersection intersection = NearestIntersection(scene.primitives);
            if (intersection == null)
                return scene.backgroundColor;

            Vector3 color = Color.Black;

            //Handle diffuse color
            if (intersection.material.diffuse > 0)
            {
                Vector3 illumination = Color.Black;
                foreach (Light light in scene.lights)
                {
                    Vector3 lightColor = light.DirectIllumination(intersection, scene);
                    illumination = illumination + lightColor;
                }
                color = intersection.material.color * illumination * intersection.material.diffuse;
            }

            //Handle specularity
            if (intersection.material.specularity > 0)
            {
                Ray reflection = Reflect(intersection.distance, intersection.normal);
                color += reflection.Trace(scene) * intersection.material.specularity;
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
        public bool backface;
        
        public Intersection(float distance, Ray ray, Vector3 normal, Material material, bool backface)
        {
            this.distance = distance;
            this.ray = ray;
            this.normal = normal;
            this.material = material;
            this.backface = backface;
        }
    }
}