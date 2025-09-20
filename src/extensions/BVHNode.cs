using System.Collections.Generic;

namespace RayTracer
{
    public class BVHNode : SceneEntity
    {
        // Each node of a BVH structure will be the parent of either:
        // a single triangle;
        // a branch of volumes with triangles closer to Xmin;
        // and/or a branch of volumes with triangles closer to Xmax.
        // Each node will also hold a bounding box, which may contain triangles or other bounding boxes depending on 
        // what it's children are.
        private Triangle leafTri;
        private BoundingBox boundingBox;
        private BVHNode left = null;
        private BVHNode right = null;

        public BVHNode(List<Triangle> triangles)
        {
            GrowTree(triangles);
        }

        /// <summary>
        /// Creates a bounding box for all the triangles in an OBJ.
        /// Recursively, this will create smaller and smaller bounding boxes until
        /// we reach the base case of a single triangle.
        /// </summary>
        /// <param name="triangles">Triangles that define the OBJ Model</param>
        private void GrowTree(List<Triangle> triangles)
        {
            if (triangles.Count == 1)
            {
                leafTri = triangles[0];
                boundingBox = BoundingBox.CreateSingleBox(leafTri);
                return;
            }

            // Sort the triangles based on X-coordinate (arbitrary). This makes bounding 
            // boxes generally smaller and balances the tree.
            triangles.Sort((a, b) =>
            {
                // I don't understand the axis alignment stuff so just sort based on v0 X-axis.
                return a.v0.X.CompareTo(b.v0.X);
            });

            boundingBox = BuildBounds(triangles);

            int mid = triangles.Count / 2;
            left = new BVHNode(triangles.GetRange(0, mid));
            right = new BVHNode(triangles.GetRange(mid, triangles.Count - mid));
        }

        /// <summary>
        /// Creates a bounding box for each triangle in the list, and then
        /// combines them into a single bounding box that contains all triangles.
        /// </summary>
        /// <param name="triangles">List of triangles to contain.</param>
        /// <returns>Bounding box of all triangles</returns>
        private static BoundingBox BuildBounds(List<Triangle> triangles)
        {
            BoundingBox bounds = BoundingBox.CreateSingleBox(triangles[0]);
            for (int i = 1; i < triangles.Count; i++)
            {
                bounds = BoundingBox.Union(bounds, BoundingBox.CreateSingleBox(triangles[i]));
            }
            return bounds;
        }

        public RayHit Intersect(Ray ray)
        {
            // If ray doesn't hit this bounding box, skip all the children!!!!!!!
            if (!boundingBox.IntersectsWith(ray)) return null;

            if (leafTri != null) return leafTri.Intersect(ray);

            RayHit leftHit = left.Intersect(ray);
            RayHit rightHit = right.Intersect(ray);

            if (leftHit == null) return rightHit;
            if (rightHit == null) return leftHit;

            double distLeft = (leftHit.Position - ray.Origin).LengthSq();
            double distRight = (rightHit.Position - ray.Origin).LengthSq();

            return distLeft < distRight ? leftHit : rightHit;
        }

        public Material Material
        {
            get
            {
                if (leafTri != null) return leafTri.Material;
                if (left != null) return left.Material;
                return right.Material;
            }
        }
    }
}