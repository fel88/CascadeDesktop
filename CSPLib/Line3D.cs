using System;
using OpenTK;
using OpenTK.Mathematics;

namespace CSPLib
{
    public class Line3D
    {
        public Vector3d Start;
        public Vector3d End;
        public Vector3d Dir
        {
            get
            {
                return (End - Start).Normalized();
            }
        }

        public bool IsPointOnLine(Vector3d pnt, float epsilon = 10e-6f)
        {
            float tolerance = 10e-6f;
            var d1 = pnt - Start;
            if (d1.Length < tolerance) return true;
            if ((End - Start).Length < tolerance) throw new Exception("degenerated 3d line");
            var crs = Vector3d.Cross(d1.Normalized(), (End - Start).Normalized());
            return Math.Abs(crs.Length) < epsilon;
        }
        public bool IsPointInsideSegment(Vector3d pnt, float epsilon = 10e-6f)
        {
            if (!IsPointOnLine(pnt, epsilon)) return false;
            var v0 = (End - Start).Normalized();
            var v1 = pnt - Start;
            var crs = Vector3d.Dot(v0, v1) / (End - Start).Length;
            return !(crs < 0 || crs > 1);
        }
        public bool IsSameLine(Line3D l)
        {
            return IsPointOnLine(l.Start) && IsPointOnLine(l.End);
        }

        public void Shift(Vector3d vector3)
        {
            Start += vector3;
            End += vector3;
        }
    }
}
