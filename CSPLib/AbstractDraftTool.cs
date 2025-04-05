using CSPLib.Interfaces;
using System.Windows.Forms;

namespace CSPLib
{
    public abstract class AbstractDraftTool : ITool
    {

        public AbstractDraftTool(IDraftEditor editor)
        {
            Editor = editor;
        }

        public abstract void Deselect();


        public abstract void Draw();

        public IDraftEditor Editor;
        public abstract void MouseDown(MouseEventArgs e);

        public abstract void MouseUp(MouseEventArgs e);

        public abstract void Select();

        public abstract void Update();
    }
}
