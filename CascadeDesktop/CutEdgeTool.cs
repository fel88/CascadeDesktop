using CSPLib;
using System.Windows.Forms;
using System.Linq;
using OpenTK;

namespace CascadeDesktop
{
    public class CutEdgeTool : AbstractDraftTool
    {
        public CutEdgeTool(IDraftEditor editor) : base(editor)
        {

        }

        public override void Deselect()
        {

        }

        public override void Draw()
        {
            var ctx = Editor.DrawingContext;
            var lns = Editor.Draft.DraftLines.ToArray();
            var gcur = ctx.GetCursor();
            var curp = ctx.Transform(gcur);
            double maxSnapDist = 10 / ctx.zoom;
            currentProj = null;
            line = null;

            foreach (var item in lns)
            {
                //get projpoint                     
                var proj = GeometryUtils.GetProjPoint(gcur.ToVector2d(), item.V0.Location, item.Dir);
                var sx = ctx.BackTransform(ctx.startx, ctx.starty);

                if (!item.ContainsPoint(proj))
                    continue;

                var len = (proj - gcur.ToVector2d()).Length;

                if (len < maxSnapDist)
                {
                    line = item;
                    //sub nearest = projpoint
                    curp = ctx.Transform(proj);
                    gcur = proj.ToPointF();
                    currentProj = proj;
                    var pp0 = ctx.Transform(proj.ToPointF());

                    ctx.DrawLine(pp0.Offset(0, 5), pp0.Offset(0, -5));
                    ctx.DrawLine(pp0.Offset(5, 0), pp0.Offset(-5, 0));
                }
            }
        }
        DraftLine line = null;
        Vector2d? currentProj = null;

        public override void MouseDown(MouseEventArgs e)
        {
            var _draft = Editor.Draft;
            if (e.Button == MouseButtons.Left)
            {
                if (currentProj != null)
                {
                    var pp = currentProj.Value;
                    Editor.Backup();
                    var p0 = new DraftPoint(_draft, pp.X, pp.Y);

                    _draft.RemoveElement(line);

                    Editor.ResetTool();
                    _draft.AddElement(p0);


                    var line1 = new DraftLine(line.V0, p0, _draft);
                    var line2 = new DraftLine(line.V1, p0, _draft);

                    _draft.AddElement(line1);
                    _draft.AddElement(line2);

                }
            }
        }

        public override void MouseUp(MouseEventArgs e)
        {

        }

        public override void Select()
        {

        }

        public override void Update()
        {

        }
    }
}
