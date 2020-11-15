using System.Collections;
using OpenTK.Graphics;
using System.Drawing;

namespace Lighthouse3.Primitives
{

    // Should probably have an enum of defaults?

    public abstract class Primitive
    {
        public Material material;

        public Primitive(Material material)
        {
            this.material = material;
        }


        public abstract Intersection Intersect(Ray ray);
    }
}