using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CascadeDesktop.Tools
{
    public class BoolTool : AbstractTool
    {
        public enum FuseOperation
        {
            Fuse, Diff, Intersect
        }

        FuseOperation Operation;
        public BoolTool(IEditor editor, FuseOperation op) : base(editor)
        {
            Operation = op;
        }

        public override void Deselect()
        {
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
            var obj = Editor.GetSelectedOccObject();

            if (obj == null)
                return;

            objs.Add(obj);
            if (objs.Count == 2)
            {
                switch (Operation)
                {
                    case FuseOperation.Fuse:
                        {
                            var cs = Editor.Proxy.MakeFuse(objs[0].Handle, objs[1].Handle);
                            Editor.Objs.Add(new OccSceneObject(cs, Editor.Proxy) { Name = $"{objs[0].Name}_{objs[1].Name}_fuse" });
                        }
                        break;
                    case FuseOperation.Diff:
                        {
                            var cs = Editor.Proxy.MakeDiff(objs[0].Handle, objs[1].Handle);
                            Editor.Objs.Add(new OccSceneObject(cs, Editor.Proxy) { Name = $"{objs[0].Name}_{objs[1].Name}_diff" });
                        }
                        break;
                    case FuseOperation.Intersect:
                        {
                            var cs = Editor.Proxy.MakeCommon(objs[0].Handle, objs[1].Handle);
                            Editor.Objs.Add(new OccSceneObject(cs, Editor.Proxy) { Name = $"{objs[0].Name}_{objs[1].Name}_common" });
                        }
                        break;
                }

              
                if (MessageBox.Show("Remove source objects?", "OCC", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    foreach (var item in objs)
                    {
                        Editor.Remove(item);
                    }
                }
                Editor.ResetTool(); 
                
                Editor.Proxy.UpdateCurrentViewer();                
            }
        }

        List<OccSceneObject> objs = new List<OccSceneObject>();

        public override void Select()
        {
            Editor.Proxy.SetSelectionMode(OCCTProxy.SelectionModeEnum.Shape);
        }

        public override void Update()
        {

        }
    }
}
