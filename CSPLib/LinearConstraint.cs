using System;
using System.Data;
using System.Linq;
using System.IO;
using System.Xml.Linq;

namespace CSPLib
{
    [XmlName(XmlName = "linearConstraint")]
    public class LinearConstraint : DraftConstraint
    {
        public DraftElement Element1;
        public DraftElement Element2;

        decimal _length;
        public decimal Length
        {
            get => _length; set
            {
                BeforeChanged?.Invoke();
                _length = value;
                Element1.Parent.RecalcConstraints();
            }
        }
        public LinearConstraint(XElement el, Draft parent) : base(parent)
        {
            if (el.Attribute("id") != null)
                Id = int.Parse(el.Attribute("id").Value);

            Element1 = parent.Elements.First(z => z.Id == int.Parse(el.Attribute("p0").Value));
            Element2 = parent.Elements.First(z => z.Id == int.Parse(el.Attribute("p1").Value));
            Length = Helpers.ParseDecimal(el.Attribute("length").Value);
        }

        public LinearConstraint(DraftElement draftPoint1, DraftElement draftPoint2, decimal len, Draft parent) : base(parent)
        {
            this.Element1 = draftPoint1;
            this.Element2 = draftPoint2;
            Length = len;
        }

        public override bool IsSatisfied(float eps = 1e-6f)
        {
            var elems = new[] { Element1, Element2 };
            if (Element1 is DraftPoint dp0 && Element2 is DraftPoint dp1)
            {
                var diff = (dp1.Location - dp0.Location).Length;
                return Math.Abs(diff - (double)Length) < eps;
            }
            if (elems.Any(z => z is DraftLine) && elems.Any(z => z is DraftPoint))
            {
                var dp = elems.OfType<DraftPoint>().First();
                var dl = elems.OfType<DraftLine>().First();

                //get proj of point to line
                var pp = GeometryUtils.GetProjPoint(dp.Location, dl.V0.Location, dl.Dir);
                var diff = (pp - dp.Location).Length;
                return Math.Abs(diff - (double)Length) < eps;
            }
            throw new NotImplementedException();
        }

        internal void Update()
        {
            var dp0 = Element1 as DraftPoint;
            var dp1 = Element2 as DraftPoint;
            var diff = (dp1.Location - dp0.Location).Normalized();
            dp1.SetLocation(dp0.Location + diff * (double)Length);
        }

        public override void RandomUpdate(ConstraintSolverContext ctx)
        {
            var elems = new[] { Element1, Element2 };

            if (Element1 is DraftPoint dp0 && Element2 is DraftPoint dp1)
            {
                if ((dp0.Frozen && dp1.Frozen) || (ctx.FreezedPoints.Contains(dp1) && ctx.FreezedPoints.Contains(dp0)))
                {
                    throw new ConstraintsException("double frozen");
                }
                var ar = new[] { dp0, dp1 }.OrderBy(z => GeometryUtils.Random.Next(100)).ToArray();
                dp0 = ar[0];
                dp1 = ar[1];
                if (dp1.Frozen || ctx.FreezedPoints.Contains(dp1))
                {
                    var temp = dp1;
                    dp1 = dp0;
                    dp0 = temp;
                }
                var diff = (dp1.Location - dp0.Location).Normalized();
                //preserve location
                bool good = false;
                var fr = ctx.FreezedLinesDirs.FirstOrDefault(z => (z.Line.V0 == dp0 && z.Line.V1 == dp1) || (z.Line.V0 == dp1 && z.Line.V1 == dp0));
                if (fr != null)
                {
                    var lns1 = dp0.Parent.DraftLines.FirstOrDefault(uu => (uu.V0 == dp0 && uu.V1 == dp1) || (uu.V0 == dp1 && uu.V1 == dp0));
                    if (lns1 != null)
                    {
                        var fr2 = ctx.FreezedLinesDirs.FirstOrDefault(zz => zz.Line == lns1);
                        if (fr2 != null)
                        {
                            diff = fr2.Dir;
                            dp0 = Element1 as DraftPoint;
                            dp1 = Element2 as DraftPoint;
                            if (ctx.FreezedPoints.Contains(dp1) && !ctx.FreezedPoints.Contains(dp0))
                            {
                                dp0.SetLocation(dp1.Location - diff * (double)Length);
                                good = true;
                            }
                            if (ctx.FreezedPoints.Contains(dp0) && !ctx.FreezedPoints.Contains(dp1))
                            {
                                dp1.SetLocation(dp0.Location + diff * (double)Length);
                                good = true;
                            }
                        }
                    }
                }
                if (!good)
                    dp1.SetLocation(dp0.Location + diff * (double)Length);
            }
            if (elems.Any(z => z is DraftLine) && elems.Any(z => z is DraftPoint))
            {
                var dp = elems.OfType<DraftPoint>().First();
                var dl = elems.OfType<DraftLine>().First();
                var pp = GeometryUtils.GetProjPoint(dp.Location, dl.V0.Location, dl.Dir);

                var cand1 = pp + dl.Normal * (double)Length;
                var cand2 = pp - dl.Normal * (double)Length;
                if (GeometryUtils.Random.Next(100) < 50)
                {
                    dp.SetLocation(cand1);
                }
                else
                {
                    dp.SetLocation(cand2);
                }
            }
        }
        public bool IsSame(LinearConstraint cc)
        {
            return new[] { Element2, Element1 }.Except(new[] { cc.Element1, cc.Element2 }).Count() == 0;
        }
        public bool IsLineConstraint(DraftLine line)
        {
            if (!(Element1 is DraftPoint dp0 && Element2 is DraftPoint dp1)) return false;
            return new[] { line.V0, line.V1 }.Intersect(new[] { dp0, dp1 }).Count() == 2;
        }
        public override bool ContainsElement(DraftElement de)
        {
            return Element1 == de || Element2 == de;
        }

        internal override void Store(TextWriter writer)
        {
            writer.WriteLine($"<linearConstraint id=\"{Id}\" length=\"{Length}\" p0=\"{Element1.Id}\" p1=\"{Element2.Id}\"/>");
        }
    }

}
