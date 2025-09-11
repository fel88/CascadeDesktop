using System.Drawing;
using OpenTK;
using System.Drawing.Drawing2D;
using CSPLib.Interfaces;
using OpenTK.Mathematics;

namespace CSPLib
{
    public class PerpendicularConstraintHelper : AbstractDrawable, IDraftConstraintHelper
    {
        public readonly PerpendicularConstraint constraint;
        public PerpendicularConstraintHelper(PerpendicularConstraint c)
        {
            constraint = c;
        }

        public Vector2d SnapPoint { get; set; }
        public DraftConstraint Constraint => constraint;

        public bool Enabled { get => constraint.Enabled; set => constraint.Enabled = value; }

        public Draft DraftParent => throw new System.NotImplementedException();

        public void Draw(IDrawingContext ctx)
        {
            var dp0 = constraint.Element1.Center;
            var dp1 = constraint.Element2.Center;
            var tr0 = ctx.Transform(dp0);
            var tr1 = ctx.Transform(dp1);
            var text = ctx.Transform((dp0 + dp1) / 2);

            ctx.DrawString("P-|", SystemFonts.DefaultFont, Brushes.Black, text);
            SnapPoint = (dp0 + dp1) / 2;
            AdjustableArrowCap bigArrow = new AdjustableArrowCap(5, 5);
            Pen p = new Pen(Color.Red, 1);
            p.CustomEndCap = bigArrow;
            p.CustomStartCap = bigArrow;


            //create bezier here
            ctx.DrawPolygon(p, new PointF[] { tr0, tr1 });
        }

        public override void Draw()
        {

        }
    }
}
