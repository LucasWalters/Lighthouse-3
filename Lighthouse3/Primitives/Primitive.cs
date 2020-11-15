﻿using System.Collections;
using OpenTK.Graphics;
using System.Drawing;

namespace Lighthouse3.Primitives
{

    // Should probably have an enum of defaults?
    public class Material
    {
        
        public Color4 color;

        // Could call it absorption, which is the inverse of reflectiveness
        public float reflectiveness;
        // Texture?

        // Constructor for testing purposes only
        public Material()
        {
            this.color = new Color4(255,0,0,1);
            this.reflectiveness = 0;
        }

        public Material(float r, float g, float b, float a, float reflectiveness) 
        {
            this.color = new Color4(r, g, b, a);
            this.reflectiveness = reflectiveness;
        }
        
    }

    public class Primitive
    {
        public Material material;

        public Primitive(Material material)
        {
            this.material = material;
        }

    }
}