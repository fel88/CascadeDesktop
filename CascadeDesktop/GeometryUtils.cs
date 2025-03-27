using OpenTK;
using System;
using System.Drawing;
using System.Linq;

namespace CascadeDesktop
{
    public class GeometryUtils
    {
        public static Vector2d GetProjPoint(Vector2d point, Vector2d loc, Vector2d norm)
        {
            var v = point - loc;
            var dist = Vector2d.Dot(v, norm);
            //var proj = point - dist * norm;
            //return proj;
            return dist * norm + loc;

        }
        public static Vector2d point_on_line(Vector2d a, Vector2d b, Vector2d p)
        {
            var ap = p - a;
            var ab = b - a;

            var result = a + Vector2d.Dot(ap, ab) / Vector2d.Dot(ab, ab) * ab;
            return result;
        }
        public static Vector3d point_on_line(Vector3d a, Vector3d b, Vector3d p)
        {
            var ap = p - a;
            var ab = b - a;

            var result = a + Vector3d.Dot(ap, ab) / Vector3d.Dot(ab, ab) * ab;
            return result;
        }

        
        public static bool pnpoly(PointF[] verts, float testx, float testy)
        {
            int nvert = verts.Length;
            int i, j;
            bool c = false;
            for (i = 0, j = nvert - 1; i < nvert; j = i++)
            {
                if (((verts[i].Y > testy) != (verts[j].Y > testy)) &&
                    (testx < (verts[j].X - verts[i].X) * (testy - verts[i].Y) / (verts[j].Y - verts[i].Y) + verts[i].X))
                    c = !c;
            }
            return c;
        }
        public static bool pnpoly(Vector2d[] verts, double testx, double testy)
        {
            int nvert = verts.Length;
            int i, j;
            bool c = false;
            for (i = 0, j = nvert - 1; i < nvert; j = i++)
            {
                if (((verts[i].Y > testy) != (verts[j].Y > testy)) &&
                    (testx < (verts[j].X - verts[i].X) * (testy - verts[i].Y) / (verts[j].Y - verts[i].Y) + verts[i].X))
                    c = !c;
            }
            return c;
        }

        public static Random Random = new Random();
        public static PointF GetRandomInteriorPoint(Vector2d[] polygon)
        {
            var rand = Random;
            var maxx = polygon.Max(z => z.X);
            var minx = polygon.Min(z => z.X);
            var maxy = polygon.Max(z => z.Y);
            var miny = polygon.Min(z => z.Y);
            var tx = rand.Next((int)(minx * 100), (int)(maxx * 100)) / 100f;
            var ty = rand.Next((int)(miny * 100), (int)(maxy * 100)) / 100f;
            PointF test = new PointF(tx, ty);

            while (true)
            {
                if (pnpoly(polygon.ToArray(), test.X, test.Y))
                {
                    break;
                }
                tx = rand.Next((int)(minx * 100), (int)(maxx * 100)) / 100f;
                ty = rand.Next((int)(miny * 100), (int)(maxy * 100)) / 100f;
                test = new PointF(tx, ty);
            }
            return test;
        }
        static double signedArea(Vector2d[] polygon)
        {
            double area = 0.0;

            int j = 1;
            for (int i = 0; i < polygon.Length; i++, j++)
            {
                j = j % polygon.Length;
                area += (polygon[j].X - polygon[i].X) * (polygon[j].Y + polygon[i].Y);
            }

            return area / 2.0;
        }

        public static bool AlmostEqual(double a, double b, double eps = 1e-8)
        {
            return Math.Abs(a - b) <= eps;
        }
        public static double CalculateArea(Vector2d[] Points)
        {
            // Add the first point to the end.
            int num_points = Points.Length;
            Vector2d[] pts = new Vector2d[num_points + 1];
            Points.CopyTo(pts, 0);
            pts[num_points] = Points[0];

            // Get the areas.
            double area = 0;
            for (int i = 0; i < num_points; i++)
            {
                area += (pts[i + 1].X - pts[i].X) * (pts[i + 1].Y + pts[i].Y) / 2;
            }

            // Return the result.
            return Math.Abs(area);
        }

