using AutoDialog;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CascadeDesktop.Tools
{
    public class AdjoinTool : AbstractTool
    {
        bool WithDistance;
        public AdjoinTool(IEditor editor, bool withDistance) : base(editor)
        {
            WithDistance = withDistance;
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
            var vertex = proxy.GetVertexPosition(proxy.GetSelectedObject());
            var edge = proxy.GetEdgeInfoPosition(proxy.GetSelectedObject());

            if (vertex != null)
            {
                objs.Add(proxy.GetSelectedObject());
                vertices.Add(vertex);
                if (vertices.Count == 2)
                {
                    var shift = vertices[1].ToVector3d() - vertices[0].ToVector3d();
                    if (shift != null)
                    {
                        var dir = shift.Normalized();
                        Editor.Proxy.MoveObject(objs[0], shift.X, shift.Y, shift.Z, true);
                    }
                    Editor.ResetTool();
                    return;
                }
            }
            else if (edge != null)
            {
                objs.Add(proxy.GetSelectedObject());
                edges.Add(edge);
                if (edges.Count == 2)
                {
                    var shift = GeomHelpers.GetAdjointEdgesShift(edges[0], edges[1]);
                    if (shift != null)
                    {
                        var dir = shift.Value.Normalized();
                        Editor.Proxy.MoveObject(objs[0], shift.Value.X, shift.Value.Y, shift.Value.Z, true);
                    }
                    Editor.ResetTool();
                    return;
                }
            }
            else if (face is PlaneSurfInfo p)
            {
                planes.Add(p);
                objs.Add(proxy.GetSelectedObject());
                if (planes.Count == 2)
                {
                    var shift = GeomHelpers.GetAdjointFacesShift(planes[0], planes[1]);
                    if (shift != null)
                    {
                        var dir = shift.Value.Normalized();
                        if (WithDistance)
                        {
                            var d = DialogHelpers.StartDialog();
                            d.AddNumericField("d", "dist", 0, 10000, -10000);

                            if (!d.ShowDialog())
                                return;

                            var dist = d.GetNumericField("d");

                            Editor.Proxy.MoveObject(objs[0], shift.Value.X, shift.Value.Y, shift.Value.Z, true);
                            var res = -dir * dist;
                            Editor.Proxy.MoveObject(objs[0], res.X, res.Y, res.Z, true);
                        }
                        else
                            Editor.Proxy.MoveObject(objs[0], shift.Value.X, shift.Value.Y, shift.Value.Z, true);
                    }
                    Editor.ResetTool();
                }
            }
            else if (face is CylinderSurfInfo c)
            {
                cylinders.Add(c);
                objs.Add(proxy.GetSelectedObject());
                if (cylinders.Count == 2)
                {
                    var shift = GeomHelpers.GetAdjointFacesShift(cylinders[0], cylinders[1]);
                    if (shift != null)
                    {
                        var dir = shift.Value.Normalized();
                        if (WithDistance)
                        {
                            var d = DialogHelpers.StartDialog();
                            d.AddNumericField("d", "dist", 0, 10000, -10000);


                            if (!d.ShowDialog())
                                return;

                            var dist = d.GetNumericField("d");

                            Editor.Proxy.MoveObject(objs[0], shift.Value.X, shift.Value.Y, shift.Value.Z, true);
                            var res = -dir * dist;
                            Editor.Proxy.MoveObject(objs[0], res.X, res.Y, res.Z, true);
                        }
                        else
                            Editor.Proxy.MoveObject(objs[0], shift.Value.X, shift.Value.Y, shift.Value.Z, true);
                    }
                    Editor.ResetTool();
                }
            }
        }

        List<ManagedObjHandle> objs = new List<ManagedObjHandle>();
        List<PlaneSurfInfo> planes = new List<PlaneSurfInfo>();
        List<CylinderSurfInfo> cylinders = new List<CylinderSurfInfo>();
        List<EdgeInfo> edges = new List<EdgeInfo>();
        List<Vector3> vertices = new List<Vector3>();

        public override void Select()
        {
            Editor.Proxy.SetSelectionMode(OCCTProxy.SelectionModeEnum.Face);
        }

        public override void Update()
        {

        }
    }
}
