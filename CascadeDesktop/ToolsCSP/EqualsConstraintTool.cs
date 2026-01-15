using CSPLib;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using CascadeDesktop.Common;

namespace CascadeDesktop.ToolsCSP
{
    public class EqualsConstraintTool : AbstractDraftTool
    {
        public EqualsConstraintTool(IDraftEditor editor) : base(editor)
        {
        }

        public override void Deselect()
        {

        }

        public override void Draw()
        {

        }
        List<DraftElement> queue = new List<DraftElement>();
        public override void MouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var nearest = Editor.nearest;
                if (Editor.nearest is DraftLine)
                {
                    if (!queue.Contains(nearest))
                        queue.Add(nearest as DraftLine);
                }
                var _draft = Editor.Draft;
                if (Editor.nearest is DraftLine dl && queue.Count > 1)
                {
                    var cc = new EqualsConstraint(queue[0] as DraftLine, dl, _draft);

                    if (!_draft.Constraints.OfType<EqualsConstraint>().Any(z => z.IsSame(cc)))
                    {
                        Editor.Backup();
                        _draft.AddConstraint(cc);
                        _draft.AddHelper(new EqualsConstraintHelper(_draft, cc));
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
            queue.Clear();
        }

        public override void Select()
        {


        }

        public override void Update()
        {

        }
    }
}