        public static bool IntersectSegments(Vector2d p0, Vector2d p1, Vector2d q0, Vector2d q1, ref Vector2d c0)
        {
            double ux = p1.X - p0.X;
            double uy = p1.Y - p0.Y;
            double vx = q1.X - q0.X;
            double vy = q1.Y - q0.Y;
            double wx = p0.X - q0.X;
            double wy = p0.Y - q0.Y;

            double d = (ux * vy - uy * vx);
            double s = (vx * wy - vy * wx) / d;

            // Intersection point
            c0.X = p0.X + s * ux;
            c0.Y = p0.Y + s * uy;
            if (!IsPointInsideSegment(p0, p1, c0)) return false;
            if (!IsPointInsideSegment(q0, q1, c0)) return false;
            return true;
        }
        public static bool IsPointOnLine(Vector2d start, Vector2d end, Vector2d pnt, double epsilon = 10e-6f)
        {
            float tolerance = 10e-6f;
            var d1 = pnt - start;
            if (d1.Length < tolerance) return true;
            if ((end - start).Length < tolerance) throw new ArgumentException("degenerated line");
            d1 = d1.Normalized();
            var p2 = (end - start).Normalized();
            var crs = Vector3d.Cross(new Vector3d(d1.X, d1.Y, 0), new Vector3d(p2.X, p2.Y, 0));
            return Math.Abs(crs.Length) < epsilon;
        }
        public static bool IsPointInsideSegment(Vector2d start, Vector2d end, Vector2d pnt, double epsilon = 10e-6f)
        {
            if (!IsPointOnLine(start, end, pnt, epsilon)) return false;
            var diff1 = (pnt - start).Length + (pnt - end).Length;
            return Math.Abs(diff1 - (end - start).Length) < epsilon;
        }
        public static bool IsPointInsideBBox(double xmin, double xmax, double ymin, double ymax, Vector2d p0)
        {
            return p0.X >= xmin && p0.X <= xmax && p0.Y >= ymin && p0.Y <= ymax;
        }

        public static bool BBoxIntersection(double xmin, double xmax, double ymin, double ymax, Vector2d p0, Vector2d p1, ref Vector2d c0, ref Vector2d c1)
        {
            // Define the start and end points of the line.
            double x0 = p0.X;
            double y0 = p0.Y;
            double x1 = p1.X;
            double y1 = p1.Y;

            double t0 = 0.0;
            double t1 = 1.0;

            double dx = x1 - x0;
            double dy = y1 - y0;

            double p = 0.0, q = 0.0, r;

            for (int edge = 0; edge < 4; edge++)
            {
                // Traverse through left, right, bottom, top edges.
                if (edge == 0) { p = -dx; q = -(xmin - x0); }
                if (edge == 1) { p = dx; q = (xmax - x0); }
                if (edge == 2) { p = -dy; q = -(ymin - y0); }
                if (edge == 3) { p = dy; q = (ymax - y0); }
                r = q / p;
                if (p == 0 && q < 0) return false; // Don't draw line at all. (parallel line outside)
                if (p < 0)
                {
                    if (r > t1) return false; // Don't draw line at all.
                    else if (r > t0) t0 = r; // Line is clipped!
                }
                else if (p > 0)
                {
                    if (r < t0) return false; // Don't draw line at all.
                    else if (r < t1) t1 = r; // Line is clipped!
                }
            }

            c0.X = x0 + t0 * dx;
            c0.Y = y0 + t0 * dy;
            c1.X = x0 + t1 * dx;
            c1.Y = y0 + t1 * dy;

            return true; // (clipped) line is drawn
        }

    }
}
