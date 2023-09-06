using OpenTK;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Runtime.InteropServices;
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
            mobjs.Clear();
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
            var v = proxy.GetVertexPoition(proxy.GetSelectedObject());
            var face = proxy.GetFaceInfo(proxy.GetSelectedObject());
            var edge = proxy.GetEdgeInfoPoition(proxy.GetSelectedObject());
            if (v != null || face != null || edge != null)
            {
                objs.Add(proxy.GetSelectedObject());
                foreach (var item in new object[] { v, face, edge })
                {
                    if (item != null)
                        mobjs.Add(item);
                }
            }

            //vecs.Add(proxy.GetVertexPoition(proxy.GetSelectedObject()).ToVector3d());            
            if (objs.Count == 2)
            {
                if (mobjs.All(z => z is Vector3))
                {
                    var vecs = mobjs.Cast<Vector3>().ToArray();
                    Editor.SetStatus($"dist: {(vecs[0].ToVector3d() - vecs[1].ToVector3d()).Length}");
                    Editor.ResetTool();
                }
                else
                if (mobjs.Any(z => z is Vector3) && mobjs.Any(z => z is EdgeInfo))
                {
                    var vec = mobjs.First(z => z is Vector3) as Vector3;
                    var edg = mobjs.First(z => z is EdgeInfo) as EdgeInfo;
                    //get projection point to edge
                    Editor.SetStatus($"point to edge dist: unsupported");
                    Editor.ResetTool();
                }
                else
                if (mobjs.Any(z => z is Vector3) && mobjs.Any(z => z is PlaneSurfInfo))
                {
                    var vec = mobjs.First(z => z is Vector3) as Vector3;
                    var pln= mobjs.First(z => z is PlaneSurfInfo) as PlaneSurfInfo;
                    var p = new Plane() { Location = pln.Position.ToVector3d(), Normal = pln.Normal.ToVector3d() };
                    var len = (vec.ToVector3d() - p.GetProjPoint(vec.ToVector3d())).Length;
                    Editor.SetStatus($"point to plane face dist: {len}");
                    Editor.ResetTool();
                }
            }
        }

        List<ManagedObjHandle> objs = new List<ManagedObjHandle>();

        List<object> mobjs = new List<object>();

        public override void Select()
        {
            Editor.Proxy.ResetSelectionMode();
            Editor.Proxy.SetSelectionMode(OCCTProxy.SelectionModeEnum.Vertex);
            Editor.Proxy.SetSelectionMode(OCCTProxy.SelectionModeEnum.Face);
            Editor.Proxy.SetSelectionMode(OCCTProxy.SelectionModeEnum.Edge);
        }

        public override void Update()
        {

        }
    }
}
