using Cascade.Common;
using CascadeDesktop.Interfaces;
using OpenTK;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace CascadeDesktop
{
    public class Contour
    {
        public Contour Parent;
        public List<Contour> Childrens = new List<Contour>();
        public List<IElement> Elements = new List<IElement>();

        public double Length
        {
            get
            {
                var points = GetPoints();
                float len = 0;
                for (int i = 1; i <= points.Length; i++)
                {
                    var p1 = points[i - 1];
                    var p2 = points[i % points.Length];
                    len += (float)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
                }
                return len;
            }
        }
        public Vector2d[] GetPoints()
        {
            List<Vector2d> ret = new List<Vector2d>();
            foreach (var item in Elements)
            {
                ret.AddRange(item.GetPoints());
            }
            return ret.ToArray();
        }

        internal BlueprintContour ToBlueprintContour()
        {
            BlueprintContour cntr = new BlueprintContour();

            foreach (var element in Elements)
            {
                if (element is ArcElement arc)
                {
                    Arc2d a = new Arc2d()
                    {
                        Radius = arc.Radius,
                        Center = new Vertex2D(arc.Center.X, arc.Center.Y),
                        IsCircle = arc.IsCircle,
                        Start = new Vertex2D(arc.Start.X, arc.Start.Y),
                        End = new Vertex2D(arc.End.X, arc.End.Y),
                        CCW = Parent != null
                    };
                    cntr.Items.Add(a);
                }
                else
                    if (element is PolylineElement pl)
                {
                    BlueprintPolyline poly = new BlueprintPolyline();
                    cntr.Items.Add(poly);
                    foreach (var pp in pl.Points)
                    {
                        poly.Points.Add(new Vertex2D(pp.X, pp.Y));
                    }
                    //sign = Math.Sign(StaticHelpers.signed_area(poly.Points.ToArray()));
                }
                else
                    if (element is LineElement l)
                {
                    Line2D line = new Line2D();
                    cntr.Items.Add(line);

                    line.Start = new Vertex2D(l.Start.X, l.Start.Y);
                    line.End = new Vertex2D(l.End.X, l.End.Y);

                    //sign = Math.Sign(StaticHelpers.signed_area(poly.Points.ToArray()));
                }
            }
            var d = (cntr.Items[0].Start.ToVector2d() - cntr.Items[cntr.Items.Count - 1].End.ToVector2d()).Length;
            if (d > 1e-5)
            {
                cntr.Items.Add(new Line2D()
                {
                    Start = cntr.Items[cntr.Items.Count - 1].End,
                    End = cntr.Items[0].Start
                });
            }
            return cntr;
        }

        internal void Reverse()
        {
            foreach (var item in Elements)
            {
                item.Reverse();
            }
            Elements.Reverse();
        }

        public bool Enable = true;
    }
}
