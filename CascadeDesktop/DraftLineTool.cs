using CSPLib;
using System.Windows.Forms;

namespace CascadeDesktop
{
    public class DraftLineTool : AbstractDraftTool
    {
        public DraftLineTool(IDraftEditor editor) : base(editor)
        {

        }

        public override void Deselect()
        {
            lastDraftPoint = null;

        }

        public override void Draw()
        {

        }
        DraftPoint lastDraftPoint = null;

        public override void MouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var _draft = Editor.Draft;
                var nearest = Editor.nearest;
                var p = (Editor.DrawingContext.GetCursor());
                DraftPoint target = null;
                if (nearest is DraftPoint dp)
                {
                    target = dp;
                }
                else
                {
                    target = new DraftPoint(_draft, p.X, p.Y);
                    _draft.Elements.Add(target);
                }

                if (lastDraftPoint != null)
                {
                    _draft.Elements.Add(new DraftLine(lastDraftPoint, target, _draft));
                }
                lastDraftPoint = target;
            }
        }

        public override void MouseUp(MouseEventArgs e)
        {
            lastDraftPoint = null;

        }

        public override void Select()
        {

        }

        public override void Update()
        {

        }
    }
}
