using System;
using System.Drawing;
using System.Windows.Forms;

namespace CSPLib
{
    public class DraftEllipseTool : AbstractDraftTool
    {
        public DraftEllipseTool(IDraftEditor editor) : base(editor)
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
            var editor = Editor;
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

                    var p2 = new DraftPoint(_draft, firstClick.Value.X, firstClick.Value.Y);
                    var c = new DraftPoint(_draft, (firstClick.Value.X + p.X) / 2, (firstClick.Value.Y + p.Y) / 2);

                    editor.ResetTool();


                    _draft.AddElement(new DraftEllipse(c, (decimal)Math.Abs(firstClick.Value.X - p.X) / 2, _draft)
                    {
                        Angles = Angles,
                        SpecificAngles = SpecificAngles
                    });
                    firstClick = null;
                }
            }
        }
        public bool SpecificAngles { get; set; }
        public int Angles { get; set; }

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
