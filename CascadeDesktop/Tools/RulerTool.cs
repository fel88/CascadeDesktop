using Cascade.Common;
using CascadeDesktop.Interfaces;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Linq;
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
            var v = proxy.GetVertexPosition(proxy.GetSelectedObject());
            var face = proxy.GetFaceInfo(proxy.GetSelectedObject());
            var edge = proxy.GetEdgeInfoPosition(proxy.GetSelectedObject());
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
                if (mobjs.All(z => z is Vector3d))
                {
                    var vecs = mobjs.Cast<Vector3d>().ToArray();
                    Editor.SetStatus($"dist: {(vecs[0] - vecs[1]).Length}");
                    Editor.ResetTool();
                }
                else
                if (mobjs.Any(z => z is Vector3d) && mobjs.Any(z => z is EdgeInfo))
                {
                    var vec = (Vector3d)mobjs.First(z => z is Vector3d);
                    var edg = mobjs.First(z => z is EdgeInfo) as EdgeInfo;
                    if (edg.CurveType == CurveType.Line)
                    {
                        //get projection point to edge
                        var d = GeomHelpers.dist(vec, edg.Start, edg.End);
                        Editor.SetStatus($"dist: {d}");
                    }
                    else
                    {
                        Editor.SetStatus($"point to edge ({edg.CurveType}) dist: unsupported");
                    }
                    Editor.ResetTool();
                }
                else
                if (mobjs.Any(z => z is Vector3) && mobjs.Any(z => z is PlaneSurfInfo))
                {
                    var vec = (Vector3d)mobjs.First(z => z is Vector3d)  ;
                    var pln = mobjs.First(z => z is PlaneSurfInfo) as PlaneSurfInfo;
                    var p = new Plane() { Location = pln.Position, Normal = pln.Normal };
                    var len = (vec - p.GetProjPoint(vec)).Length;
                    Editor.SetStatus($"point to plane face dist: {len}");
                    Editor.ResetTool();
                }
                else
                if (mobjs.All(z => z is PlaneSurfInfo))
                {
                    var v1 = mobjs.First(z => z is PlaneSurfInfo) as PlaneSurfInfo;
                    var v2 = mobjs.First(z => z != v1 && z is PlaneSurfInfo) as PlaneSurfInfo;
                    if (!GeomHelpers.IsCollinear(v1.Normal, v2.Normal))
                    {
                        Editor.SetStatus($"plane to plane dist: not collinear");
                        Editor.ResetTool();
                        return;
                    }
                    var p = new Plane() { Location = v1.Position, Normal = v1.Normal };
                    var len = (v2.Position - p.GetProjPoint(v2.Position)).Length;
                    Editor.SetStatus($"plane to plane dist: {len}");
                    Editor.ResetTool();
                }
            }
        }

        List<ManagedObjHandle> objs = new List<ManagedObjHandle>();

        List<object> mobjs = new List<object>();

        public override void Select()
        {
            Editor.Proxy.ResetSelectionMode();
            Editor.Proxy.SetSelectionMode(SelectionModeEnum.Vertex);
            Editor.Proxy.SetSelectionMode(SelectionModeEnum.Face);
            Editor.Proxy.SetSelectionMode(SelectionModeEnum.Edge);
        }

        public override void Update()
        {

        }
    }
}
