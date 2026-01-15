using CSPLib;
using System.Windows.Forms;
using System.Linq;
using DxfPad;
using CascadeDesktop.Common;

namespace CascadeDesktop.ToolsCSP
{
    public class VerticalConstraintTool : AbstractDraftTool
    {
        public VerticalConstraintTool(IDraftEditor editor) : base(editor)
        {
        }

        public override void Deselect()
        {

        }

        public override void Draw()
        {

        }

        public override void MouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var _draft = Editor.Draft;
                if (Editor.nearest is DraftLine dl)
                {
                    var cc = new VerticalConstraint(dl, _draft);

                    if (!_draft.Constraints.OfType<VerticalConstraint>().Any(z => z.IsSame(cc)))
                    {
                        _draft.AddConstraint(cc);
                        _draft.AddHelper(new VerticalConstraintHelper(cc));
                        _draft.Childs.Add(_draft.Helpers.Last());
                    }
                    else
                    {
                        GUIHelpers.Warning("such constraint already exist");
                    }
                    //queue.Clear();
                    //editor.ResetTool();

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
