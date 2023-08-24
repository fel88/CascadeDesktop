using OpenTK;

namespace CascadeDesktop
{
    public class Plane
    {
        public Vector3d Location;
        public Vector3d Normal;

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
