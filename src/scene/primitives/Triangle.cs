using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a triangle in a scene represented by three vertices.
    /// </summary>
    public class Triangle : SceneEntity
    {
        public Vector3 v0, v1, v2;
        private Material material;
        public Vector3 normal = Vector3.Zero;
        private TextureCoord[] texCoords;

        /// <summary>
        /// Construct a triangle object given three vertices.
        /// </summary>
        /// <param name="v0">First vertex position</param>
        /// <param name="v1">Second vertex position</param>
        /// <param name="v2">Third vertex position</param>
        /// <param name="material">Material assigned to the triangle</param>
        public Triangle(Vector3 v0, Vector3 v1, Vector3 v2, Material material)
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
            this.material = material;
        }

        public Triangle(Vector3 v0, Vector3 v1, Vector3 v2, Material material, Vector3 normal, TextureCoord[] texCoords)
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
            this.material = material;
            this.normal = normal;
            this.texCoords = texCoords;
        }

        public Triangle(Vector3 v0, Vector3 v1, Vector3 v2, Material material, Vector3 normal)
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
            this.material = material;
            this.normal = normal;
        }
        /// <summary>
        /// Determine if a ray intersects with the triangle, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit Intersect(Ray ray)
        {
            Vector3 e1 = v1 - v0;
            Vector3 e2 = v2 - v0;
            normal = e1.Cross(e2);

            // Plane intersection calculation.

            double rayNormDot = normal.Dot(ray.Direction);
            if (Math.Abs(rayNormDot) < SceneEntity.EPSILON) return null;

            double tParam = normal.Dot(v0 - ray.Origin) / rayNormDot;
            if (tParam < SceneEntity.EPSILON) return null;

            Vector3 P = ray.Origin + ray.Direction * tParam;

            // Inside-Outside testing.

            Vector3 v0p = v0 - P;
            Vector3 v1p = v1 - P;
            Vector3 v2p = v2 - P;

            Vector3 areaU = v0p.Cross(v1p) / 2.0;
            Vector3 areaV = v2p.Cross(v0p) / 2.0;
            Vector3 areaW = v1p.Cross(v2p) / 2.0;
            double areaTotal = normal.Dot(normal) / 2.0;

            double u = normal.Dot(areaU / areaTotal);
            double v = normal.Dot(areaV / areaTotal);
            double w = normal.Dot(areaW / areaTotal);

            if (u < -SceneEntity.EPSILON || v < -SceneEntity.EPSILON || w < -SceneEntity.EPSILON) return null;
            if (Math.Abs(u + v + w - 1.0) > SceneEntity.EPSILON) return null;

            return new RayHit(P, normal.Normalized(), ray.Direction, this.material);
        }

        /// <summary>
        /// The material of the triangle.
        /// </summary>
        public Material Material { get { return this.material; } }
        public Vector3 Normal { get { return this.normal; } }
    }
}
