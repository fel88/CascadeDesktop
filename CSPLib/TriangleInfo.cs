using System.Collections.Generic;
using OpenTK;
using System.Linq;
using System.IO;
using System.Xml.Linq;

namespace CSPLib
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

        internal bool IsSame(TriangleInfo tr)
        {
            float eps = 1e-8f;
            return tr.Vertices.All(uu =>
            {
                return Vertices.Any(zz => (zz.Position - uu.Position).Length < eps);
            });
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
        public Vector3d V0 => Vertices[0].Position;

        public TriangleInfo Multiply(Matrix4d matrix)
        {
            TriangleInfo ret = new TriangleInfo();
            ret.Vertices = new VertexInfo[Vertices.Length];
            for (int i = 0; i < Vertices.Length; i++)
            {
                ret.Vertices[i] = new VertexInfo();
                ret.Vertices[i].Position = Vector3d.Transform(Vertices[i].Position, matrix);
            }

            return ret;
        }

        public Vector3d V1 => Vertices[1].Position;
        public Vector3d V2 => Vertices[2].Position;
        internal Line3D[] GetLines()
        {
            List<Line3D> ret = new List<Line3D>();
            ret.Add(new Line3D() { Start = V0, End = V1 });
            ret.Add(new Line3D() { Start = V1, End = V2 });
            ret.Add(new Line3D() { Start = V2, End = V0 });
            return ret.ToArray();
        }
        public Plane GetPlane()
        {
            var n0 = V2 - V0;
            var n1 = V1 - V0;
            var normal = Vector3d.Cross(n0, n1).Normalized();
            return (new Plane() { Position = V0, Normal = normal });
        }
        public Line3D[] SplitByPlane(Plane pl)
        {

            List<Vector3d> ret = new List<Vector3d>();
            var pl0 = GetPlane();
            var crs = Vector3d.Cross(pl0.Normal, pl.Normal);
            if (crs.Length < 1e-5f) return new Line3D[] { };

            var ln = pl0.Intersect(pl);
            if (ln == null) return null;
            var lns = GetLines();
            List<Vector3d> pp = new List<Vector3d>();
            foreach (var item in lns)
            {
                var l3 = item;
                var inter = GeometryUtils.Intersect3dCrossedLines(ln, l3);
                if (inter != null && l3.IsPointInsideSegment(inter.Value)) pp.Add(inter.Value);
            }

            var pnts = pp.ToArray();
            List<Vector3d> pnts3 = new List<Vector3d>();
            foreach (var item in pnts)
            {
                bool good = true;
                for (int i = 0; i < pnts3.Count; i++)
                {
                    if ((pnts[i] - item).Length < 1e-6)
                    {
                        good = false;
                        break;
                    }
                }
                if (good) pnts3.Add(item);
            }
            pnts = pnts3.ToArray();
            if (pnts.Length == 2)
                return new[] { new Line3D() { Start = pnts[0], End = pnts[1] } };

            return new Line3D[] { };
            //return pnts;

        }

        internal void StoreXml(TextWriter writer)
        {
            writer.WriteLine("<triangle>");
            foreach (var item in Vertices)
            {
                writer.WriteLine($"<vertex pos=\"{item.Position.X};{item.Position.Y};{item.Position.Z}\" normal=\"{item.Normal.X};{item.Normal.Y};{item.Normal.Z}\"/>");
            }
            writer.WriteLine("</triangle>");
        }

        internal void RestoreXml(XElement t)
        {
            int cnt = 0;
            Vertices = new VertexInfo[t.Elements("vertex").Count()];

            foreach (var tt in t.Elements("vertex"))
            {
                Vertices[cnt] = new VertexInfo();
                Vertices[cnt].Position = Helpers.ParseVector(tt.Attribute("pos").Value);
                Vertices[cnt].Normal = Helpers.ParseVector(tt.Attribute("normal").Value);
                cnt++;
            }
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
