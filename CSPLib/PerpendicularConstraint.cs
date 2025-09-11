using System;
using System.Data;
using OpenTK;
using System.Linq;
using System.IO;
using OpenTK.Mathematics;

namespace CSPLib
{
    public class PerpendicularConstraint : DraftConstraint
    {
        public DraftLine Element1;
        public DraftLine Element2;
        public DraftPoint CommonPoint;
        public PerpendicularConstraint(DraftLine draftPoint1, DraftLine draftPoint2, Draft parent) : base(parent)
        {
            var ar1 = new[] { draftPoint2.V0, draftPoint2.V1 };
            var ar2 = new[] { draftPoint1.V0, draftPoint1.V1 };
            if (ar1.Intersect(ar2).Count() != 1) throw new ArgumentException();
            CommonPoint = ar1.Intersect(ar2).First();
            this.Element1 = draftPoint1;
            this.Element2 = draftPoint2;
        }

        public override bool IsSatisfied(float eps = 1e-6f)
        {
            var dp0 = Element1 as DraftLine;
            var dp1 = Element2 as DraftLine;

            return Math.Abs(Vector2d.Dot(dp0.Dir, dp1.Dir)) <= eps;
        }

        internal void Update()
        {
            var dp0 = Element1 as DraftLine;
            var dp1 = Element2 as DraftLine;
            /*var diff = (dp1.Location - dp0.Location).Normalized();
            dp1.Location = dp0.Location + diff * (double)Length;*/
        }
        public override void RandomUpdate(ConstraintSolverContext ctx)
        {
            var dp0 = Element1 as DraftLine;
            var dp1 = Element2 as DraftLine;
            if (dp0.Frozen && dp1.Frozen)
            {
                throw new ConstraintsException("double frozen");
            }
            var ar = new[] { dp0, dp1 }.OrderBy(z => GeometryUtils.Random.Next(100)).ToArray();
            dp0 = ar[0];
            dp1 = ar[1];
            if (dp1.Frozen || (dp1.V0 == CommonPoint && dp1.V1.Frozen) || (dp1.V1 == CommonPoint && dp1.V0.Frozen))
            {
                var temp = dp1;
                dp1 = dp0;
                dp0 = temp;
            }

            //generate all valid candidates first. then random select
            //not frozen points to move
            var mp = new[] { dp1.V0, dp1.V1, dp0.V1, dp0.V0 }.Distinct().Where(z => !z.Frozen).ToArray();

            if (!CommonPoint.Frozen)
            {
                //intersect
            }
            else
            if (dp1.V0 == CommonPoint)
            {
                var diff = dp1.Dir * dp1.Length;
                var projectV = new Vector2d(-dp0.Dir.Y, dp0.Dir.X);
                var cand1 = CommonPoint.Location + projectV * dp1.Length;
                var cand2 = CommonPoint.Location - projectV * dp1.Length;
                if ((cand1 - dp1.V1.Location).Length < (cand2 - dp1.V1.Location).Length)
                {
                    dp1.V1.SetLocation(cand1);
                }
                else
                {
                    dp1.V1.SetLocation(cand2);
                }
            }
            else
            {
                var diff = dp1.Dir * dp1.Length;
                var projectV = new Vector2d(-dp0.Dir.Y, dp0.Dir.X);
                //dp1.V0.SetLocation(CommonPoint.Location + projectV * dp1.Length);
                var cand1 = CommonPoint.Location + projectV * dp1.Length;
                var cand2 = CommonPoint.Location - projectV * dp1.Length;
                if ((cand1 - dp1.V0.Location).Length < (cand2 - dp1.V0.Location).Length)
                {
                    dp1.V0.SetLocation(cand1);
                }
                else
                {
                    dp1.V0.SetLocation(cand2);
                }
            }
            /* var diff = (dp1.Location - dp0.Location).Normalized();
             dp1.Location = dp0.Location + diff * (double)Length;*/
        }
        public bool IsSame(PerpendicularConstraint cc)
        {
            return new[] { Element2, Element1 }.Except(new[] { cc.Element1, cc.Element2 }).Count() == 0;
        }

        public override bool ContainsElement(DraftElement de)
        {
            return Element1 == de || Element2 == de;
        }

        internal override void Store(TextWriter writer)
        {
            writer.WriteLine($"<perpendicularConstraint p0=\"{Element1.Id}\" p1=\"{Element2.Id}\"/>");
        }
    }
}
