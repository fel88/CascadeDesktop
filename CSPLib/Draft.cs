using System;
using System.Collections.Generic;
using System.Data;
using OpenTK;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using System.Diagnostics;
using TriangleNet.Geometry;
using System.Reflection;
using DxfPad;
using CSPLib.Interfaces;

namespace CSPLib
{
    public class Draft : AbstractDrawable
    {
        public List<Vector3d> Points3D = new List<Vector3d>();

        public Draft()
        {
            Plane = new PlaneHelper() { Normal = Vector3d.UnitZ, Position = Vector3d.Zero };
            _inited = true;
        }

        public Draft(XElement el)
        {
            Restore(el);
            _inited = true;
        }
        public void Clear()
        {
            Elements.Clear();
            Helpers.Clear();
            Constraints.Clear();
        }
        public void Restore(XElement el)
        {
            Clear();
            Plane = new PlaneHelper(el.Element("plane"));
            Name = el.Attribute("name").Value;
            foreach (var item2 in el.Elements())
            {
                if (item2.Name == "point")
                {
                    DraftPoint dl = new DraftPoint(item2, this);
                    AddElement(dl);
                }
                if (item2.Name == "line")
                {
                    DraftLine dl = new DraftLine(item2, this);
                    AddElement(dl);
                }
                if (item2.Name == "ellipse")
                {
                    DraftEllipse dl = new DraftEllipse(item2, this);
                    AddElement(dl);
                }
            }

            var constr = el.Element("constraints");
            if (constr != null)
            {
                Type[] types = new[] {
                      typeof(LinearConstraint),
                      typeof(VerticalConstraint),
                     typeof(HorizontalConstraint),
                     typeof(EqualsConstraint),
                     typeof(PointPositionConstraint),
                     typeof(TopologyConstraint),
                                  };
                foreach (var item in constr.Elements())
                {
                    var fr = types.FirstOrDefault(z => (z.GetCustomAttributes(typeof(XmlNameAttribute), true).First() as XmlNameAttribute).XmlName == item.Name);
                    if (fr == null) continue;
                    var v = Activator.CreateInstance(fr, new object[] { item, this }) as DraftConstraint;
                    //if (v is IXmlStorable xx)
                    //{
                    //    xx.RestoreXml(item);
                    //}
                    AddConstraint(v);
                }
            }

            var helpers = el.Element("helpers");
            if (helpers != null)
            {
                var types = Assembly.GetEntryAssembly().GetTypes().Where(z => z.GetCustomAttribute(typeof(XmlNameAttribute), true) != null).ToArray();
                foreach (var item in helpers.Elements())
                {
                    var fr = types.FirstOrDefault(z => (z.GetCustomAttributes(typeof(XmlNameAttribute), true).First() as XmlNameAttribute).XmlName == item.Name);
                    if (fr == null) continue;
                    var v = Activator.CreateInstance(fr, new object[] { item, this }) as IDraftHelper;
                    var ch = ConstraintHelpers.ToArray();
                    if (v is IDraftConstraintHelper dch)
                    {
                        if (!ch.Any(z => z.Constraint.Id == dch.Constraint.Id))
                        {
                            AddHelper(v);
                        }
                        else
                        {
                            //todo: warning!
                            //
                            if (MessageReporter != null)
                            {
                                MessageReporter.Warning($"duplicate constraint helper detected. skipped: {dch.Constraint.Id}");
                            }
                        }
                    }
                    else
                        AddHelper(v);
                }
            }

            EndEdit();
        }

