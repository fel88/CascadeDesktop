using OpenTK;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CascadeDesktop
{
    public class Camera
    {       
        public Vector3d Dir
        {
            get { return (CameraFrom - CameraTo).Normalized(); }
        }
        public double DirLen
        {
            get { return Dir.Length; }
        }

        public Vector3d CameraFrom = new Vector3d(250, 250, 250);
        
        public Vector3d CameraTo = new Vector3d(0, 0, 0);
     
        public Vector3d CameraUp = new Vector3d(0, 0, 1);
     

        public Matrix4d ProjectionMatrix { get; set; }
        public Matrix4d ViewMatrix { get; set; }
        public int[] viewport = new int[4];

        public void MoveForw(float ang)
        {
            var vect = CameraFrom - CameraTo;
            CameraTo += new Vector3d(ang, 0, 0);
            CameraFrom = vect + CameraTo;
        }

        public void RotateFromZ(float ang)
        {
            var vect = CameraFrom - CameraTo;
            var m = Matrix4d.CreateFromAxisAngle(CameraUp, ang);
            CameraFrom = Vector3d.TransformVector(vect, m) + CameraTo;
            CameraUp = Vector3d.TransformVector(CameraUp, m);
        }

        public void RotateFromX(float ang)
        {
            var vect = CameraFrom - CameraTo;
            var m = Matrix4d.CreateFromAxisAngle(Vector3d.UnitX, ang);

            CameraFrom = Vector3d.TransformVector(vect, m) + CameraTo;
            CameraUp = Vector3d.TransformVector(CameraUp, m);
        }

        public void RotateFromY(float ang)
        {
            var vect = CameraFrom - CameraTo;

            var cross1 = Vector3d.Cross(vect, CameraUp);
            var m = Matrix4d.CreateFromAxisAngle(cross1, ang);
            //var m = Matrix4.CreateRotationY(ang);

            CameraFrom = Vector3d.TransformVector(vect, m) + CameraTo;
            CameraUp = Vector3d.TransformVector(CameraUp, m);
        }

        public float zoom = 1;

        public float ZNear = -25e3f;
        public float ZFar = 25e3f;

        public bool IsOrtho { get; set; } = false;
        public double OrthoWidth { get; set; } = 1000;
        public float Fov { get; set; } = 60;

        public void UpdateMatricies(GLControl glControl)
        {

            viewport[0] = 0;
            viewport[1] = 0;
            viewport[2] = glControl.Width;
            viewport[3] = glControl.Height;
            var aspect = glControl.Width / (float)glControl.Height;
            var o = Matrix4d.CreateOrthographic(OrthoWidth, OrthoWidth / aspect, ZNear, ZFar);

            Matrix4d mp = Matrix4d.CreatePerspectiveFieldOfView((float)(Fov * System.Math.PI / 180) * zoom,
                glControl.Width / (float)glControl.Height, 1, 25e4f);


            if (IsOrtho)
            {
                ProjectionMatrix = o;

            }
            else
            {
                ProjectionMatrix = mp;
            }

            Matrix4d modelview = Matrix4d.LookAt(CameraFrom, CameraTo, CameraUp);
            ViewMatrix = modelview;
        }
        public void Setup(GLControl glControl)
        {
            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            viewport[0] = 0;
            viewport[1] = 0;
            viewport[2] = glControl.Width;
            viewport[3] = glControl.Height;
            var aspect = glControl.Width / (float)glControl.Height;
            var o = Matrix4d.CreateOrthographic(OrthoWidth, OrthoWidth / aspect, ZNear, ZFar);

            Matrix4d mp = Matrix4d.CreatePerspectiveFieldOfView((float)(Fov * Math.PI / 180) * zoom,
                glControl.Width / (float)glControl.Height, 1, 25e4f);

            GL.MatrixMode(MatrixMode.Projection);
            if (IsOrtho)
            {
                ProjectionMatrix = o;
                GL.LoadMatrix(ref o);
            }
            else
            {
                ProjectionMatrix = mp;
                GL.LoadMatrix(ref mp);
            }

            Matrix4d modelview = Matrix4d.LookAt(CameraFrom, CameraTo, CameraUp);
            //modelview = WorldMatrix * modelview;
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);
            ViewMatrix = modelview;

            GL.MultMatrix(ref WorldMatrix);
        }

        public Matrix4d WorldMatrix = Matrix4d.Identity;

        public void Shift(Vector3d vector3)
        {
            CameraFrom += vector3;
            CameraTo += vector3;
        }

        public Vector3d GetSide()
        {
            var dirr = CameraFrom - CameraTo;
            var forw = new Vector3d(dirr.X, dirr.Y, 0);
            forw.Normalize();
            var crs = Vector3d.Cross(forw, CameraUp);
            var side = new Vector3d(crs.X, crs.Y, 0);
            side.Normalize();
            return side;
        }

        public void FitToPoints(Vector3d[] pnts, int w, int h)
        {
            List<Vector2d> vv = new List<Vector2d>();
            foreach (var vertex in pnts)
            {
                var p = MouseRay.Project(new Vector3d((float)vertex.X, (float)vertex.Y, (float)vertex.Z), ProjectionMatrix, ViewMatrix, WorldMatrix, viewport);
                vv.Add(p.Xy);
            }

            //prjs->xy coords
            var minx = vv.Min(z => z.X);
            var maxx = vv.Max(z => z.X);
            var miny = vv.Min(z => z.Y);
            var maxy = vv.Max(z => z.Y);


            var dx = (maxx - minx);
            var dy = (maxy - miny);

            var cx = dx / 2;
            var cy = dy / 2;
            var dir = CameraTo - CameraFrom;
            //center back to 3d

            var mr = new MouseRay(cx + minx, cy + miny, this);
            var v0 = mr.Start;

            CameraFrom = v0;
            CameraTo = CameraFrom + dir;

            var aspect = w / (float)(h);

            dx /= w;
            dx *= OrthoWidth;
            dy /= h;
            dy *= OrthoWidth;

            OrthoWidth = Math.Max(dx, dy);
        }

    }
}
