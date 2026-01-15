using Cascade.Common;
using CascadeDesktop.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace CascadeDesktop.Tools
{
    public class AdjoinCOMsTool : AbstractTool
    {
        public AdjoinCOMsTool(IEditor editor) : base(editor)
        {
        }

        public override void Deselect()
        {
            surfs.Clear();
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
            var sob = proxy.GetSelectedObject();
            var fr = Editor.Objs.FirstOrDefault(z => z.ChildsIds.Contains(sob.BindId));
            if (fr == null)
                return;

            sob.AisShapeBindId = fr.Handle.BindId;
            var face = proxy.GetFaceInfo(sob);

            if (face == null)
                return;

            surfs.Add(face);
            
            objs.Add(sob);
            if (surfs.Count == 2)
            {
                var shift = -surfs[0].COM + surfs[1].COM;
                Editor.Proxy.MoveObject(objs[0], shift.X, shift.Y, shift.Z, true);
                Editor.ResetTool();
            }
        }

        List<ManagedObjHandle> objs = new List<ManagedObjHandle>();
        List<SurfInfo> surfs = new List<SurfInfo>();

        public override void Select()
        {
            Editor.Proxy.SetSelectionMode(SelectionModeEnum.Face);
        }

        public override void Update()
        {

        }
    }
}
