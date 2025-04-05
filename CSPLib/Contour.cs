using System;
using System.Collections.Generic;
using System.Data;
using OpenTK;
using System.Linq;

namespace CSPLib
{
    public class Contour
    {
        //public BRepWire Wire;
        public List<Segment> Elements = new List<Segment>();

        public Vector2d Start
        {
            get
            {
                return Elements[0].Start;
            }
        }

        public Vector2d End
        {
            get
            {
                return Elements.Last().End;
            }
        }

        public virtual bool Contains(Vector2d point)
        {
            return Math.Abs(((point - Start).Length + (point - End).Length) - (End - Start).Length) < float.Epsilon;
        }

        public static double DistByRing(double v1, double v2, double diap)
        {
            var dist1 = Math.Abs(v1 - v2);
            var dist2 = Math.Abs(v1 - (v2 - diap));
            var dist3 = Math.Abs(v2 - (v1 - diap));
            return new[] { dist1, dist2, dist3 }.Min();
        }
        public static double DistByXRing(Vector2d v1, Vector2d v2, double xdiap)
        {
            return new Vector2d(DistByRing(v1.X, v2.X, xdiap), v2.Y - v1.Y).Length;
        }
        public Contour ConnectNext(Contour[] cntr, bool useEnd = true, bool useStart = true)
        {
            if (Elements.Count == 0)
            {
                //Wire = cntr[0].Wire;
                Elements.AddRange(cntr[0].Elements);
                return cntr[0];
            }

            var start = new Vector2d(Elements[0].Start.X, Elements[0].Start.Y);
            var end = new Vector2d(Elements.Last().End.X, Elements.Last().End.Y);
            float tol = 10e-6f;
            double mindist = double.MaxValue;
            double rmindist = double.MaxValue;
            Contour minsegm = null;
            bool reverse = false;
            bool insert = false;
            Vector2d connectPoint1 = Vector2d.One;
            Vector2d connectPoint2 = Vector2d.One;

            foreach (var item in cntr)
            {
                if (useEnd)
                {
                    var v1 = new Vector2d(DistByRing(end.X, item.Start.X, Math.PI * 2), end.Y - item.Start.Y);
                    var dist1 = v1.Length;
                    var rdist1 = Math.Abs((end - item.Start).Length);
                    if (dist1 < mindist || rdist1 < rmindist)
                    {
                        connectPoint1 = end;
                        connectPoint2 = item.Start;
                        mindist = dist1;
                        rmindist = rdist1;
                        minsegm = item;
                        reverse = false;
                        insert = false;

                    }

                    var v2 = new Vector2d(DistByRing(end.X, item.End.X, Math.PI * 2), end.Y - item.End.Y);
                    var dist2 = v2.Length;
                    var rdist2 = Math.Abs((end - item.End).Length);
                    if (dist2 < mindist || rdist2 < rmindist)
                    {
                        connectPoint1 = end;
                        connectPoint2 = item.End;
                        mindist = dist2;
                        rmindist = rdist2;
                        minsegm = item;
                        reverse = true;
                        insert = false;

                    }
                }
                if (useStart)
                {

                    var v3 = new Vector2d(DistByRing(start.X, item.Start.X, Math.PI * 2), start.Y - item.Start.Y);
                    var dist3 = v3.Length;
                    var rdist3 = Math.Abs((start - item.Start).Length);
                    if (dist3 < mindist || rdist3 < rmindist)
                    {
                        connectPoint1 = start;
                        connectPoint2 = item.Start;
                        rmindist = rdist3;
                        mindist = dist3;
                        minsegm = item;
                        reverse = true;
                        insert = true;
                    }

                    var v4 = new Vector2d(DistByRing(start.X, item.End.X, Math.PI * 2), start.Y - item.End.Y);
                    var dist4 = v4.Length;
                    var rdist4 = Math.Abs((start - item.End).Length);
                    if (dist4 < mindist || rdist4 < rmindist)
                    {
                        connectPoint1 = start;
                        connectPoint2 = item.End;
                        mindist = dist4;
                        rmindist = rdist4;
                        minsegm = item;
                        reverse = false;
                        insert = true;
                    }
                }
            }

            var diffX = Math.Abs(connectPoint2.X - connectPoint1.X);
            double epsilon = 1e-5;
            if (minsegm != null && mindist < epsilon)
            {
                var item = minsegm;
                if (reverse)
                {
                    item = new Contour();
                    foreach (var ritem in minsegm.Elements.ToArray().Reverse())
                    {
                        item.Elements.Add(new Segment() { Start = ritem.End, End = ritem.Start }); ;
                    }
                }
                if (Math.Abs(diffX - Math.PI * 2) < epsilon)
                {

                    var temp = new Contour();
                    foreach (var ritem in item.Elements.ToArray())
                    {
                        var shift = ((connectPoint1.X < connectPoint2.X) ? -1 : 1) * new Vector2d(Math.PI * 2, 0);
                        temp.Elements.Add(new Segment() { Start = ritem.Start + shift, End = ritem.End + shift });
                    }
                    item = temp;
                }
                if (insert)
                {

                    Elements.InsertRange(0, item.Elements);
                }
                else
                {


                    Elements.AddRange(item.Elements);
                }
                return minsegm;
            }

            return null;
        }


        public Segment ConnectNext(Segment[] segments)
        {
            if (Elements.Count == 0)
            {
                Elements.Add(segments[0]);
                return segments[0];
            }
            var start = new Vector2d(Elements[0].Start.X, Elements[0].Start.Y);
            var end = new Vector2d(Elements.Last().End.X, Elements.Last().End.Y);
            float tol = 10e-6f;
            double mindist = double.MaxValue;
            Segment minsegm = null;
            bool reverse = false;

            bool insert = false;
            foreach (var item in segments)
            {
                var dist1 = Math.Abs((end - item.Start).Length);
                if (dist1 < mindist)
                {
                    mindist = dist1;
                    minsegm = item;
                    reverse = false;
                    insert = false;

                }
                var dist2 = Math.Abs((end - item.End).Length);

                if (dist2 < mindist)
                {
                    mindist = dist2;
                    minsegm = item;
                    reverse = true;
                    insert = false;
                }

                var dist3 = Math.Abs((start - item.Start).Length);
                if (dist3 < mindist)
                {
                    mindist = dist3;
                    minsegm = item;
                    reverse = true;
                    insert = true;
                }
                var dist4 = Math.Abs((start - item.End).Length);

                if (dist4 < mindist)
                {
                    mindist = dist4;
                    minsegm = item;
                    reverse = false;
                    insert = true;

                }

            }
            double epsilon = 1e-5;
            if (minsegm != null && mindist < epsilon)
            {
                if (insert)
                {
                    var item = minsegm;
                    if (reverse)
                    {
                        item = new Segment() { End = minsegm.Start, Start = minsegm.End };
                    }
                    Elements.Insert(0, item);
                }
                else
                {
                    var item = minsegm;
                    if (reverse)
                    {
                        item = new Segment() { End = minsegm.Start, Start = minsegm.End };
                    }
                    Elements.Add(item);
                }

                return minsegm;
            }

            return null;
        }

        public double Area()
        {
            var ar = GeometryUtils.CalculateArea(Elements.Select(z => z.End).ToArray());
            return Math.Abs(ar);
        }
        internal void Reduce(double eps = 1e-8)
        {
            Elements.RemoveAll(x => x.Length() < eps);
        }
    }
}
