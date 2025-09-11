using System.Drawing;
using OpenTK;
using DxfPad;
using CSPLib.Interfaces;
using OpenTK.Mathematics;

namespace CSPLib
{
    public class HorizontalConstraintHelper : AbstractDrawable, IDraftConstraintHelper
    {
        public readonly HorizontalConstraint constraint;
        public HorizontalConstraintHelper(HorizontalConstraint c)
        {
            constraint = c;
        }

        public Vector2d SnapPoint { get; set; }
        public DraftConstraint Constraint => constraint;

        public bool Enabled { get => constraint.Enabled; set => constraint.Enabled = value; }

        public Draft DraftParent { get; private set; }

        public void Draw(IDrawingContext ctx)
        {
            var dp0 = constraint.Line.Center;
            var perp = new Vector2d(-constraint.Line.Dir.Y, constraint.Line.Dir.X);

            SnapPoint = (dp0);
            var tr0 = ctx.Transform(dp0 + perp * 15 / ctx.zoom);

            var gap = 10;
            //create bezier here
            ctx.FillCircle(Brushes.Green, tr0.X, tr0.Y, gap);
            ctx.SetPen(new Pen(Brushes.Orange, 3));
            ctx.DrawLine(tr0.X - 5, tr0.Y, tr0.X + 5, tr0.Y);
        }

        public override void Draw()
        {

        }
    }
}
