using OpenTK;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Linq;

namespace CascadeDesktop
{
    public class NFP
    {
        public SvgPoint[] Points = new SvgPoint[] { };
        public List<NFP> Childrens = new List<NFP>();
        public NFP Parent;

        public void Shift(Vector2d vector)
        {
            for (int i = 0; i < Points.Length; i++)
            {
                Points[i].X += vector.X;
                Points[i].Y += vector.Y;
            }
        }

        public NFP Clone()
        {
            NFP newnfp = new NFP();
            for (var i = 0; i < Length; i++)
            {
                newnfp.AddPoint(new SvgPoint(Points[i].X, Points[i].Y));
            }

            if (Childrens != null && Childrens.Count > 0)
            {
                newnfp.Childrens = new List<NFP>();
                for (int i = 0; i < Childrens.Count; i++)
                {
                    var child = Childrens[i];
                    NFP newchild = new NFP();
                    for (var j = 0; j < child.Length; j++)
                    {
                        newchild.AddPoint(new SvgPoint(child[j].X, child[j].Y));
                    }
                    newnfp.Childrens.Add(newchild);
                }
            }
            return newnfp;
        }
        public double SignedArea()
        {
            return StaticHelpers.signed_area(Points);
        }

        public int Length
        {
            get
            {
                return Points.Length;
            }
        }
        public void push(SvgPoint svgPoint)
        {
            List<SvgPoint> points = new List<SvgPoint>();
            if (Points == null)
            {
                Points = new SvgPoint[] { };
            }
            points.AddRange(Points);
            points.Add(svgPoint);
            Points = points.ToArray();

        }
        public NFP slice(int v)
        {
            var ret = new NFP();
            List<SvgPoint> pp = new List<SvgPoint>();
            for (int i = v; i < Length; i++)
            {
                pp.Add(new SvgPoint(this[i].X, this[i].Y));

            }
            ret.Points = pp.ToArray();
            return ret;
        }
        public void reverse()
        {
            Points = Points.Reverse().ToArray();
        }
        public SvgPoint this[int ind]
        {
            get
            {
                return Points[ind];
            }
        }

        public void AddPoint(SvgPoint point)
        {
            var list = Points.ToList();
            list.Add(point);
            Points = list.ToArray();
        }
        public void Translate(Vector2d c)
        {
            
            for (int i = 0; i < Points.Length; i++)
            {
                Points[i].X -= c.X;
                Points[i].Y -= c.Y;
            }
            foreach (var item in Childrens)
            {
                //item.Translate(-c);
            }
        }
    }
}
