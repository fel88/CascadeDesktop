using System;
using System.Collections.Generic;
using System.Data;
using OpenTK;
using System.Linq;
using OpenTK.Mathematics;

namespace CSPLib
{
    public class Plane
    {
        public Plane()
        {

        }

        public Vector3d Position { get; set; }


        public Vector3d Normal { get; set; }




        public Vector3d[] GetBasis()
        {
            Vector3d[] shifts = new[] { Vector3d.UnitX, Vector3d.UnitY, Vector3d.UnitZ };
            Vector3d axis1 = Vector3d.Zero;
            for (int i = 0; i < shifts.Length; i++)
            {
                var proj = ProjPoint(Position + shifts[i]);

                if (Vector3d.Distance(proj, Position) > 10e-6)
                {
                    axis1 = (proj - Position).Normalized();
                    break;
                }
            }
            var axis2 = Vector3d.Cross(Normal.Normalized(), axis1);

            return new[] { axis1, axis2 };
        }
        public Vector2d ProjectPointUV(Vector3d v)
        {
            var basis = GetBasis();
            return GetUVProjPoint(v, basis[0], basis[1]);
        }
        public Vector2d GetUVProjPoint(Vector3d point, Vector3d axis1, Vector3d axis2)
        {
            var p = GetProjPoint(point) - Position;
            var p1 = Vector3d.Dot(p, axis1);
            var p2 = Vector3d.Dot(p, axis2);
            return new Vector2d(p1, p2);
        }
        public Vector3d GetProjPoint(Vector3d point)
        {
            var v = point - Position;
            var nrm = Normal;
            var dist = Vector3d.Dot(v, nrm);
            var proj = point - dist * nrm;
            return proj;
        }
        public Vector3d ProjPoint(Vector3d point)
        {
            var nrm = Normal.Normalized();
            var v = point - Position;
            var dist = Vector3d.Dot(v, nrm);
            var proj = point - dist * nrm;
            return proj;
        }

        public Line3D Intersect(Plane ps)
        {
            Line3D ret = new Line3D();

            var dir = Vector3d.Cross(ps.Normal, Normal);


            var k1 = ps.GetKoefs();
            var k2 = GetKoefs();
            var a1 = k1[0];
            var b1 = k1[1];
            var c1 = k1[2];
            var d1 = k1[3];

            var a2 = k2[0];
            var b2 = k2[1];
            var c2 = k2[2];
            var d2 = k2[3];



            var res1 = det2(new[] { a1, a2 }, new[] { b1, b2 }, new[] { -d1, -d2 });
            var res2 = det2(new[] { a1, a2 }, new[] { c1, c2 }, new[] { -d1, -d2 });
            var res3 = det2(new[] { b1, b2 }, new[] { c1, c2 }, new[] { -d1, -d2 });

            List<Vector3d> vvv = new List<Vector3d>();

            if (res1 != null)
            {
                Vector3d v1 = new Vector3d((float)res1[0], (float)res1[1], 0);
                vvv.Add(v1);

            }

            if (res2 != null)
            {
                Vector3d v1 = new Vector3d((float)res2[0], 0, (float)res2[1]);
                vvv.Add(v1);
            }
            if (res3 != null)
            {
                Vector3d v1 = new Vector3d(0, (float)res3[0], (float)res3[1]);
                vvv.Add(v1);
            }

            var pnt = vvv.OrderBy(z => z.Length).First();


            var r1 = IsOnPlane(pnt);
            var r2 = IsOnPlane(pnt);

            ret.Start = pnt;
            ret.End = pnt + dir * 100;
            return ret;
        }
        public bool IsOnPlane(Vector3d orig, Vector3d normal, Vector3d check, double tolerance = 10e-6)
        {
            return (Math.Abs(Vector3d.Dot(orig - check, normal)) < tolerance);
        }
        public bool IsOnPlane(Vector3d v)
        {
            return IsOnPlane(Position, Normal, v);
        }
        double[] det2(double[] a, double[] b, double[] c)
        {
            var d = a[0] * b[1] - a[1] * b[0];
            if (d == 0) return null;
            var d1 = c[0] * b[1] - c[1] * b[0];
            var d2 = a[0] * c[1] - a[1] * c[0];
            var x = d1 / d;
            var y = d2 / d;
            return new[] { x, y };
        }




        public double[] GetKoefs()
        {
            double[] ret = new double[4];
            ret[0] = Normal.X;
            ret[1] = Normal.Y;
            ret[2] = Normal.Z;
            ret[3] = -(ret[0] * Position.X + Position.Y * ret[1] + Position.Z * ret[2]);

            return ret;
        }
    }
}
