using System;

namespace RayTracer.src.scene
{
    public class AreaLight : SceneEntity
    {
        private Vector3 position;
        private Vector3 normal;
        private Vector3 up;
        private double width;
        private double height;
        private Material material;
        private Color lightColor;
        private Random rand;

        public AreaLight(Vector3 position, Vector3 normal, Vector3 up, double width, double height, Color lightColor, Material material)
        {
            this.position = position;
            this.normal = normal.Normalized();
            this.up = up.Normalized();
            this.width = width;
            this.height = height;
            this.lightColor = lightColor;
            this.material = material;
            rand = new Random();
        }

        private Vector3 SamplePoint()
        {
            Vector3 right = up.Cross(normal).Normalized();

            double u = (rand.NextDouble() - 0.5) * width;
            double v = (rand.NextDouble() - 0.5) * height;

            return position + right * u + up * v;
        }

        public Vector3[] GetSamples(int sampleCount)
        {
            Vector3[] samples = new Vector3[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                samples[i] = SamplePoint();
            }
            return samples;
        }

        public RayHit Intersect(Ray ray)
        {
            double rayNormDot = ray.Direction.Dot(this.normal);

            if (Math.Abs(rayNormDot) < SceneEntity.EPSILON) return null;

            double tParam = normal.Dot(position - ray.Origin) / rayNormDot;
            if (tParam < SceneEntity.EPSILON) return null;

            Vector3 P = ray.Origin + ray.Direction * tParam;
            Vector3 toP = P - position;

            Vector3 right = up.Cross(normal).Normalized();

            double projRight = toP.Dot(right);
            double projUp = toP.Dot(up);

            if (Math.Abs(projRight) > width / 2 ||  Math.Abs(projUp) > height / 2) return null;

            return new RayHit(P, normal.Normalized(), ray.Direction, material);
        }

        public Material Material { get { return this.material; } }
    }
}
