using OpenTK;
using OpenTK.Mathematics;

namespace CascadeDesktop
{
    public class IntersectInfo
    {
        public double Distance;
        public TriangleInfo Target;
        public Model Model;
        public Vector3d Point { get; set; }
        public object Parent;
        public object Tag { get; set; }
    }
}
