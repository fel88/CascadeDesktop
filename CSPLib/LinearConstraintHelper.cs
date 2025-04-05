using System.Drawing;
using OpenTK;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using SkiaSharp;
using System.Drawing.Drawing2D;
using CSPLib.Interfaces;

namespace CSPLib
{
    [XmlName(XmlName = "linearConstraintHelper")]
    public class LinearConstraintHelper : AbstractDrawable, IDraftConstraintHelper
    {
        public readonly LinearConstraint constraint;
        public LinearConstraintHelper(Draft parent, LinearConstraint c)
        {
            DraftParent = parent;
            constraint = c;
        }
        public LinearConstraintHelper(XElement el, Draft draft)
        {
            DraftParent = draft;
            var cid = int.Parse(el.Attribute("constrId").Value);
            constraint = draft.Constraints.OfType<LinearConstraint>().First(z => z.Id == cid);
            Shift = int.Parse(el.Attribute("shift").Value);
        }
        public decimal Length { get => constraint.Length; set => constraint.Length = value; }
        public Vector2d SnapPoint { get; set; }
        public DraftConstraint Constraint => constraint;
        public int Shift { get; set; } = 10;
        public bool Enabled { get => constraint.Enabled; set => constraint.Enabled = value; }

        public Draft DraftParent { get; private set; }

        public override void Store(TextWriter writer)
        {
            writer.WriteLine($"<linearConstraintHelper constrId=\"{constraint.Id}\" shift=\"{Shift}\" enabled=\"{Enabled}\" snapPoint=\"{SnapPoint.X};{SnapPoint.Y}\"/>");
        }
        public static SKPath RoundedRect(SKRect bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            SKRect arc = new SKRect(bounds.Location.X, bounds.Location.Y, bounds.Location.X + size.Width, bounds.Location.Y + size.Height);
            SKPath path = new SKPath();


            if (radius == 0)
            {
                path.AddRect(bounds);
                return path;
            }

            // top left arc  
            path.AddArc(arc, 180, 90);

            // top right arc  
            arc.Left = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc  
            arc.Top = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc 
            arc.Left = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.Close();
            return path;
        }

        public void Draw(IDrawingContext ctx)
        {
            var editor = ctx.Tag as IDraftEditor;
            var elems = new[] { constraint.Element1, constraint.Element2 };
            AdjustableArrowCap bigArrow = new AdjustableArrowCap(5, 5);
            var hovered = editor.nearest == this;
            Pen p = new Pen(hovered ? Color.Red : Color.Blue, 1);
            p.CustomEndCap = bigArrow;
            p.CustomStartCap = bigArrow;
            if (constraint.Element1 is DraftPoint dp0 && constraint.Element2 is DraftPoint dp1)
            {
                //get perpencdicular
                var diff = (dp1.Location - dp0.Location).Normalized();
                var perp = new Vector2d(-diff.Y, diff.X);
                var tr0 = ctx.Transform(dp0.Location + perp * Shift);
                var tr1 = ctx.Transform(dp1.Location + perp * Shift);
                var tr2 = ctx.Transform(dp0.Location);
                var tr3 = ctx.Transform(dp1.Location);
                var shiftX = 0;
                var text = (dp0.Location + perp * Shift + dp1.Location + perp * Shift) / 2 + perp;
                var trt = ctx.Transform(text);
                trt = new PointF(trt.X + shiftX, trt.Y);
                var ms = ctx.MeasureString(constraint.Length.ToString(), SystemFonts.DefaultFont);

                var fontBrush = Brushes.Black;
                if (hovered)
                    fontBrush = Brushes.Red;

                if (!constraint.IsSatisfied())
                {
                    var rect = new SKRect(trt.X, trt.Y, trt.X + ms.Width, trt.Y + ms.Height);
                    rect.Inflate(1.3f, 1.3f);
                    var rr = new SKRoundRect(rect, 5);
                    ctx.FillRoundRectangle(Brushes.Red, rr);
                    ctx.DrawRoundRectangle(Pens.Black, rr);
                    fontBrush = Brushes.White;
                }

                ctx.DrawString(constraint.Length.ToString(), SystemFonts.DefaultFont, fontBrush, trt);

                SnapPoint = text;

                //ctx.DrawLine(p, tr0, tr1);
                ctx.DrawArrowedLine(p, tr0, tr1, 5);
                ctx.SetPen(hovered ? Pens.Red : Pens.Blue);
                ctx.DrawLine(tr0, tr2);
                ctx.DrawLine(tr1, tr3);
                if (hovered)
                {
                    ctx.FillCircle(Brushes.Red, tr2.X, tr2.Y, 5);
                    ctx.FillCircle(Brushes.Red, tr3.X, tr3.Y, 5);
                }
            }
            if (elems.Any(z => z is DraftLine) && elems.Any(z => z is DraftPoint))
            {
                var dp = elems.OfType<DraftPoint>().First();
                var dl = elems.OfType<DraftLine>().First();
                var pp = GeometryUtils.GetProjPoint(dp.Location, dl.V0.Location, dl.Dir);

                var diff = (dp.Location - pp).Normalized();
                var perp = new Vector2d(-diff.Y, diff.X);
                var tr0 = ctx.Transform(dp.Location + perp * Shift);
                var tr1 = ctx.Transform(pp + perp * Shift);
                var text = (dp.Location + perp * Shift + pp + perp * Shift) / 2 + perp;
                var trt = ctx.Transform(text);
                ctx.DrawString(constraint.Length.ToString(), SystemFonts.DefaultFont, Brushes.Black, trt);
                SnapPoint = text;
                //get proj of point to line
                //var diff = (pp - dp.Location).Length;
                ctx.SetPen(p);
                ctx.DrawLine(tr0, tr1);
                var tr2 = ctx.Transform(dp.Location);
                var tr3 = ctx.Transform(pp);
                ctx.SetPen(Pens.Red);
                ctx.DrawLine(tr0, tr2);
                ctx.DrawLine(tr1, tr3);
            }
        }

        public override void Draw()
        {

        }
    }
}
