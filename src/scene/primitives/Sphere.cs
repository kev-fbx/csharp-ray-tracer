using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent an (infinite) plane in a scene.
    /// </summary>
    public class Sphere : SceneEntity
    {
        private Vector3 center;
        private double radius;
        private Material material;

        /// <summary>
        /// Construct a sphere given its center point and a radius.
        /// </summary>
        /// <param name="center">Center of the sphere</param>
        /// <param name="radius">Radius of the spher</param>
        /// <param name="material">Material assigned to the sphere</param>
        public Sphere(Vector3 center, double radius, Material material)
        {
            this.center = center;
            this.radius = radius;
            this.material = material;
        }

        /// <summary>
        /// Determine if a ray intersects with the sphere, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit Intersect(Ray ray)
        {
            Vector3 raySphereVec = ray.Origin - this.center;
            double a = ray.Direction.Dot(ray.Direction);
            double b = 2 * ray.Direction.Dot(raySphereVec);
            double c = raySphereVec.LengthSq() - (this.radius * this.radius);

            double discr = b * b - (4 * a * c);

            if (discr < 0.0) return null;

            double root = Math.Sqrt(discr);
            double t1 = (-b - root) / (2.0 / a);
            double t2 = (-b + root) / (2.0 / a);

            double t = -1;
            if (t1 > SceneEntity.EPSILON) t = t1;
            else if (t2 > SceneEntity.EPSILON) t = t2;

            if (t < 0) return null;

            Vector3 intersectionPoint = ray.Origin + ray.Direction * t;
            Vector3 normal = (intersectionPoint - this.center).Normalized();

            return new RayHit(intersectionPoint, normal, ray.Direction, this.material);
        }

        /// <summary>
        /// The material of the sphere.
        /// </summary>
        public Material Material { get { return this.material; } }
    }

}
