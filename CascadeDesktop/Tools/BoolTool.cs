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
            var obj = proxy.GetSelectedObject();

            if (obj != null)
            {
                objs.Add(proxy.GetSelectedObject());
                if (objs.Count == 2)
                {
                    switch (Operation)
                    {
                        case FuseOperation.Fuse:
                            Editor.Proxy.MakeFuse(objs[0], objs[1], true);
                            break;
                        case FuseOperation.Diff:
                            Editor.Proxy.MakeDiff(objs[0], objs[1]);
                            break;
                        case FuseOperation.Intersect:
                            Editor.Proxy.MakeCommon(objs[0], objs[1]);
                            break;
                    }
                    
                    foreach (var item in objs)
                    {
                        proxy.Erase(item);
                    }
                    Editor.ResetTool();
                }
            }
        }

        List<ManagedObjHandle> objs = new List<ManagedObjHandle>();

        public override void Select()
        {
            Editor.Proxy.SetSelectionMode(OCCTProxy.SelectionModeEnum.Shape);
        }

        public override void Update()
        {

        }
    }
}
