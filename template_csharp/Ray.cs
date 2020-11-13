using OpenTK;
using System.Collections;

namespace Lighthouse3
{
    public struct Ray
    {
        Vector3 startPosition;
        Vector3 direction;

        // Direction should be normalized
        public Ray(Vector3 startPosition, Vector3 direction)
        {
            this.startPosition = startPosition;
            this.direction = direction;
        }

        public Vector3 GetPoint(float distance)
        {
            return startPosition + direction * distance;
        }
    }
}