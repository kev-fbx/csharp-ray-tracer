using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a ray traced scene, including the objects,
    /// light sources, and associated rendering logic.
    /// </summary>
    public class Scene
    {
        private SceneOptions options;
        private Camera camera;
        private Color ambientLightColor;
        private ISet<SceneEntity> entities;
        private ISet<PointLight> lights;
        private ISet<Animation> animations;
        private const int X_COORD = 0;
        private const int Y_COORD = 1;
        private const double EPSILON = 1e-6;
        private const int MAX_RECURSION_DEPTH = 8;

        /// <summary>
        /// Construct a new scene with provided options.
        /// </summary>
        /// <param name="options">Options data</param>
        public Scene(SceneOptions options = new SceneOptions())
        {
            this.options = options;
            this.camera = new Camera(Transform.Identity);
            this.ambientLightColor = new Color(0, 0, 0);
            this.entities = new HashSet<SceneEntity>();
            this.lights = new HashSet<PointLight>();
            this.animations = new HashSet<Animation>();
        }

        /// <summary>
        /// Set the camera for the scene.
        /// </summary>
        /// <param name="camera">Camera object</param>
        public void SetCamera(Camera camera)
        {
            this.camera = camera;
            if (options.ApertureRadius > 0.0)
            {
                camera.FocalLength = options.FocalLength;
                camera.ApertureRadius = options.ApertureRadius;
            }
        }

        /// <summary>
        /// Set the ambient light color for the scene.
        /// </summary>
        /// <param name="color">Color object</param>
        public void SetAmbientLightColor(Color color)
        {
            this.ambientLightColor = color;
        }

        /// <summary>
        /// Add an entity to the scene that should be rendered.
        /// </summary>
        /// <param name="entity">Entity object</param>
        public void AddEntity(SceneEntity entity)
        {
            this.entities.Add(entity);
        }

        /// <summary>
        /// Add a point light to the scene that should be computed.
        /// </summary>
        /// <param name="light">Light structure</param>
        public void AddPointLight(PointLight light)
        {
            this.lights.Add(light);
        }

        /// <summary>
        /// Add an animation to the scene.
        /// </summary>
        /// <param name="animation">Animation object</param>
        public void AddAnimation(Animation animation)
        {
            this.animations.Add(animation);
        }

        /// <summary>
        /// Render the scene to an output image. This is where the bulk
        /// of your ray tracing logic should go... though you may wish to
        /// break it down into multiple functions as it gets more complex!
        /// </summary>
        /// <param name="outputImage">Image to store render output</param>
        /// <param name="time">Time since start in seconds</param>
        public void Render(Image outputImage, double time = 0)
        {
            // Begin writing your code here...
            var stopwatch = Stopwatch.StartNew();
            int DOF_MaxSamples = 2;

            foreach (Animation anim in animations)
            {
                if (anim is SimpleAnimation simpleAnim)
                {
                    simpleAnim.Update();
                }
            }

            int aaSamples = options.AAMultiplier;
            int dofSamples = (camera.ApertureRadius > 0.0) ? DOF_MaxSamples : 1;
            int totalSamples = aaSamples * aaSamples * dofSamples * dofSamples;

            Vector3 cameraOrigin = camera.Transform.Position;
            Quaternion camRotation = camera.Transform.Rotation;

            // Apply camera rotation transforms to local axes
            Vector3 cameraRight = camRotation.Rotate(Vector3.xVector);
            Vector3 cameraUp = camRotation.Rotate(Vector3.yVector);
            Vector3 cameraForward = camRotation.Rotate(Vector3.zVector);

            // Pixels are currently in image/raster space.
            Parallel.For(0, outputImage.Width, x =>
            {
                for (int y = 0; y < outputImage.Height; y++)
                {
                    Color pixelColor = new Color(0, 0, 0);

                    // Divide each pixel into a square grid, fire a ray into each subpixel,
                    // then average the colour.
                    for (int ssX = 0; ssX < options.AAMultiplier; ssX++)
                    {
                        for (int ssY = 0; ssY < options.AAMultiplier; ssY++)
                        {
                            double subPixelX = x + ((double)ssX / options.AAMultiplier);
                            double subPixelY = y + ((double)ssY / options.AAMultiplier);

                            double[] mappedCoords = MapCoordinates(subPixelX, subPixelY, outputImage);
                            
                            for (int dofX = 0; dofX < dofSamples; dofX++)
                            {
                                for (int dofY = 0; dofY < dofSamples; dofY++)
                                {
                                    Vector3 rayDirection = (cameraForward + cameraRight * mappedCoords[X_COORD] + cameraUp * mappedCoords[Y_COORD]).Normalized();
                                    Ray ray = camera.GenerateRay(rayDirection);
                                    pixelColor += TraceRay(ray, 0);
                                }
                            }
                        }
                    }
                    pixelColor /= totalSamples;
                    outputImage.SetPixel(x, y, pixelColor);
                }
            });
            stopwatch.Stop();
            Console.WriteLine($"Render time (min:sec:ms): {stopwatch.Elapsed.Minutes:D2}:{stopwatch.Elapsed.Seconds:D2}:{stopwatch.Elapsed.Milliseconds})");
        }

        /// <summary>
        /// Converts a coordinate pair from raster to camera space.
        /// Logic largely understood from:
        /// - https://www.scratchapixel.com/lessons/3d-basic-rendering/introduction-to-ray-tracing/implementing-the-raytracing-algorithm.html
        /// - https://www.scratchapixel.com/lessons/3d-basic-rendering/ray-tracing-generating-camera-rays/standard-coordinate-systems.html
        /// - https://www.scratchapixel.com/lessons/3d-basic-rendering/computing-pixel-coordinates-of-3d-point/mathematics-computing-2d-coordinates-of-3d-points.html
        /// </summary>
        /// <param name="xRaster">X-coordinate in raster space.</param>
        /// <param name="yRaster">Y-coordinate in raster space.</param>
        /// <param name="image">Output image of ray tracer program.</param>
        /// <returns>X,Y coordinate pair in camera space.</returns>
        private static double[] MapCoordinates(double xRaster, double yRaster, Image image)
        {
            double x = xRaster;
            double y = yRaster;

            double aspectRatio = (double)image.Width / (double)image.Height;
            double horizontalFOV = 60.0 * (Math.PI / 180.0);
            double fov = Math.Tan(horizontalFOV / 2.0);

            // Centre pixels and convert to NDC
            x = (x + 0.5) / image.Width;
            y = (y + 0.5) / image.Height;

            // Convert to Screen Space
            x = 2.0 * x - 1.0;
            y = 1.0 - 2.0 * y;

            // Convert to Camera Space
            x = x * fov * aspectRatio;
            y = y * fov;

            return [x, y];
        }

        /// <summary>
        /// Traces a ray recursively.
        /// </summary>
        /// <param name="ray">Ray to trace, defined by an origin and direction.</param>
        /// <param name="depth">Recursion depth of traced ray, used for reflection and refraction rays.</param>
        /// <returns>Color of last thing the ray hit.</returns>
        private Color TraceRay(Ray ray, int depth)
        {
            if (depth > MAX_RECURSION_DEPTH) return new Color(0, 0, 0);

            RayHit closestHit = null;
            SceneEntity hitEntity = null;
            double minDepth = double.MaxValue;

            // Initial ray intersection test, including Z-depth test
            foreach (SceneEntity entity in entities)
            {
                RayHit hit = entity.Intersect(ray);
                if (hit == null) continue;

                double dist = (hit.Position - ray.Origin).LengthSq();
                if (dist < minDepth)
                {
                    minDepth = dist;
                    closestHit = hit;
                    hitEntity = entity;
                }
            }

            if (hitEntity == null) return new Color(0, 0, 0);

            // Actual lighting logic

            Color pixelColor = hitEntity.Material.AmbientColor * ambientLightColor;

            // Phong lighting contributions
            foreach (PointLight light in lights)
            {
                Vector3 shadowRayDir = light.Position - closestHit.Position;
                double shadowRayDist = (light.Position - closestHit.Position).Length();
                Ray shadowRay = new Ray(closestHit.Position + closestHit.Normal * EPSILON, shadowRayDir.Normalized());
                bool occluded = false;

                // Shadow Catcher
                foreach (SceneEntity other in entities)
                {
                    if (other == hitEntity) continue;
                    RayHit shadowHit = other.Intersect(shadowRay);
                    if (shadowHit == null) continue;

                    double hitDist = (shadowHit.Position - closestHit.Position).Length();
                    if (hitDist > EPSILON && hitDist < shadowRayDist)
                    {
                        occluded = true;
                        break;
                    }
                }
                // If not in shadow, include diffuse and specular color contributions
                if (!occluded) pixelColor += Color.CalcPhongLighting(hitEntity, closestHit, light, camera);
            }

            Vector3 D = ray.Direction.Normalized();
            Vector3 N = closestHit.Normal.Normalized();

            // Reflectivity contributions
            if (hitEntity.Material.Reflectivity > 0)
            {
                Vector3 reflectedDir = D - 2.0 * (D.Dot(N)) * N;
                Ray reflectionRay = new Ray(closestHit.Position + N * EPSILON, reflectedDir.Normalized());
                Color reflectionColor = TraceRay(reflectionRay, depth + 1);

                pixelColor += reflectionColor * hitEntity.Material.Reflectivity;
            }

            // Refractivity contributions
            if (hitEntity.Material.Transmissivity > 0)
            {
                double idx_i = 1.0;
                double idx_t = hitEntity.Material.RefractiveIndex;
                Vector3 surfaceNorm = N;

                Vector3 offset = N * EPSILON;

                bool exiting_object = false;

                // The ray is exiting an object.
                if (D.Dot(N) > 0.0)
                {
                    exiting_object = true;
                    surfaceNorm = -N;

                    // Swap values
                    (idx_i, idx_t) = (idx_t, idx_i);
                }

                double ratio = idx_i / idx_t;
                double cos_i = -D.Dot(surfaceNorm);

                // If we are exiting an object, then we nudge the ray slightly outwards along the 
                // direction of the transmitted vector to avoid self collision. If we are entering an object,
                // we need to offset slightly as well.

                if (!exiting_object)
                {
                    offset *= -1;
                }

                double root = 1.0 - ratio * ratio * (1.0 - cos_i * cos_i);

                // If no real solution to square root, fallback to a reflection (total internal reflection).
                if (root < 0.0)
                {
                    Vector3 reflectedDir = D - 2.0 * D.Dot(surfaceNorm) * surfaceNorm;
                    Ray reflectionRay = new Ray(closestHit.Position + offset, reflectedDir.Normalized());
                    Color reflectionColor = TraceRay(reflectionRay, depth + 1);
                    pixelColor += reflectionColor * hitEntity.Material.Reflectivity;
                }
                else
                {
                    Vector3 T = ratio * D + (ratio * cos_i - Math.Sqrt(root)) * surfaceNorm;
                    Ray refracRay = new Ray(closestHit.Position + T * EPSILON, T.Normalized());
                    Color refracColor = TraceRay(refracRay, depth + 1);
                    pixelColor += refracColor * hitEntity.Material.Transmissivity;
                }
            }
            return pixelColor;
        }
    }
}