        public DraftElement[][] GetWires()
        {
            var remains = DraftLines.Where(z => !z.Dummy).ToList();
            List<List<DraftLine>> ret1 = new List<List<DraftLine>>();

            while (remains.Any())
            {
                List<DraftLine> added = new List<DraftLine>();
                foreach (var rem in remains)
                {
                    bool good = false;

                    foreach (var item in ret1)
                    {
                        var arr1 = item.SelectMany(z => new[] { z.V0, z.V1 }).ToArray();
                        if (arr1.Contains(rem.V0) || arr1.Contains(rem.V1))
                        {
                            item.Add(rem);
                            good = true;
                            break;
                        }
                    }
                    if (good)
                    {
                        added.Add(rem);
                    }
                }
                if (added.Count == 0)
                {
                    ret1.Add(new List<DraftLine>());
                    ret1.Last().Add(remains[0]);
                    added.Add(remains[0]);
                }
                foreach (var item in added)
                {
                    remains.Remove(item);
                }
            }

            List<List<DraftElement>> ret = new List<List<DraftElement>>();
            var remains1 = DraftEllipses.Where(z => !z.Dummy).ToList();

            ret.AddRange(ret1.Select(z => z.Select(u => u as DraftElement).ToList()));
            foreach (var item in remains1)
            {
                ret.Add(new List<DraftElement>()
                {
                    item
                });
            }

            return ret.Select(z => z.ToArray()).ToArray();
        }

        public override void Store(TextWriter writer)
        {
            writer.WriteLine($"<draft name=\"{Name}\">");
            Plane.Store(writer);
            foreach (var item in DraftPoints)
            {
                item.Store(writer);
            }
            foreach (var item in Elements.Except(DraftPoints))
            {
                item.Store(writer);
            }
            writer.WriteLine("<constraints>");
            foreach (var item in Constraints)
            {
                item.Store(writer);
            }
            writer.WriteLine("</constraints>");
            writer.WriteLine("<helpers>");
            foreach (var item in Helpers)
            {
                item.Store(writer);
            }
            writer.WriteLine("</helpers>");
            writer.WriteLine("</draft>");
        }

        bool _inited = false;

        bool expandGraphSolver(ConstraintSolverContext ctx)
        {
            var start = Stopwatch.StartNew();
            int cntr = 0;
            while (true)
            {
                var unsat = Constraints.Where(z => !z.IsSatisfied()).ToArray();
                if (unsat.Length == 0) return true;
                cntr++;
                if (start.Elapsed.TotalSeconds > 5 /*&& !Debugger.IsAttached*/)
                {
                    //throw new LiteCadException("time exceed");
                    return false;
                }
                var cnctd = unsat.Where(z => ctx.FreezedPoints.Any(uu => z.ContainsElement(uu))).ToArray();
                var top = Constraints.OfType<TopologyConstraint>().First();
                var vv = cnctd.Where(zz => zz is VerticalConstraint || zz is HorizontalConstraint).ToArray();
                List<DraftPoint> toFreeze = new List<DraftPoint>();
                foreach (var item in vv)
                {
                    item.RandomUpdate(ctx);
                    DraftLine line = null;
                    if (item is VerticalConstraint vv2)
                    {
                        line = vv2.Line;
                    }
                    if (item is HorizontalConstraint hh)
                    {
                        line = hh.Line;
                    }
                    toFreeze.Add(line.V0);
                    toFreeze.Add(line.V1);
                }
                var size = cnctd.OfType<LinearConstraint>().ToArray();
                foreach (var ss in size)
                {
                    ss.RandomUpdate(ctx);
                }
                ctx.FreezedPoints.AddRange(toFreeze);
            }
        }


