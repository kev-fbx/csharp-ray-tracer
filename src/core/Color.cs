using System;

namespace RayTracer
{
    /// <summary>
    /// Immutable structure to represent a color as r/g/b with 0-1 ranges
    /// </summary>
    public readonly struct Color
    {
        private readonly double r, g, b;

        /// <summary>
        /// Construct a new color structure given red, green, blue
        /// components (0-1 ranges).
        /// </summary>
        /// <param name="r">Red component (0-1)</param>
        /// <param name="g">Blue component (0-1)</param>
        /// <param name="b">Green component (0-1)</param>
        public Color(double r, double g, double b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        /// <summary>
        /// Convert color structure to string.
        /// </summary>
        /// <returns>Color as string in form (r, g, b)</returns>
        public override string ToString()
        {
            return "(" + this.r + "," + this.g + "," + this.b + ")";
        }

        /// <summary>
        /// The red component of the color (0-1).
        /// </summary>
        public double R { get { return this.r; } }

        /// <summary>
        /// The green component of the color (0-1).
        /// </summary>
        public double G { get { return this.g; } }

        /// <summary>
        /// The blue component of the color (0-1).
        /// </summary>
        public double B { get { return this.b; } }

        /// <summary>
        /// Multiply each color component by a single scalar value.
        /// </summary>
        /// <param name="a">Color structure</param>
        /// <param name="b">Scalar multiple</param>
        /// <returns>Multiplied color structure</returns>
        public static Color operator *(Color a, double b)
        {
            return new Color(a.r * b, a.g * b, a.b * b);
        }

        /// <summary>
        /// Multiply two colors together component-wise.
        /// </summary>
        /// <param name="a">First color structure</param>
        /// <param name="b">Second color structure</param>
        /// <returns>Multiplied color structure</returns>
        public static Color operator *(Color a, Color b)
        {
            return new Color(a.r * b.r, a.g * b.g, a.b * b.b);
        }

        /// <summary>
        /// Divide each color component by a single scalar value.
        /// </summary>
        /// <param name="a">Color structure</param>
        /// <param name="b">Scalar divisor</param>
        /// <returns>Divided color structure</returns>
        public static Color operator /(Color a, double b)
        {
            return new Color(a.r / b, a.g / b, a.b / b);
        }

        /// <summary>
        /// Divide two colors component-wise.
        /// </summary>
        /// <param name="a">Color structure</param>
        /// <param name="b">Divisor color structure</param>
        /// <returns>Divided color structure</returns>
        public static Color operator /(Color a, Color b)
        {
            return new Color(a.r / b.r, a.g / b.g, a.b / b.b);
        }

        /// <summary>
        /// Add two colors together component-wise.
        /// </summary>
        /// <param name="a">First color structure</param>
        /// <param name="b">Second color structure</param>
        /// <returns>Multiplied color structure</returns>
        public static Color operator +(Color a, Color b)
        {
            return new Color(a.r + b.r, a.g + b.g, a.b + b.b);
        }

        /// <summary>
        /// Calculates the diffuse reflection and specular reflection color contribution of a light for a pixel.
        /// </summary>
        /// <param name="entity">Object in scene with attributes.</param>
        /// <param name="hit">Ray intersecting with entity.</param>
        /// <param name="light">Light that hits entity.</param>
        /// <param name="camera">Scene camera.</param>
        /// <returns></returns>
        public static Color CalcPhongLighting(SceneEntity entity, RayHit hit, PointLight light, Camera camera)
        {
            Vector3 hitNormal = hit.Normal.Normalized();
            Vector3 incidentDir = (light.Position - hit.Position).Normalized();
            Vector3 reflectedDir = ((2 * incidentDir.Dot(hitNormal) * hitNormal) - incidentDir).Normalized();
            Vector3 cameraDir = (camera.Transform.Position - hit.Position).Normalized();

            Color diffuseComponent = entity.Material.DiffuseColor * light.Color * Math.Max(0, hitNormal.Dot(incidentDir));
            Color specularComponent = entity.Material.SpecularColor * light.Color * Math.Pow(Math.Max(0, reflectedDir.Dot(cameraDir)), entity.Material.Shininess);
            return diffuseComponent + specularComponent;
        }
    }
}
