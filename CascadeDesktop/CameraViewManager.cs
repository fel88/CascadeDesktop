using OpenTK;

namespace CascadeDesktop
{
    public abstract class CameraViewManager
    {
        public GLControl Control;
        public EventWrapperGlControl EventWrapper;

        public virtual void Deattach(EventWrapperGlControl control)
        {
            control.MouseUpAction = null;
            control.MouseDownAction = null;
            control.MouseWheelAction = null;
            control.KeyDownAction = null;
            control.KeyUpUpAction = null;

        }

        public abstract void Update();

        public virtual void Attach(EventWrapperGlControl control, Camera camera)
        {
            Control = control.Control;
            EventWrapper = control;
        }
    }
}