        public CSPTask ExtractCSPTask()
        {
            CSPTask task = new CSPTask();
            int vcntr = 0;
            foreach (var item in DraftPoints)
            {
                task.Vars.Add(new CSPVar() { Name = "x" + vcntr });
                task.Vars.Add(new CSPVar() { Name = "y" + vcntr });
                vcntr++;
            }
            var ppc2 = Constraints.OfType<PointPositionConstraint>().ToArray();
            foreach (var item in ppc2)
            {
                var vind = DraftPoints.ToList().IndexOf(item.Point);
                task.Constrs.Add(new CSPConstrEqualVarValue(task.Vars.First(zz => zz.Name == "x" + vind), item.Location.X));
                task.Constrs.Add(new CSPConstrEqualVarValue(task.Vars.First(zz => zz.Name == "y" + vind), item.Location.Y));
            }
            var vert = Constraints.OfType<VerticalConstraint>().ToArray();
            foreach (var item in vert)
            {
                var vind0 = DraftPoints.ToList().IndexOf(item.Line.V0);
                var vind1 = DraftPoints.ToList().IndexOf(item.Line.V1);
                task.Constrs.Add(new CSPConstrEqualTwoVars(task.Vars.First(zz => zz.Name == "x" + vind0), task.Vars.First(uu => uu.Name == "x" + vind1)));
            }
            var horiz = Constraints.OfType<HorizontalConstraint>().ToArray();
            foreach (var item in horiz)
            {
                var vind0 = DraftPoints.ToList().IndexOf(item.Line.V0);
                var vind1 = DraftPoints.ToList().IndexOf(item.Line.V1);
                task.Constrs.Add(new CSPConstrEqualTwoVars(task.Vars.First(zz => zz.Name == "y" + vind0), task.Vars.First(uu => uu.Name == "y" + vind1)));
            }
            var linears = Constraints.OfType<LinearConstraint>().ToList();
            var eqls = Constraints.OfType<EqualsConstraint>().ToArray();

            if (Constraints.Any(z => z is TopologyConstraint))
            {
                var topo = Constraints.First(z => z is TopologyConstraint) as TopologyConstraint;
                List<EqualsConstraint> notFoundEqs = new List<EqualsConstraint>();
                foreach (var item in eqls)
                {
                    if (linears.Any(z => z.IsLineConstraint(item.SourceLine)))
                    {
                        var frr = linears.First(z => z.IsLineConstraint(item.SourceLine));
                        linears.Add(new LinearConstraint(item.TargetLine.V0, item.TargetLine.V1, frr.Length, this));
                    }
                    else
                    {
                        notFoundEqs.Add(item);
                    }
                }

                foreach (var item in notFoundEqs)
                {
                    //add something like this: x3-x2=x1-x0
                    var vind0 = DraftPoints.ToList().IndexOf(item.TargetLine.V0);
                    var vind1 = DraftPoints.ToList().IndexOf(item.TargetLine.V1);
                    var tind0 = DraftPoints.ToList().IndexOf(item.SourceLine.V0);
                    var tind1 = DraftPoints.ToList().IndexOf(item.SourceLine.V1);

                    var t1 = topo.Lines.First(z => z.Line == item.TargetLine);
                    var t2 = topo.Lines.First(z => z.Line == item.SourceLine);

                    if (vert.Any(z => z.Line == item.TargetLine) && vert.Any(z => z.Line == item.SourceLine))
                    {
                        var vx1 = task.Vars.First(zz => zz.Name == "y" + vind0);
                        var vx2 = task.Vars.First(zz => zz.Name == "y" + vind1);
                        var vx3 = task.Vars.First(zz => zz.Name == "y" + tind0);
                        var vx4 = task.Vars.First(zz => zz.Name == "y" + tind1);
                        if (Math.Abs(t1.Dir.Y - t2.Dir.Y) > 1)
                        {
                            task.Constrs.Add(new CSPConstrEqualExpression()
                            {
                                Vars = new[] { vx1, vx2, vx3, vx4 },
                                Expression = vx1.Name + "-" + vx2.Name + "=" + vx4.Name + "-" + vx3.Name
                            });
                        }
                        else
                        {
                            task.Constrs.Add(new CSPConstrEqualExpression()
                            {
                                Vars = new[] { vx1, vx2, vx3, vx4 },
                                Expression = vx2.Name + "-" + vx1.Name + "=" + vx4.Name + "-" + vx3.Name
                            });
                        }

                    }
                    if (horiz.Any(z => z.Line == item.TargetLine) && horiz.Any(z => z.Line == item.SourceLine))
                    {
                        var vx1 = task.Vars.First(zz => zz.Name == "x" + vind0);
                        var vx2 = task.Vars.First(zz => zz.Name == "x" + vind1);
                        var vx3 = task.Vars.First(zz => zz.Name == "x" + tind0);
                        var vx4 = task.Vars.First(zz => zz.Name == "x" + tind1);
                        if (Math.Abs(t1.Dir.X - t2.Dir.X) > 1)
                        {
                            task.Constrs.Add(new CSPConstrEqualExpression()
                            {
                                Vars = new[] { vx1, vx2, vx3, vx4 },
                                Expression = vx1.Name + "-" + vx2.Name + "=" + vx4.Name + "-" + vx3.Name
                            });
                        }
                        else
                        {
                            task.Constrs.Add(new CSPConstrEqualExpression()
                            {
                                Vars = new[] { vx1, vx2, vx3, vx4 },
                                Expression = vx2.Name + "-" + vx1.Name + "=" + vx4.Name + "-" + vx3.Name
                            });
                        }

                    }
                }

                foreach (var item in linears)
                {
                    if (!(item.Element1 is DraftPoint dp0 && item.Element2 is DraftPoint dp1)) continue;
                    var vind0 = DraftPoints.ToList().IndexOf(dp0);
                    var vind1 = DraftPoints.ToList().IndexOf(dp1);

                    var fr = topo.Lines.FirstOrDefault(z => new[] { z.Line.V0, z.Line.V1 }.Intersect(new[] { dp0, dp1 }).Count() == 2);
                    if (fr == null)
                    {
                        //add dist equation
                        continue;
                    }
                    var dd = fr.Dir;
                    vind0 = DraftPoints.ToList().IndexOf(fr.Line.V0);
                    vind1 = DraftPoints.ToList().IndexOf(fr.Line.V1);
                    var vx1 = task.Vars.First(zz => zz.Name == "x" + vind0);
                    var vx2 = task.Vars.First(zz => zz.Name == "x" + vind1);
                    var vy1 = task.Vars.First(zz => zz.Name == "y" + vind0);
                    var vy2 = task.Vars.First(zz => zz.Name == "y" + vind1);
                    if (Math.Abs(dd.X) == 1)
                    {
                        task.Constrs.Add(new CSPConstrEqualExpression() { Vars = new[] { vx1, vx2 }, Expression = vx2.Name + "=" + vx1.Name + (dd.X > 0 ? "+" : "-") + item.Length });
                    }
                    if (Math.Abs(dd.Y) == 1)
                    {
                        task.Constrs.Add(new CSPConstrEqualExpression() { Vars = new[] { vy1, vy2 }, Expression = vy2.Name + "=" + vy1.Name + (dd.Y > 0 ? "+" : "-") + item.Length });
                    }
                }
            }
            return task;
        }

