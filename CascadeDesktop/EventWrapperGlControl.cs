using OpenTK;
using System;
using System.Windows.Forms;

namespace CascadeDesktop
{
    public class EventWrapperGlControl
    {
        public GLControl Control;
        public EventWrapperGlControl(GLControl control)
        {
            Control = control;
            control.MouseUp += WrapGlControl_MouseUp;
            control.MouseDown += Control_MouseDown;
            control.KeyDown += Control_KeyDown;
            control.MouseWheel += Control_MouseWheel;
            control.KeyUp += Control_KeyUp;
        }

        private void Control_KeyUp(object sender, KeyEventArgs e)
        {
            if (KeyUpUpAction != null)
            {
                KeyUpUpAction(sender, e);
            }
        }

        private void Control_MouseWheel(object sender, MouseEventArgs e)
        {
            if (MouseWheelAction != null)
            {
                MouseWheelAction(sender, e);
            }

        }

        private void Control_KeyDown(object sender, KeyEventArgs e)
        {
            if (KeyDownAction != null)
            {
                KeyDownAction(sender, e);
            }
        }

        private void Control_MouseDown(object sender, MouseEventArgs e)
        {
            if (MouseDownAction != null)
            {
                MouseDownAction(sender, e);
            }
        }

        private void WrapGlControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (MouseUpAction != null)
            {
                MouseUpAction(sender, e);
            }

        }

        public Action<object, MouseEventArgs> MouseUpAction;
        public Action<object, MouseEventArgs> MouseDownAction;
        public Action<object, MouseEventArgs> MouseWheelAction;
        public Action<object, KeyEventArgs> KeyUpUpAction;
        public Action<object, KeyEventArgs> KeyDownAction;
    }
}
