using CSPLib;
using System.Windows.Forms;
using System.Linq;
using DxfPad;
using CascadeDesktop.Common;

namespace CascadeDesktop.ToolsCSP
{
    public class HorizontalConstraintTool : AbstractDraftTool
    {

        public HorizontalConstraintTool(IDraftEditor e) : base(e) { }

        public override void Deselect()
        {

        }

        public override void Draw()
        {

        }

        public override void MouseDown(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            var draft = Editor.Draft;
            if (Editor.nearest is DraftLine dl)
            {
                var cc = new HorizontalConstraint(dl, draft);

                if (!Editor.Draft.Constraints.OfType<HorizontalConstraint>().Any(z => z.IsSame(cc)))
                {
                    Editor.Draft.AddConstraint(cc);
                    Editor.Draft.AddHelper(new HorizontalConstraintHelper(cc));
                    Editor.Draft.Childs.Add(Editor.Draft.Helpers.Last());
                }
                else
                {
                    GUIHelpers.Warning("such constraint already exist");
                }

                //Form1.Form.ResetTool();

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
