using OpenTK;
using OpenTK.Mathematics;

namespace CascadeDesktop
{
    public class Plane
    {
        public Plane()
        { 
        }

        public Plane(Vector3d normal, double w)
        {
            Normal = normal;
            W = w;
        }
        public double W;


        public Vector3d Location;
        public Vector3d Normal;
        public static Plane FromPoints(Vector3d a, Vector3d b, Vector3d c)
        {
            var n = Vector3d.Cross((b - a), (c - a)).Normalized();
            return new Plane(n, Vector3d.Dot(n, a));
        }

        public Vector3d GetProjPoint(Vector3d point)
        {
            var v = point - Location;
            var nrm = Normal;
            var dist = Vector3d.Dot(v, nrm);
            var proj = point - dist * nrm;
            return proj;
        }
    }
}
