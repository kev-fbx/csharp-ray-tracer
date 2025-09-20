using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace RayTracer
{
    /// <summary>
    /// Add-on option C. You should implement your solution in this class template.
    /// </summary>
    public class ObjModel : SceneEntity
    {
        private string objFilePath;
        private Transform transform;
        private Material material;
        private List<Triangle> tris = new List<Triangle>();
        private List<TextureCoord> texCoords = new List<TextureCoord>();
        private BVHNode bvh;

        private const int X_COORD = 1;
        private const int Y_COORD = 2;
        private const int Z_COORD = 3;
        private const int U_COORD = 1;
        private const int V_COORD = 2;
        private const int NUM_FACE_VERTS = 3;
        private const int VERT_POS_IDX = 0;
        private const int TEX_COORD_IDX = 1;
        private const int VERT_NORM_IDX = 2;

        /// <summary>
        /// Construct a new OBJ model.
        /// </summary>
        /// <param name="objFilePath">File path of .obj</param>
        /// <param name="transform">Transform to apply to each vertex</param>
        /// <param name="material">Material applied to the model</param>
        public ObjModel(string objFilePath, Transform transform, Material material)
        {
            this.objFilePath = objFilePath;
            this.transform = transform;
            this.material = material;
            Stopwatch stopwatch = Stopwatch.StartNew();

            string[] lines = File.ReadAllLines(objFilePath);
            List<string> vLines = new List<string>();
            List<string> vtLines = new List<string>();
            List<string> vnLines = new List<string>();
            List<string> fLines = new List<string>();

            foreach (string line in lines)
            {
                if (line.StartsWith("v ")) vLines.Add(line);
                else if (line.StartsWith("vt ")) vtLines.Add(line);
                else if (line.StartsWith("vn ")) vnLines.Add(line);
                else if (line.StartsWith("f ")) fLines.Add(line);
            }

            Vector3[] verts = new Vector3[vLines.Count];
            Vector3[] norms = new Vector3[vnLines.Count];
            TextureCoord[] textureCoords = new TextureCoord[vtLines.Count];

            // Vertex attributes can be read independently of each other
            // so just speed it up with parallel read.
            Parallel.For(0, vLines.Count, i =>
            {
                string[] currLine = vLines[i].Split(" ");
                double x = double.Parse(currLine[X_COORD]);
                double y = double.Parse(currLine[Y_COORD]);
                double z = double.Parse(currLine[Z_COORD]);
                Vector3 currVec = new Vector3(x, y, z);
                currVec = transform.Apply(currVec);
                verts[i] = currVec;
            });

            Parallel.For(0, vtLines.Count, i =>
            {
                string[] currLine = vLines[i].Split(" ");
                double u = double.Parse(currLine[U_COORD]);
                double v = double.Parse(currLine[V_COORD]);
                textureCoords[i] = new TextureCoord(u, v);
            });

            Parallel.For(0, vnLines.Count, i =>
            {
                string[] currLine = vnLines[i].Split(" ");
                double x = double.Parse(currLine[X_COORD]);
                double y = double.Parse(currLine[Y_COORD]);
                double z = double.Parse(currLine[Z_COORD]);
                Vector3 currVecNorm = new Vector3(x, y, z).Normalized();
                norms[i] = currVecNorm;
            });

            foreach (string line in fLines)
            {
                string[] faceAttribs = line.Split(" ");

                Vector3[] faceVertices = new Vector3[3];
                Vector3 faceNormal = new Vector3();
                TextureCoord[] faceTexCoords = new TextureCoord[NUM_FACE_VERTS];

                for (int currFaceVert = 0; currFaceVert < NUM_FACE_VERTS; currFaceVert++)
                {
                    string[] splitFaceAttribs = faceAttribs[currFaceVert + 1].Split("/");

                    int vIdx = int.Parse(splitFaceAttribs[VERT_POS_IDX]) - 1;
                    int nIdx = int.Parse(splitFaceAttribs[VERT_NORM_IDX]) - 1;

                    if (vtLines.Count > 0)
                    {
                        int tIdx = int.Parse(splitFaceAttribs[TEX_COORD_IDX]) - 1;
                        faceTexCoords[currFaceVert] = textureCoords[tIdx];
                    }

                    faceVertices[currFaceVert] = verts[vIdx];
                    faceNormal += norms[nIdx];
                }
                faceNormal = faceNormal.Normalized();

                if (vtLines.Count > 0)
                {
                    tris.Add(new Triangle(faceVertices[X_COORD - 1], faceVertices[Y_COORD - 1], faceVertices[Z_COORD - 1], material, faceNormal, faceTexCoords));
                }
                else
                {
                    tris.Add(new Triangle(faceVertices[X_COORD - 1], faceVertices[Y_COORD - 1], faceVertices[Z_COORD - 1], material, faceNormal));
                }
            }
            bvh = new BVHNode(tris);
            stopwatch.Stop();
            Console.WriteLine($"OBJ load time (mm:ss::ms): {stopwatch.Elapsed.Minutes:D2}:{stopwatch.Elapsed.Seconds:D2}:{stopwatch.Elapsed.Milliseconds}");
        }

        /// <summary>
        /// Given a ray, determine whether the ray hits the object
        /// and if so, return relevant hit data (otherwise null).
        /// </summary>
        /// <param name="ray">Ray data</param>
        /// <returns>Ray hit data, or null if no hit</returns>
        public RayHit Intersect(Ray ray)
        {
            // Write your code here...
            return bvh.Intersect(ray);
        }

        /// <summary>
        /// Applies transformation to triangles of the OBJ
        /// </summary>
        /// <param name="translation">Translation vector</param>
        /// <param name="rotation">Rotation in quaternion form</param>
        public void ApplyTransform(Vector3 translation, Quaternion rotation)
        {
            this.transform = new Transform(
                transform.Position + translation,
                transform.Rotation,
                transform.Scale);

            foreach (Triangle tri in tris)
            {
                tri.v0 += translation;
                tri.v1 += translation;
                tri.v2 += translation;

                Vector3 origin = transform.Position;
                tri.v0 = rotation.Rotate(tri.v0 - origin) + origin;
                tri.v1 = rotation.Rotate(tri.v1 - origin) + origin;
                tri.v2 = rotation.Rotate(tri.v2 - origin) + origin;

                tri.normal = (tri.v1 - tri.v2).Cross(tri.v2 - tri.v0).Normalized();
            }
            bvh = new BVHNode(tris);
        }

        /// <summary>
        /// The material attached to this object.
        /// </summary>
        public Material Material { get { return this.material; } }
    }
}
