using System.Drawing;
using OpenTK;
using CSPLib.Interfaces;

namespace CSPLib
{
    public class EqualsConstraintHelper : AbstractDrawable, IDraftConstraintHelper
    {
        public readonly EqualsConstraint constraint;

        public Draft DraftParent { get; private set; }
        public EqualsConstraintHelper(Draft parent, EqualsConstraint c)
        {
            DraftParent = parent;
            constraint = c;
        }

        public Vector2d SnapPoint { get; set; }
        public DraftConstraint Constraint => constraint;

        public bool Enabled { get => constraint.Enabled; set => constraint.Enabled = value; }

        public void Draw(IDrawingContext ctx)
        {
            var editor = ctx.Tag as IDraftEditor;


            var hovered = editor.nearest == this;

            var dp0 = constraint.TargetLine.Center;
            var perp = new Vector2d(-constraint.TargetLine.Dir.Y, constraint.TargetLine.Dir.X);

            SnapPoint = (dp0) + constraint.TargetLine.Dir * 10 / ctx.zoom;
            var tr0 = ctx.Transform(dp0 + perp * 15 / ctx.zoom);

            var gap = 10;
            var gap2 = 6;
            var offset = 25;
            //create bezier here
            var shiftX = (int)(constraint.TargetLine.Dir.X * offset);
            var shiftY = (int)(constraint.TargetLine.Dir.Y * offset);

            ctx.FillCircle(hovered ? Brushes.Blue : Brushes.Green, tr0.X + shiftX, tr0.Y + shiftY, gap);
            ctx.SetPen(new Pen(Brushes.Violet, 3));
            ctx.DrawLine(tr0.X - gap2 + shiftX, tr0.Y - 4 + shiftY, tr0.X + gap2 + shiftX, tr0.Y - 4 + shiftY);
            ctx.DrawLine(tr0.X - gap2 + shiftX, tr0.Y + 4 + shiftY, tr0.X + gap2 + shiftX, tr0.Y + 4 + shiftY);

            if (hovered)
            {
                var tr00 = ctx.Transform(constraint.SourceLine.V0.Location);
                var tr1 = ctx.Transform(constraint.SourceLine.V1.Location);
                var tr2 = ctx.Transform(constraint.TargetLine.V0.Location);
                var tr3 = ctx.Transform(constraint.TargetLine.V1.Location);
                ctx.FillCircle(Brushes.Red, tr2.X, tr2.Y, 5);
                ctx.FillCircle(Brushes.Red, tr3.X, tr3.Y, 5);

                ctx.FillCircle(Brushes.Red, tr00.X, tr00.Y, 5);
                ctx.FillCircle(Brushes.Red, tr1.X, tr1.Y, 5);
            }
        }

        public override void Draw()
        {

        }
    }
}
