using OpenTK;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CascadeDesktop
{
    public static class Intersection
    {
        public static IntersectInfo[] AllRayIntersect(Model[] array, MouseRay mr, Matrix4d? mtr = null)
        {
            List<IntersectInfo> ret = new List<IntersectInfo>();
            foreach (var bindMesh in array)
            {
                var dd = CheckIntersect(mr, bindMesh.Polygons.ToArray(), mtr);

                if (dd != null)
                {
                    dd.Parent = bindMesh;
                    if (bindMesh.Tag != null)
                    {
                        dd.Parent = bindMesh.Tag;
                    }
                    var dist = dd.Distance;
                    dd.Model = bindMesh;
                    ret.Add(dd);
                }
            }

            return ret.ToArray();
        }

        public static IntersectInfo RayIntersect(Model[] array, MouseRay mr)
        {
            IntersectInfo mininfo = null;
            double mdist = double.MaxValue;

            foreach (var bindMesh in array)
            {

                var dd = CheckIntersect(mr, bindMesh.Polygons.ToArray());

                if (dd != null)
                {
                    dd.Parent = bindMesh;
                    if (bindMesh.Tag != null)
                    {
                        dd.Parent = bindMesh.Tag;
                    }
                    var dist = dd.Distance;
                    if (dist < mdist)
                    {
                        mininfo = dd;
                        mdist = dist;
                    }
                }
            }

            if (mdist > 10e4)
                return null;

            return mininfo;
        }

        public static IntersectInfo CheckIntersect(MouseRay ray, TriangleInfo[] poly, Matrix4d? mtr = null)
        {
            var a = System.Math.Abs(2);
            List<IntersectInfo> intersects = new List<IntersectInfo>();
            if (poly != null)
            {
                foreach (var polygon in poly)
                {

                    var vv = polygon.Vertices.Select(z => z.Position).ToArray();
                    if (mtr != null)
                    {
                        for (int i = 0; i < vv.Length; i++)
                        {
                            vv[i] = Vector3d.Transform(vv[i], mtr.Value);
                        }
                    }
                    var res = CheckIntersect(ray, vv);
                    if (res.HasValue)
                    {
                        intersects.Add(new IntersectInfo()
                        {
                            Distance = (res.Value - ray.Start).Length,
                            Target = polygon,
                            Point = res.Value
                        });
                    }
                }
            }
            if (intersects.Any())
            {
                return intersects.OrderBy(z => z.Distance).First();
            }
            return null;
        }

        public static Vector3d? InstersectPlaneWithRay(Plane plane, MouseRay ray)
        {

            var l = ray.End - ray.Start;
            l.Normalize();

            //check point exists 
            var n = plane.Normal;
            var d = l.X * n.X + l.Y * n.Y + n.Z * l.Z;
            var r0n = ray.Start.X * n.X + ray.Start.Y * n.Y + ray.Start.Z * n.Z;
            if (System.Math.Abs(d) > 1e-4)
            {
                var t0 = -((r0n) + plane.W) / d;
                if (t0 >= 0)
                {
                    return ray.Start + l * (float)t0;
                }
            }
            return null;
        }


        public static OpenTK.Vector3d? CheckIntersect(MouseRay ray, OpenTK.Vector3d[] triangle)
        {
            var dir = ray.End - ray.Start;
            dir.Normalize();

            var a = triangle[0];
            var b = triangle[1];
            var c = triangle[2];
            var plane = Plane.FromPoints(new Vector3d(a.X, a.Y, a.Z),
                new Vector3d(b.X, b.Y, b.Z),
                new Vector3d(c.X, c.Y, c.Z));

            plane.W = -plane.W;
            var s = InstersectPlaneWithRay(plane, ray);

            if (s != null)
            {
                var ss = s.Value;
                var v1 = triangle[1] - triangle[0];
                var v2 = triangle[2] - triangle[1];
                var v3 = triangle[0] - triangle[2];
                var crs1 = Vector3d.Cross(ss - triangle[0], v1);
                var crs2 = Vector3d.Cross(ss - triangle[1], v2);
                var crs3 = Vector3d.Cross(ss - triangle[2], v3);
                var up = Vector3d.Cross(v1, triangle[2] - triangle[0]);

                var dot1 = Vector3d.Dot(crs1, up);
                var dot2 = Vector3d.Dot(crs2, up);
                var dot3 = Vector3d.Dot(crs3, up);

                if (System.Math.Sign(dot1) == System.Math.Sign(dot2) && System.Math.Sign(dot2) == System.Math.Sign(dot3) /*&& Math.Sign(dot1) == 1*/)
                {
                    return ss;
                }
            }
            return null;
        }
    }
}
