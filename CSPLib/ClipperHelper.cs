using System;
using System.Collections.Generic;
using System.Data;
using OpenTK;
using System.Linq;
using ClipperLib;
using OpenTK.Mathematics;

namespace CSPLib
{
    public class ClipperHelper
    {
        public static NFP clipperToSvg(IList<IntPoint> polygon, double clipperScale = 10000000)
        {
            List<Vector2d> ret = new List<Vector2d>();

            for (var i = 0; i < polygon.Count; i++)
            {
                ret.Add(new Vector2d(polygon[i].X / clipperScale, polygon[i].Y / clipperScale));
            }

            return new NFP() { Points = ret.ToArray() };
        }

        public static IntPoint[] ScaleUpPaths(NFP p, double scale = 10000000)
        {
            List<IntPoint> ret = new List<IntPoint>();

            for (int i = 0; i < p.Points.Count(); i++)
            {
                ret.Add(new ClipperLib.IntPoint(
                    (long)Math.Round((decimal)p.Points[i].X * (decimal)scale),
                    (long)Math.Round((decimal)p.Points[i].Y * (decimal)scale)
                ));

            }
            return ret.ToArray();
        }

        public static NFP[] offset(NFP polygon, double offset, JoinType jType = JoinType.jtMiter, double clipperScale = 10000000, double curveTolerance = 0.72, double miterLimit = 4)
        {
            var p = ScaleUpPaths(polygon, clipperScale).ToList();

            var co = new ClipperLib.ClipperOffset(miterLimit, curveTolerance * clipperScale);
            co.AddPath(p.ToList(), jType, ClipperLib.EndType.etClosedPolygon);

            var newpaths = new List<List<ClipperLib.IntPoint>>();
            co.Execute(ref newpaths, offset * clipperScale);

            var result = new List<NFP>();
            for (var i = 0; i < newpaths.Count; i++)
            {
                result.Add(clipperToSvg(newpaths[i]));
            }
            return result.ToArray();
        }
        public static IntPoint[][] nfpToClipperCoordinates(NFP nfp, double clipperScale = 10000000)
        {

            List<IntPoint[]> clipperNfp = new List<IntPoint[]>();

            // children first
            if (nfp.Childrens != null && nfp.Childrens.Count > 0)
            {
                for (var j = 0; j < nfp.Childrens.Count; j++)
                {
                    if (GeometryUtils.polygonArea(nfp.Childrens[j]) < 0)
                    {
                        nfp.Childrens[j].Reverse();
                    }
                    //var childNfp = SvgNest.toClipperCoordinates(nfp.children[j]);
                    var childNfp = ScaleUpPaths(nfp.Childrens[j], clipperScale);
                    clipperNfp.Add(childNfp);
                }
            }

            if (GeometryUtils.polygonArea(nfp) > 0)
            {
                nfp.Reverse();
            }


            //var outerNfp = SvgNest.toClipperCoordinates(nfp);

            // clipper js defines holes based on orientation

            var outerNfp = ScaleUpPaths(nfp, clipperScale);

            //var cleaned = ClipperLib.Clipper.CleanPolygon(outerNfp, 0.00001*config.clipperScale);

            clipperNfp.Add(outerNfp);
            //var area = Math.abs(ClipperLib.Clipper.Area(cleaned));

            return clipperNfp.ToArray();
        }
        public static IntPoint[][] ToClipperCoordinates(NFP[] nfp, double clipperScale = 10000000)
        {
            List<IntPoint[]> clipperNfp = new List<IntPoint[]>();
            for (var i = 0; i < nfp.Count(); i++)
            {
                var clip = nfpToClipperCoordinates(nfp[i], clipperScale);
                clipperNfp.AddRange(clip);
            }

            return clipperNfp.ToArray();
        }
        public static NFP toNestCoordinates(IntPoint[] polygon, double scale)
        {
            var clone = new List<Vector2d>();

            for (var i = 0; i < polygon.Count(); i++)
            {
                clone.Add(new Vector2d(
                     polygon[i].X / scale,
                             polygon[i].Y / scale
                        ));
            }
            return new NFP() { Points = clone.ToArray() };
        }
        public static NFP[] intersection(NFP polygon, NFP polygon1, double offset, JoinType jType = JoinType.jtMiter, double clipperScale = 10000000, double curveTolerance = 0.72, double miterLimit = 4)
        {
            var p = ToClipperCoordinates(new[] { polygon }, clipperScale).ToList();
            var p1 = ToClipperCoordinates(new[] { polygon1 }, clipperScale).ToList();

            Clipper clipper = new Clipper();
            clipper.AddPaths(p.Select(z => z.ToList()).ToList(), PolyType.ptClip, true);
            clipper.AddPaths(p1.Select(z => z.ToList()).ToList(), PolyType.ptSubject, true);

            List<List<IntPoint>> finalNfp = new List<List<IntPoint>>();
            if (clipper.Execute(ClipType.ctIntersection, finalNfp, PolyFillType.pftNonZero, PolyFillType.pftNonZero) && finalNfp != null && finalNfp.Count > 0)
            {
                return finalNfp.Select(z => toNestCoordinates(z.ToArray(), clipperScale)).ToArray();
            }
            return null;
        }
        public static NFP MinkowskiSum(NFP pattern, NFP path, bool useChilds = false, bool takeOnlyBiggestArea = true)
        {
            var ac = ScaleUpPaths(pattern);

            List<List<IntPoint>> solution = null;
            if (useChilds)
            {
                var bc = nfpToClipperCoordinates(path);
                for (var i = 0; i < bc.Length; i++)
                {
                    for (int j = 0; j < bc[i].Length; j++)
                    {
                        bc[i][j].X *= -1;
                        bc[i][j].Y *= -1;
                    }
                }

                solution = ClipperLib.Clipper.MinkowskiSum(new List<IntPoint>(ac), new List<List<IntPoint>>(bc.Select(z => z.ToList())), true);
            }
            else
            {
                var bc = ScaleUpPaths(path);
                for (var i = 0; i < bc.Length; i++)
                {
                    bc[i].X *= -1;
                    bc[i].Y *= -1;
                }
                solution = Clipper.MinkowskiSum(new List<IntPoint>(ac), new List<IntPoint>(bc), true);
            }
            NFP clipperNfp = null;

            double? largestArea = null;
            int largestIndex = -1;

            for (int i = 0; i < solution.Count(); i++)
            {
                var n = toNestCoordinates(solution[i].ToArray(), 10000000);
                var sarea = Math.Abs(GeometryUtils.polygonArea(n));
                if (largestArea == null || largestArea < sarea)
                {
                    clipperNfp = n;
                    largestArea = sarea;
                    largestIndex = i;
                }
            }
            if (!takeOnlyBiggestArea)
            {
                for (int j = 0; j < solution.Count; j++)
                {
                    if (j == largestIndex) continue;
                    var n = toNestCoordinates(solution[j].ToArray(), 10000000);
                    if (clipperNfp.Childrens == null)
                        clipperNfp.Childrens = new List<NFP>();
                    clipperNfp.Childrens.Add(n);
                }
            }

            for (var i = 0; i < clipperNfp.Length; i++)
            {
                clipperNfp.Points[i].X *= -1;
                clipperNfp.Points[i].Y *= -1;
                clipperNfp.Points[i].X += pattern[0].X;
                clipperNfp.Points[i].Y += pattern[0].Y;
            }
            var minx = clipperNfp.Points.Min(z => z.X);
            var miny = clipperNfp.Points.Min(z => z.Y);
            var minx2 = path.Points.Min(z => z.X);
            var miny2 = path.Points.Min(z => z.Y);

            var shiftx = minx2 - minx;
            var shifty = miny2 - miny;
            if (clipperNfp.Childrens != null)
                foreach (var nFP in clipperNfp.Childrens)
                {
                    for (int j = 0; j < nFP.Length; j++)
                    {

                        nFP.Points[j].X *= -1;
                        nFP.Points[j].Y *= -1;
                        nFP.Points[j].X += pattern[0].X;
                        nFP.Points[j].Y += pattern[0].Y;
                    }
                }

            return clipperNfp;
        }

    }
}
