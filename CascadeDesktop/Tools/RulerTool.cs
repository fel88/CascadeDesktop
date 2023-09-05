using OpenTK;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CascadeDesktop.Tools
{
    public class RulerTool : AbstractTool
    {
        public RulerTool(IEditor editor) : base(editor)
        {

        }

        public override void Deselect()
        {
            vecs.Clear();            
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


            vecs.Add(proxy.GetVertexPoition(proxy.GetSelectedObject()).ToVector3d());
            if (vecs.Count == 2)
            {
                Editor.SetStatus($"dist: {(vecs[0] - vecs[1]).Length}");
                vecs.Clear();
                Editor.ResetTool();
            }


        }

        List<ManagedObjHandle> objs = new List<ManagedObjHandle>();
        List<Vector3d> vecs = new List<Vector3d>();

        public override void Select()
        {
            Editor.Proxy.SetSelectionMode(OCCTProxy.SelectionModeEnum.Vertex);
        }

        public override void Update()
        {

        }
    }
}
