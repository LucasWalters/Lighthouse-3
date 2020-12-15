using System.Collections;
using OpenTK.Graphics;
using System.Drawing;
using OpenTK;

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


        public abstract bool Intersect(Ray ray, out float t);

        public abstract Vector3 Normal(Intersection intersection = null);

        public abstract Vector3 Center();

        public abstract Vector3 Min();

        public abstract Vector3 Max();
    }
}