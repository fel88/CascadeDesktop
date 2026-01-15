using AutoDialog;
using Cascade.Common;
using CascadeDesktop.Interfaces;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Linq;
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
            var sob = proxy.GetSelectedObject();
            var fr = Editor.Objs.FirstOrDefault(z => z.ChildsIds.Contains(sob.BindId));
            if (fr == null)
                return;

            sob.AisShapeBindId = fr.Handle.BindId;

            var face = proxy.GetFaceInfo(sob);
            var vertex = proxy.GetVertexPosition(sob);
            var edge = proxy.GetEdgeInfoPosition(sob);

            if (vertex != null)
            {
                objs.Add(proxy.GetSelectedObject());
                vertices.Add(vertex.Value);
                if (vertices.Count == 2)
                {
                    var shift = vertices[1] - vertices[0];
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
                objs.Add(sob);
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
                objs.Add(sob);
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
        List<Vector3d> vertices = new List<Vector3d>();

        public override void Select()
        {
            Editor.Proxy.SetSelectionMode(SelectionModeEnum.Face);
        }

        public override void Update()
        {

        }
    }
}
