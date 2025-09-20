using System;

namespace RayTracer
{
    /// <summary>
    /// Represents a camera in the ray tracing scene.
    /// </summary>
    public class Camera : SceneEntity
    {
        public Transform Transform { get; private set; }
        public double ApertureRadius { get; set; }
        public double FocalLength { get; set; }


        /// <summary>
        /// Construct a new camera with a specified transformation.
        /// </summary>
        /// <param name="transform">Transformation to apply to the camera</param>
        public Camera(Transform transform, double apertureRadius = 0.0, double focalLength = 1.0)
        {
            this.Transform = transform;
            ApertureRadius = apertureRadius;
            FocalLength = focalLength;
        }

        /// <summary>
        /// Generates a ray firing along a given direction.
        /// If DOF is defined, the ray is given a randomised direction (random point -> focal point).
        /// </summary>
        /// <param name="direction">Direction of ray.</param>
        /// <returns>Ray defined by camera origin and a set/random direction.</returns>
        public Ray GenerateRay(Vector3 direction)
        {
            Vector3 cameraOrigin = this.Transform.Position;
            Vector3 focalPoint = cameraOrigin + direction * FocalLength;

            if (ApertureRadius > 0.0)
            {
                Vector3 aperturePoint = cameraOrigin + RandomPointOnDisk(ApertureRadius);
                Vector3 randRayDirection = (focalPoint - aperturePoint).Normalized();

                return new Ray(aperturePoint, randRayDirection);
            }
            else
            {
                Vector3 rayDirection = direction.Normalized();
                return new Ray(cameraOrigin, rayDirection);
            }
        }

        /// <summary>
        /// Generates a random point on a 2D sensor disk.
        /// </summary>
        /// <param name="rad">Radius of aperture</param>
        /// <returns>Random point on sensor disk in Vector3 form.</returns>
        private static Vector3 RandomPointOnDisk(double rad)
        {
            Random rand = new Random();
            double r = Math.Sqrt(rand.NextDouble()) * rad;
            double theta = rand.NextDouble() * 2.0 * Math.PI;
            return new Vector3(r * Math.Cos(theta), r * Math.Sin(theta), 0);
        }

        /// <summary>
        /// Cameras do not intersect with rays in the same way as other entities.
        /// This method can is left unimplemented.
        /// </summary>
        public RayHit Intersect(Ray ray)
        {
            throw new NotImplementedException("Camera does not support intersection checks.");
        }

        /// <summary>
        /// Cameras do not have a material in the same way as other entities.
        /// This property is left unimplemented.
        /// </summary>
        public Material Material
        {
            get { throw new NotImplementedException("Camera does not have a material."); }
        }
    }
}
