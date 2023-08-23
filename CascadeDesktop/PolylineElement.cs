using OpenTK;
using System.Collections.Generic;
using System.Linq;

namespace CascadeDesktop
{
    public class PolylineElement : IElement
    {
        public Vector2d Start => Points[0];
        public Vector2d End => Points[Points.Length - 1];
        public Vector2d[] Points { get; set; }

        public double Length
        {
            get
            {
                double ret = 0;
                for (int i = 1; i < Points.Length; i++)
                {
                    ret += (Points[i - 1] - Points[i]).Length;
                }
                return ret;
            }
        }

        public IElement Clone()
        {
            return new PolylineElement()
            {
                Points = Points.ToArray()
            };
        }

        public IEnumerable<Vector2d> GetPoints(double eps = 1E-05)
        {
            return Points;
        }

        public void Reverse()
        {
            Points = Points.Reverse().ToArray();
        }
    }
}
