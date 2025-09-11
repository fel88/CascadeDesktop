using IxMilia.Dxf.Entities;
using IxMilia.Dxf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using OpenTK;
using CascadeDesktop.Interfaces;
using OpenTK.Mathematics;

namespace CascadeDesktop
{
    public class DxfParser
    {
        public static LocalContour[] ConnectElements(IElement[] elems)
        {
            List<LocalContour> ret = new List<LocalContour>();

            List<Vector2d> pp = new List<Vector2d>();
            List<IElement> last = new List<IElement>();
            last.AddRange(elems);

            while (last.Any())
            {
                if (pp.Count == 0)
                {
                    pp.Add(last.First().Start);
                    pp.Add(last.First().End);
                    last.RemoveAt(0);
                }
                else
                {
                    var ll = pp.Last();
                    var f1 = last.OrderBy(z => Math.Min((z.Start - ll).Length, (z.End - ll).Length)).First();

                    var dist = Math.Min((f1.Start - ll).Length, (f1.End - ll).Length);
                    if (dist > ClosingThreshold)
                    {
                        ret.Add(new LocalContour() { Points = pp.ToList() });
                        pp.Clear();
                        continue;
                    }
                    last.Remove(f1);
                    if ((f1.Start - ll).Length < (f1.End - ll).Length)
                    {
                        pp.Add(f1.End);
                    }
                    else
                    {
                        pp.Add(f1.Start);
                    }
                }
            }
            if (pp.Any())
            {
                ret.Add(new LocalContour() { Points = pp.ToList() });
            }
            return ret.ToArray();
        }
        public static Contour[] ElementsToContours(IElement[] elems)
        {
            List<Contour> ret = new List<Contour>();

            List<IElement> pp = new List<IElement>();
            List<IElement> last = new List<IElement>();
            last.AddRange(elems);

            while (last.Any())
            {
                if (pp.Count == 0)
                {
                    pp.Add(last.First());
                    last.RemoveAt(0);
                }
                else
                {
                    var ll = pp.Last().End;
                    var f1 = last.OrderBy(z => Math.Min((z.Start - ll).Length, (z.End - ll).Length)).First();

                    var dist = Math.Min((f1.Start - ll).Length, (f1.End - ll).Length);
                    if (dist > ClosingThreshold)
                    {
                        ret.Add(new Contour() { Elements = pp.ToList() });
                        pp.Clear();
                        continue;
                    }
                    last.Remove(f1);
                    if ((f1.Start - ll).Length < (f1.End - ll).Length)
                    {
                        var clone = f1.Clone();
                        clone.Reverse();
                        pp.Add(clone);
                    }
                    else
                    {
                        pp.Add(f1);
                    }
                }
            }
            if (pp.Any())
            {
                ret.Add(new Contour() { Elements = pp.ToList() });
            }
            return ret.ToArray();
        }

        public static IElement[] LoadDxf(string path)
        {
            FileInfo fi = new FileInfo(path);
            DxfFile dxffile = DxfFile.Load(fi.FullName);

            IEnumerable<DxfEntity> entities = dxffile.Entities.ToArray();

            List<IElement> elems = new List<IElement>();

            foreach (DxfEntity ent in entities)
            {
                switch (ent.EntityType)
                {
                    case DxfEntityType.LwPolyline:
                        {
                            DxfLwPolyline poly = (DxfLwPolyline)ent;
                            if (poly.Vertices.Count() < 2)
                            {
                                continue;
                            }
                            PolylineElement pl = new PolylineElement();
                            List<Vector2d> pnts = new List<Vector2d>();

                            for (int i = 0; i < poly.Vertices.Count; i++)
                            {
                                var vert = poly.Vertices[i];
                                pnts.Add(new Vector2d(vert.X, vert.Y));
                            }

                            elems.Add(new PolylineElement() { Points = pnts.ToArray() });
                        }
                        break;
                    case DxfEntityType.Arc:
                        {
                            DxfArc arc = (DxfArc)ent;
                            List<Vector2d> pp = new List<Vector2d>();

                            if (arc.StartAngle > arc.EndAngle)
                            {
                                arc.StartAngle -= 360;
                            }

                            for (double i = arc.StartAngle; i < arc.EndAngle; i += 15)
                            {
                                var tt = arc.GetPointFromAngle(i);
                                pp.Add(new Vector2d(tt.X, tt.Y));
                            }
                            var t = arc.GetPointFromAngle(arc.EndAngle);
                            pp.Add(new Vector2d(t.X, t.Y));
                            for (int j = 1; j < pp.Count; j++)
                            {
                                var p1 = pp[j - 1];
                                var p2 = pp[j];
                                elems.Add(new LineElement() { Start = p1, End = p2 });
                            }
                            //elems.Add(new arc)
                        }
                        break;
                    case DxfEntityType.Circle:
                        {
                            DxfCircle cr = (DxfCircle)ent;                            
                            //LocalContour cc = new LocalContour();

                            elems.Add(new ArcElement(true)
                            {
                                Radius = cr.Radius,
                                Center = cr.Center.ToVector2d(),
                                Start = cr.Center.ToVector2d() - new Vector2d(cr.Radius, 0),
                                End = cr.Center.ToVector2d() - new Vector2d(cr.Radius, 0)
                            });
                            //break;
                            //for (int i = 0; i <= 360; i += 15)
                            //{
                            //    var ang = i * Math.PI / 180f;
                            //    var xx = cr.Center.X + cr.Radius * Math.Cos(ang);
                            //    var yy = cr.Center.Y + cr.Radius * Math.Sin(ang);
                            //    cc.Points.Add(new Vector2d(xx, yy));
                            //}
                            //for (int i = 1; i < cc.Points.Count; i++)
                            //{
                            //    var p1 = cc.Points[i - 1];
                            //    var p2 = cc.Points[i];
                            //    elems.Add(new LineElement() { Start = p1, End = p2 });
                            //}
                        }
                        break;
                    case DxfEntityType.Line:
                        {
                            DxfLine poly = (DxfLine)ent;
                            elems.Add(new LineElement()
                            {
                                Start = new Vector2d(poly.P1.X, poly.P1.Y),
                                End = new Vector2d(poly.P2.X, poly.P2.Y)
                            });
                            break;
                        }

                    case DxfEntityType.Polyline:
                        {
                            DxfPolyline poly = (DxfPolyline)ent;
                            if (poly.Vertices.Count() < 2)
                                continue;

                            PolylineElement pl = new PolylineElement();
                            List<Vector2d> pnts = new List<Vector2d>();

                            for (int i = 0; i < poly.Vertices.Count; i++)
                            {
                                DxfVertex vert = poly.Vertices[i];
                                pnts.Add(new Vector2d(vert.Location.X, vert.Location.Y));
                            }

                            elems.Add(new PolylineElement() { Points = pnts.ToArray() });
                            break;
                        }
                    default:
                        throw new ArgumentException("unsupported entity type: " + ent);

                };
            }

            return elems.ToArray();
        }

        public static double RemoveThreshold = 10e-5;
        public static double ClosingThreshold = 10e-2;
    }
}
