using DxfPad;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CSPLib
{
    public class RectDraftTool : AbstractDraftTool
    {
        public RectDraftTool(IDraftEditor editor) : base(editor)
        {

        }

        public override void Deselect()
        {

        }

        public override void Draw()
        {

        }

        PointF? firstClick;

        public override void MouseDown(MouseEventArgs e)
        {
            var _draft = Editor.Draft;
            if (e.Button == MouseButtons.Left)
            {
                var p = Editor.DrawingContext.GetCursor();
                if (firstClick == null)
                {

                    firstClick = p;
                }
                else
                {
                    Editor.Backup();
                    var p0 = new DraftPoint(_draft, p.X, p.Y);
                    var p1 = new DraftPoint(_draft, firstClick.Value.X, p.Y);
                    var p2 = new DraftPoint(_draft, firstClick.Value.X, firstClick.Value.Y);
                    var p3 = new DraftPoint(_draft, p.X, firstClick.Value.Y);
                    firstClick = null;
                    Editor.ResetTool();
                    _draft.AddElement(p0);
                    _draft.AddElement(p1);
                    _draft.AddElement(p2);
                    _draft.AddElement(p3);

                    var line1 = new DraftLine(p0, p1, _draft);
                    var line2 = new DraftLine(p1, p2, _draft);
                    var line3 = new DraftLine(p2, p3, _draft);
                    var line4 = new DraftLine(p3, p0, _draft);
                    _draft.AddElement(line1);
                    _draft.AddElement(line2);
                    _draft.AddElement(line3);
                    _draft.AddElement(line4);

                    _draft.AddConstraint(new HorizontalConstraint(line1, _draft));
                    _draft.AddConstraint(new VerticalConstraint(line2, _draft));
                    _draft.AddConstraint(new HorizontalConstraint(line3, _draft));
                    _draft.AddConstraint(new VerticalConstraint(line4, _draft));

                    foreach (var item in _draft.Constraints.OfType<VerticalConstraint>())
                    {
                        if (_draft.ConstraintHelpers.Any(z => z.Constraint == item)) continue;
                        _draft.AddHelper(new VerticalConstraintHelper(item));
                    }
                    foreach (var item in _draft.Constraints.OfType<HorizontalConstraint>())
                    {
                        if (_draft.ConstraintHelpers.Any(z => z.Constraint == item)) continue;
                        _draft.AddHelper(new HorizontalConstraintHelper(item));
                    }
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
