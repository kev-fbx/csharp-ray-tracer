using System;

namespace RayTracer
{
    // Custom bounding box class. Utilises theory for Axis-Aligned Bounding Boxes.
    // Most understanding came from https://box2d.org/files/ErinCatto_DynamicBVH_GDC2019.pdf
    public class BoundingBox
    {
        public Vector3 lowerBound;
        public Vector3 upperBound;

        public BoundingBox(Vector3 lowerBound, Vector3 upperBound)
        {
            this.lowerBound = lowerBound;
            this.upperBound = upperBound;
        }

        /// <summary>
        /// Used for creating a bounding box around a single triangle.
        /// </summary>
        /// <param name="tri">Triangle to contain in bounding box.</param>
        /// <returns></returns>
        public static BoundingBox CreateSingleBox(Triangle tri)
        {
            Vector3 lowerBound = new Vector3(
                Math.Min(Math.Min(tri.v0.X, tri.v1.X), tri.v2.X),
                Math.Min(Math.Min(tri.v0.Y, tri.v1.Y), tri.v2.Y),
                Math.Min(Math.Min(tri.v0.Z, tri.v1.Z), tri.v2.Z));

            Vector3 upperBound = new Vector3(
                Math.Max(Math.Max(tri.v0.X, tri.v1.X), tri.v2.X),
                Math.Max(Math.Max(tri.v0.Y, tri.v1.Y), tri.v2.Y),
                Math.Max(Math.Max(tri.v0.Z, tri.v1.Z), tri.v2.Z));

            return new BoundingBox(lowerBound, upperBound);
        }

        /// <summary>
        /// Returns a new bounding box that contains the bounding boxes a and b.
        /// </summary>
        /// <param name="a">First bounding box.</param>
        /// <param name="b">Second bounding box.</param>
        /// <returns>Bounding box containing bounding boxes a and b.</returns>
        public static BoundingBox Union(BoundingBox a, BoundingBox b)
        {
            Vector3 lowerBound = new Vector3(
                Math.Min(a.lowerBound.X, b.lowerBound.X),
                Math.Min(a.lowerBound.Y, b.lowerBound.Y),
                Math.Min(a.lowerBound.Z, b.lowerBound.Z));
            Vector3 upperBound = new Vector3(
                Math.Max(a.upperBound.X, b.upperBound.X),
                Math.Max(a.upperBound.Y, b.upperBound.Y),
                Math.Max(a.upperBound.Z, b.upperBound.Z));

            return new BoundingBox(lowerBound, upperBound);
        }

        /// <summary>
        /// Tests for intersection between ray and the bounding box. Implements slab method for intersection testing.
        /// Logic largely from https://raytracing.github.io/books/RayTracingTheNextWeek.html#boundingvolumehierarchies 
        /// </summary>
        /// <param name="ray">Traced ray.</param>
        /// <returns>True if intersecting.</returns>
        public bool IntersectsWith(Ray ray)
        {
            double tMin = double.NegativeInfinity;
            double tMax = double.PositiveInfinity;
            double invD = 0.0;
            double t0 = 0.0;
            double t1 = 0.0;

            // For each axis slab, we check if the ray falls in the defined slab range or not.
            // If it doesn't, early return false.
            for (int i = 0; i < 3; i++)
            {
                switch(i)
                {
                    case 0:
                        invD = 1.0 / ray.Direction.X;
                        t0 = (lowerBound.X - ray.Origin.X) * invD;
                        t1 = (upperBound.X - ray.Origin.X) * invD;
                        break;
                    case 1:
                        invD = 1.0 / ray.Direction.Y;
                        t0 = (lowerBound.Y - ray.Origin.Y) * invD;
                        t1 = (upperBound.Y - ray.Origin.Y) * invD;
                        break;
                    case 2:
                        invD = 1.0 / ray.Direction.Z;
                        t0 = (lowerBound.Z - ray.Origin.Z) * invD;
                        t1 = (upperBound.Z - ray.Origin.Z) * invD;
                        break;
                }

                // If ray direction is negative, account for by swapping t values.
                if (invD < 0.0)
                {
                    (t1, t0) = (t0, t1);
                }
                tMin = Math.Max(tMin, t0);
                tMax = Math.Min(tMax, t1);

                if (tMax <= tMin) return false;
            }
            return true;
        }
    }
}