        public bool Solve()
        {
            var task = ExtractCSPTask();
            CSPVarContext ctx = new CSPVarContext() { Task = task };
            bool res;
            int vcntr = 0;
            if (res = ctx.Solve())
            {
                vcntr = 0;
                foreach (var item in DraftPoints)
                {
                    var xv = task.Vars.First(zz => zz.Name == "x" + vcntr);
                    var yv = task.Vars.First(zz => zz.Name == "y" + vcntr);
                    item.SetLocation(ctx.Infos.First(z => z.Var == xv).Value, ctx.Infos.First(z => z.Var == yv).Value);
                    vcntr++;
                }
            }
            return res;
        }
        public void RecalcConstraints()
        {
            if (!_inited) return;
            return;
            //ConstraintSolverContext ccc = new ConstraintSolverContext();
            ///*var ppc = Constraints.OfType<PointPositionConstraint>().ToArray();
            //foreach (var item in ppc)
            //{
            //    item.Update();
            //}*/
            //// ccc.FreezedPoints.AddRange(ppc.Select(z => z.Point).ToArray());

            ////expand graph solver
            //if (Constraints.Any(z => z is TopologyConstraint))
            //{
            //    CSPTask task = new CSPTask();
            //    int vcntr = 0;
            //    foreach (var item in DraftPoints)
            //    {
            //        task.Vars.Add(new CSPVar() { Name = "x" + vcntr });
            //        task.Vars.Add(new CSPVar() { Name = "y" + vcntr });
            //        vcntr++;
            //    }
            //    var ppc2 = Constraints.OfType<PointPositionConstraint>().ToArray();
            //    foreach (var item in ppc2)
            //    {
            //        var vind = DraftPoints.ToList().IndexOf(item.Point);
            //        task.Constrs.Add(new CSPConstrEqualVarValue(task.Vars.First(zz => zz.Name == "x" + vind), item.Location.X));
            //        task.Constrs.Add(new CSPConstrEqualVarValue(task.Vars.First(zz => zz.Name == "y" + vind), item.Location.Y));
            //    }
            //    var vert = Constraints.OfType<VerticalConstraint>().ToArray();
            //    foreach (var item in vert)
            //    {
            //        var vind0 = DraftPoints.ToList().IndexOf(item.Line.V0);
            //        var vind1 = DraftPoints.ToList().IndexOf(item.Line.V1);
            //        task.Constrs.Add(new CSPConstrEqualTwoVars(task.Vars.First(zz => zz.Name == "x" + vind0), task.Vars.First(uu => uu.Name == "x" + vind1)) { });
            //    }
            //    var horiz = Constraints.OfType<HorizontalConstraint>().ToArray();
            //    foreach (var item in horiz)
            //    {
            //        var vind0 = DraftPoints.ToList().IndexOf(item.Line.V0);
            //        var vind1 = DraftPoints.ToList().IndexOf(item.Line.V1);
            //        task.Constrs.Add(new CSPConstrEqualTwoVars(task.Vars.First(zz => zz.Name == "y" + vind0), task.Vars.First(uu => uu.Name == "y" + vind1)) { });
            //    }
            //    var linears = Constraints.OfType<LinearConstraint>().ToArray();
            //    var topo = Constraints.First(z => z is TopologyConstraint) as TopologyConstraint;
            //    foreach (var item in linears)
            //    {
            //        if (!(item.Element1 is DraftPoint dp0 && item.Element2 is DraftPoint dp1)) continue;
            //        var vind0 = DraftPoints.ToList().IndexOf(dp0);
            //        var vind1 = DraftPoints.ToList().IndexOf(dp1);

            //        var fr = topo.Lines.FirstOrDefault(z => new[] { z.Line.V0, z.Line.V1 }.Intersect(new[] { dp0, dp1 }).Count() == 2);
            //        if (fr == null) continue;
            //        var dd = fr.Dir;
            //        vind0 = DraftPoints.ToList().IndexOf(fr.Line.V0);
            //        vind1 = DraftPoints.ToList().IndexOf(fr.Line.V1);
            //        var vx1 = task.Vars.First(zz => zz.Name == "x" + vind0);
            //        var vx2 = task.Vars.First(zz => zz.Name == "x" + vind1);
            //        var vy1 = task.Vars.First(zz => zz.Name == "y" + vind0);
            //        var vy2 = task.Vars.First(zz => zz.Name == "y" + vind1);
            //        if (Math.Abs(dd.X) == 1)
            //        {
            //            task.Constrs.Add(new CSPConstrEqualExpression() { Vars = new[] { vx1, vx2 }, Expression = vx2.Name + "=" + vx1.Name + (dd.X > 0 ? "+" : "-") + item.Length });
            //        }
            //        if (Math.Abs(dd.Y) == 1)
            //        {
            //            task.Constrs.Add(new CSPConstrEqualExpression() { Vars = new[] { vy1, vy2 }, Expression = vy2.Name + "=" + vy1.Name + (dd.Y > 0 ? "+" : "-") + item.Length });
            //        }
            //    }

            //    CSPVarContext ctx = new CSPVarContext() { Task = task };
            //    if (ctx.Solve())
            //    {
            //        vcntr = 0;
            //        foreach (var item in DraftPoints)
            //        {
            //            var xv = task.Vars.First(zz => zz.Name == "x" + vcntr);
            //            var yv = task.Vars.First(zz => zz.Name == "y" + vcntr);
            //            item.SetLocation(ctx.Infos.First(z => z.Var == xv).Value, ctx.Infos.First(z => z.Var == yv).Value);
            //            vcntr++;
            //        }
            //        return;
            //    }
            //    /*   var top = Constraints.First(z => z is TopologyConstraint) as TopologyConstraint;
            //       ccc.FreezedLinesDirs = top.Lines.ToList();
            //       if (expandGraphSolver(ccc))
            //           return;*/
            //}
            //return;
            //RandomSolve();
        }
        public void RandomSolve()
        {
            ConstraintSolverContext ccc = new ConstraintSolverContext();
            var lc = Constraints.Where(z => z.Enabled).ToArray();
            int counter = 0;
            int limit = 1000;
            Stopwatch stw = Stopwatch.StartNew();
            StringWriter sw = new StringWriter();

            Store(sw);
            int timeLimit = 5;
            var elem = XElement.Parse(sw.ToString());

            while (true)
            {
                if (stw.Elapsed.TotalSeconds > timeLimit)
                {
                    DebugHelpers.Error("constraints satisfaction error");
                    Restore(XElement.Parse(sw.ToString()));
                    break;
                }
                counter++;
                if (lc.All(z => z.IsSatisfied())) break;
                //preserve Refs?
                //Restore(elem);

                /*if (counter > limit)
                {
                    DebugHelpers.Error("constraints satisfaction error");
                    break;
                }*/

                foreach (var item in lc)
                {
                    if (item.IsSatisfied())
                        continue;

                    item.RandomUpdate(ccc);
                }
            }
        }
        public List<DraftElement> Elements = new List<DraftElement>();
        public List<IDraftHelper> Helpers = new List<IDraftHelper>();
        public IEnumerable<IDraftConstraintHelper> ConstraintHelpers => Helpers.OfType<IDraftConstraintHelper>();
        public List<DraftConstraint> Constraints = new List<DraftConstraint>();
        public void AddHelper(IDraftHelper h)
        {
            Helpers.Add(h);
            h.Parent = this;
        }

