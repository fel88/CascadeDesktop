using OpenTK;
using OpenTK.Mathematics;

namespace CascadeDesktop
{
    public class TriangleInfo
    {
        public VertexInfo[] Vertices;

        public double Area()
        {
            var v0 = Vertices[1].Position - Vertices[0].Position;
            var v1 = Vertices[2].Position - Vertices[0].Position;
            var crs = Vector3d.Cross(v0, v1);
            return crs.Length / 2;
        }
        public Vector3d Normal()
        {
            var v0 = Vertices[1].Position - Vertices[0].Position;
            var v1 = Vertices[2].Position - Vertices[0].Position;
            var crs = Vector3d.Cross(v0, v1);
            return crs.Normalized();
        }

        public Vector3d Center()
        {
            Vector3d z1 = Vector3d.Zero;
            foreach (var item in Vertices)
            {
                z1 += item.Position;
            }
            z1 /= 3;
            return z1;
        }
        public bool Contains(Vector3d v, double eps = 1e-8)
        {
            foreach (var item in Vertices)
            {
                if ((item.Position - v).Length < eps) { return true; }
            }
            return false;
        }
    }
}
