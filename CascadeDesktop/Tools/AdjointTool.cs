using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CascadeDesktop.Tools
{
    public class AdjointTool : AbstractTool
    {
        public AdjointTool(IEditor editor) : base(editor)
        {

        }

        public override void Deselect()
        {
            planes.Clear();
            objs.Clear();
        }

        public override void Draw()
        {

        }

        public override void MouseDown(MouseEventArgs e)
        {

        }

        public override void MouseUp(MouseEventArgs e)
        {
            if (!Editor.Proxy.IsObjectSelected())            
                return;
            
            var proxy = Editor.Proxy;
            var face = proxy.GetFaceInfo(proxy.GetSelectedObject());

            if (face != null && face is PlaneSurfInfo p)
            {
                planes.Add(p);
                objs.Add(proxy.GetSelectedObject());
                if (planes.Count == 2)
                {
                    var shift = GeomHelpers.GetAdjointFacesShift(planes[0], planes[1]);
                    if (shift != null)
                    {
                        Editor.Proxy.MoveObject(objs[0], shift.Value.X, shift.Value.Y, shift.Value.Z, true);
                    }
                    Editor.ResetTool();
                }
            }
        }

        List<ManagedObjHandle> objs = new List<ManagedObjHandle>();
        List<PlaneSurfInfo> planes = new List<PlaneSurfInfo>();

        public override void Select()
        {
            Editor.Proxy.SetSelectionMode(OCCTProxy.SelectionModeEnum.Face);
        }

        public override void Update()
        {

        }
    }
}
