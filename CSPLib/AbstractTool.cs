using System.Windows.Forms;

namespace CSPLib
{
    public abstract class AbstractTool : ITool
    {
        protected IEditor Editor;
        public AbstractTool(IEditor editor)
        {
            Editor = editor;
        }

        public abstract void Deselect();

        public abstract void Draw();

        public abstract void MouseDown(MouseEventArgs e);

        public abstract void MouseUp(MouseEventArgs e);

        public abstract void Select();

        public abstract void Update();

    }
}
