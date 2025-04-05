﻿using System;
using System.Windows.Forms;
using MathNet.Numerics;

namespace CSPLib
{
    public class EventWrapperPictureBox
    {
        public System.Windows.Forms.Control Control;
        public EventWrapperPictureBox(System.Windows.Forms.Control control)
        {
            Control = control;
            control.MouseUp += WrapGlControl_MouseUp;
            control.MouseDown += Control_MouseDown;
            control.KeyDown += Control_KeyDown;
            control.MouseMove += WrapGlControl_MouseMove;
            control.MouseWheel += Control_MouseWheel;
            control.KeyUp += Control_KeyUp;
            control.SizeChanged += Control_SizeChanged;
        }

        private void Control_SizeChanged(object sender, EventArgs e)
        {
            SizeChangedAction?.Invoke(sender, e);
        }

        private void Control_KeyUp(object sender, KeyEventArgs e)
        {
            KeyUpUpAction?.Invoke(sender, e);
        }

        private void Control_MouseWheel(object sender, MouseEventArgs e)
        {
            MouseWheelAction?.Invoke(sender, e);

        }

        private void Control_KeyDown(object sender, KeyEventArgs e)
        {
            KeyDownAction?.Invoke(sender, e);
        }

        private void Control_MouseDown(object sender, MouseEventArgs e)
        {
            MouseDownAction?.Invoke(sender, e);
        }

        private void WrapGlControl_MouseUp(object sender, MouseEventArgs e)
        {
            MouseUpAction?.Invoke(sender, e);

        }

        private void WrapGlControl_MouseMove(object sender, MouseEventArgs e)
        {
            MouseMoveAction?.Invoke(sender, e);

        }
        public Action<object, EventArgs> SizeChangedAction;
        public Action<object, MouseEventArgs> MouseMoveAction;
        public Action<object, MouseEventArgs> MouseUpAction;
        public Action<object, MouseEventArgs> MouseDownAction;
        public Action<object, MouseEventArgs> MouseWheelAction;
        public Action<object, KeyEventArgs> KeyUpUpAction;
        public Action<object, KeyEventArgs> KeyDownAction;

        /*public Bitmap Image
        {
            get { return (Bitmap)Control.Image; }
            set { Control.Image = value; }
        }*/

        public System.Drawing.Point PointToClient(System.Drawing.Point position)
        {
            return Control.PointToClient(position);
        }
    }

}
