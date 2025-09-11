using OpenTK;
using OpenTK.Mathematics;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CascadeDesktop
{
    public class DefaultCameraViewManager : CameraViewManager
    {
        public override void Update()
        {
            var dir = Camera.CamFrom - Camera.CamTo;
            var cv = dir;
            var moveVec = new Vector3d(cv.X, cv.Y, cv.Z).Normalized();
            var a1 = Vector3d.Cross(Camera.CamUp, cv.Normalized()); ;
            //var moveVecTan = new Vector3(-moveVec.Y, moveVec.X, );
            var moveVecTan = a1.Normalized();
            moveVec = Vector3d.Cross(a1.Normalized(), cv.Normalized()).Normalized();


            var pos = CursorPosition;
            var zoom = 360f / (Camera.CamFrom - Camera.CamTo).Length;

            {
                if (drag2)
                {


                    zoom = Control.Width / Camera.OrthoWidth;

                    var dx = moveVecTan * ((startPosX - pos.X) / zoom) + moveVec * ((startPosY - pos.Y) / zoom);
                    Camera.CamFrom = cameraFromStart + dx;
                    Camera.CamTo = cameraToStart + dx;
                }
                if (drag)
                {
                    //rotate here
                    float kk = 3;
                    //cameraToStart = new Vector3(cameraToStart.X, 0, 0);
                    //Camera.CamTo = cameraToStart;
                    Vector3d v1 = cameraFromStart - cameraToStart;

                    var m1 = Matrix4d.CreateFromAxisAngle(Vector3d.Cross(v1, cameraUpStart), -(startPosY - pos.Y) / 180f / kk * (float)Math.PI);
                    //var m1 = Matrix3.CreateFromAxisAngle(Vector3.UnitX, -(startPosY - pos.Y) / 180f / kk * (float)Math.PI);
                    var m2 = Matrix4d.CreateFromAxisAngle(cameraUpStart, -(startPosX - pos.X) / 180f / kk * (float)Math.PI);
                    //var m2 = Matrix3.CreateFromAxisAngle(Vector3.UnitZ, -(startPosX - pos.X) / 180f / kk * (float)Math.PI);

                    v1 = Vector3d.TransformVector(v1, m1);
                    //v1 *= m1;
                    v1 = Vector3d.TransformVector(v1, m2);
                    //v1 *= m2;

                    var up1 = cameraUpStart;

                    //up1 *= m1;
                    //up1 *= m2;
                    Camera.CamUp = up1;

                    Camera.CamFrom = cameraToStart + v1;
                    var dx = startPosX - pos.X;

                }

            }
        }

        public float AlongRotate = 0;
        public Camera Camera;
        public override void Attach(EventWrapperGlControl control, Camera camera)
        {
            base.Attach(control, camera);
            Camera = camera;
            control.MouseUpAction = Control_MouseUp;
            control.MouseDownAction = Control_MouseDown;
            control.KeyUpUpAction = Control_KeyUp;
            control.KeyDownAction = Control_KeyDown;
            control.MouseWheelAction = Control_MouseWheel;
        }

        private void Control_MouseWheel(object sender, MouseEventArgs e)
        {
            float zoomK = 20;
            var cur = Control.PointToClient(Cursor.Position);
            Control.MakeCurrent();
            //MouseRay.UpdateMatrices();
            MouseRay mr = new MouseRay(cur.X, cur.Y, Camera);
            //MouseRay mr0 = new MouseRay(Control.Width / 2, Control.Height / 2, Camera);

            var camera = Camera;
            if (camera.IsOrtho)
            {
                var shift = mr.Start - Camera.CamFrom;
                shift.Normalize();
                //var old = camera.OrthoWidth / Control.Width;
                if (e.Delta > 0)
                {
                    camera.OrthoWidth /= 1.2f;
                    ////var pxn = new Vector2(cur.X,cur.Y)-(new Vector2(Control.Width/2,Control.Height/2));
                    //Camera cam2 = new Camera();
                    //cam2.CamFrom = camera.CamFrom;
                    //cam2.CamTo = camera.CamTo;
                    //cam2.CamUp = camera.CamUp;
                    //cam2.OrthoWidth = camera.OrthoWidth;
                    //cam2.IsOrtho = camera.IsOrtho;

                    //cam2.UpdateMatricies(Control);
                    //MouseRay mr2 = new MouseRay(cur.X, cur.Y, cam2);

                    ////var a1 = pxn * camera.OrthoWidth / Control.Width;
                    //var diff = mr.Start - mr2.Start;


                    //shift *= diff.Length;
                    //camera.CamFrom += shift;
                    //camera.CamTo += shift;
                }
                else
                {
                    camera.OrthoWidth *= 1.2f;
                }
                /*var pxn = new Vector2(cur.X, cur.Y) - (new Vector2(Control.Width / 2, Control.Height / 2));

                var a1 = pxn * camera.OrthoWidth / Control.Width;*/
                Camera cam2 = new Camera();
                cam2.CamFrom = camera.CamFrom;
                cam2.CamTo = camera.CamTo;
                cam2.CamUp = camera.CamUp;
                cam2.OrthoWidth = camera.OrthoWidth;
                cam2.IsOrtho = camera.IsOrtho;

                cam2.UpdateMatricies(Control);
                MouseRay mr2 = new MouseRay(cur.X, cur.Y, cam2);

                var diff = mr.Start - mr2.Start;
                shift *= diff.Length;
                if (e.Delta > 0)
                {
                    camera.CamFrom += shift;
                    camera.CamTo += shift;
                }
                else
                {
                    camera.CamFrom -= shift;
                    camera.CamTo -= shift;
                }

                return;
            }
            if (
                Control.ClientRectangle.IntersectsWith(new System.Drawing.Rectangle(Control.PointToClient(Cursor.Position),
                    new System.Drawing.Size(1, 1))))
            {
                var dir = mr.Dir;
                dir.Normalize();
                if (e.Delta > 0)
                {
                    camera.CamFrom += dir * zoomK;
                    camera.CamTo += dir * zoomK;
                }
                else
                {
                    camera.CamFrom -= dir * zoomK;
                    camera.CamTo -= dir * zoomK;
                }
            }
        }

        private void Control_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Shift)
            {
                lshift = true;
            }
        }

        private void Control_KeyUp(object sender, KeyEventArgs e)
        {
            lshift = false;
        }

        protected bool lshiftcmd = false;
        public static Vector3d? lineIntersection(Vector3d planePoint, Vector3d planeNormal, Vector3d linePoint, Vector3d lineDirection)
        {
            if (Math.Abs(Vector3d.Dot(planeNormal, lineDirection)) < 10e-6f)
            {
                return null;
            }

            var dot1 = Vector3d.Dot(planeNormal, planePoint);
            var dot2 = Vector3d.Dot(planeNormal, linePoint);
            var dot3 = Vector3d.Dot(planeNormal, lineDirection);
            double t = (dot1 - dot2) / dot3;
            return linePoint + lineDirection * (float)t;
        }

        public bool SnapMode = false;
        public bool SnapModePlane = false;
        public virtual void Control_MouseDown(object sender, MouseEventArgs e)
        {
            Control.MakeCurrent();

            var pos = CursorPosition;
            startPosX = pos.X;
            startPosY = pos.Y;
            cameraFromStart = Camera.CamFrom;
            cameraToStart = Camera.CamTo;
            cameraUpStart = Camera.CamUp;

            if (e.Button == MouseButtons.Right)
            {

                var mr = new MouseRay(pos.X, pos.Y, Camera);
                var d1 = Camera.CamFrom - Camera.CamTo;
                //var plane1 : forw
                var crs1 = Vector3d.Cross(cameraUpStart, d1);
                var z1 = Vector3d.UnitZ;
                if (SnapModePlane)
                {

                    var inter = lineIntersection(Vector3d.Zero, Vector3d.UnitZ, Camera.CamFrom, Camera.CamTo - Camera.CamFrom);
                    if (inter != null)
                    {
                        drag = true;
                        //var shift = Camera.CamTo - inter.Value;
                        var dl = Camera.DirLen;
                        bool fixedLen = false;
                        if (fixedLen)
                        {
                            var shift2 = Camera.CamFrom - Camera.CamTo;
                            Camera.CamTo = inter.Value;
                            Camera.CamFrom = Camera.CamTo + shift2;
                            cameraToStart = Camera.CamTo;
                            cameraFromStart = Camera.CamFrom;
                        }
                        else
                        {
                            Camera.CamTo = inter.Value;
                            cameraToStart = Camera.CamTo;
                        }

                    }
                }
                else if (SnapMode)
                {



                    var inter = lineIntersection(Camera.CamTo, crs1, Vector3d.Zero, Vector3d.UnitX);
                    if (inter != null)
                    {
                        drag = true;
                        Camera.CamTo = inter.Value;
                        cameraToStart = Camera.CamTo;
                    }
                }
                else
                {
                    drag = true;
                }


                lshiftcmd = lshift;
            }

            if (e.Button == MouseButtons.Left)
            {
                drag2 = true;
                //Camera.CamTo=Drawer.tubec
            }
        }

        protected bool lshift = false;
        protected float startShiftX;
        protected float startShiftY;
        protected float startPosX;
        protected float startPosY;
        protected Vector3d cameraFromStart;
        protected Vector3d cameraToStart;
        protected Vector3d cameraUpStart;
        public PointF CursorPosition
        {
            get
            {
                return Control.PointToClient(Cursor.Position);
            }
        }
        protected bool drag = false;
        protected bool drag2 = false;

        private void Control_MouseUp(object sender, MouseEventArgs e)
        {
            drag = false;
            drag2 = false;
        }
    }
}