        public Action<DraftConstraint> ConstraintAdded;
        public Action<DraftConstraint> BeforeConstraintChanged;
        public void AddConstraint(DraftConstraint h)
        {
            h.BeforeChanged = () =>
            {
                BeforeConstraintChanged?.Invoke(h);
            };
            Constraints.Add(h);
            RecalcConstraints();
            ConstraintAdded?.Invoke(h);
        }

        public decimal CalcTotalCutLength()
        {
            decimal ret = 0;
            ret += DraftEllipses.Sum(z => z.CutLength());
            foreach (var item in DraftLines)
            {
                ret += (decimal)((item.V0.Location - item.V1.Location).Length);
            }
            return ret;
        }

        public PlaneHelper Plane;

        public decimal CalcArea()
        {
            //get nest
            //calc area

            return 0;
        }

        public Vector2d[] Points => DraftPoints.Select(z => z.Location).ToArray();
        public DraftPoint[] DraftPoints => Elements.OfType<DraftPoint>().ToArray();
        public DraftLine[] DraftLines => Elements.OfType<DraftLine>().ToArray();
        public DraftEllipse[] DraftEllipses => Elements.OfType<DraftEllipse>().ToArray();
        public void EndEdit()
        {
            //2d->3d
            var basis = Plane.GetBasis();
            Points3D.Clear();
            foreach (var item in DraftLines)
            {
                Points3D.Add(basis[0] * item.V0.X + basis[1] * item.V0.Y + Plane.Position);
                Points3D.Add(basis[0] * item.V1.X + basis[1] * item.V1.Y + Plane.Position);
            }
        }
        public override void Draw()
        {

        }
        public override void RemoveChild(IDrawable dd)
        {
            if (dd is IDraftConstraintHelper dh)
            {
                Helpers.Remove(dh);
                Constraints.Remove(dh.Constraint);
            }

            Childs.Remove(dd);
        }

        public void AddElement(DraftElement h)
        {
            if (Elements.Contains(h))
                return;

            Elements.Add(h);
            h.Parent = this;
        }

        public void RemoveElement(IDraftHelper de)
        {
            Helpers.Remove(de);
        }

        public void RemoveElement(DraftElement de)
        {
            if (de is DraftPoint dp)
            {
                var ww = Elements.OfType<DraftLine>().Where(z => z.V0 == dp || z.V1 == dp).ToArray();
                var ww2 = Elements.OfType<DraftEllipse>().Where(z => z.Center == dp).ToArray();
                var ww3 = ww.OfType<DraftElement>().Union(ww2).ToArray();
                foreach (var item in ww3)
                {
                    Constraints.RemoveAll(z => z.ContainsElement(item));
                    Helpers.RemoveAll(zz => zz is IDraftConstraintHelper z && z.Constraint.ContainsElement(item));
                    Elements.Remove(item);
                }
            }
            Constraints.RemoveAll(z => z.ContainsElement(de));
            Helpers.RemoveAll(zz => zz is IDraftConstraintHelper z && z.Constraint.ContainsElement(de));
            Elements.Remove(de);
        }

    }
}
