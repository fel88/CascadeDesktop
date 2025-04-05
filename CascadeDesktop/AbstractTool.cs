using CascadeDesktop.Interfaces;
using System.Windows.Forms;

namespace CascadeDesktop
{
    public abstract class AbstractTool : ITool
    {
        protected IEditor Editor;
        public AbstractTool(IEditor editor)
        {
            Editor = editor;
        }

        public virtual void Deselect() { }

        public virtual  void Draw() { }

        public virtual void MouseDown(MouseEventArgs e) { }

        public virtual void MouseUp(MouseEventArgs e) { }

        public virtual void Select() { }

        public virtual void Update() { }

    }
}
