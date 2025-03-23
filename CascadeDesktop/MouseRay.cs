using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace CascadeDesktop
{
    public class MouseRay
    {
        public Vector3d _start;
        public Vector3d _end;

        public Vector3d Start { get { return _start; } }

        public Vector3d End { get { return _end; } }

        public MouseRay(Point mouse)
            : this(mouse.X, mouse.Y)
        {
        }

        public MouseRay(Vector3d start, Vector3d end)
        {
            _start = start;
            _end = end;

        }
        public static int[] viewport = new int[4];
        public static Matrix4d modelMatrix, projMatrix;

        public static void UpdateMatrices()
        {
            GL.GetDouble(GetPName.ModelviewMatrix, out modelMatrix);
            GL.GetDouble(GetPName.ProjectionMatrix, out projMatrix);
            GL.GetInteger(GetPName.Viewport, viewport);
        }

        public Vector3d Dir
        {
            get { return (End - Start).Normalized(); }
        }

        public MouseRay(double x, double y, Camera view)
        {
            int[] viewport = new int[4];
            Matrix4d modelMatrix, projMatrix;
            viewport = view.viewport;
            modelMatrix = view.ViewMatrix;
            projMatrix = view.ProjectionMatrix;


            _start = UnProject(new Vector3d(x, y, 0.0f), projMatrix, modelMatrix, new Size(viewport[2], viewport[3]));
            _end = UnProject(new Vector3d(x, y, 1.0f), projMatrix, modelMatrix, new Size(viewport[2], viewport[3]));
        }

        public MouseRay(int x, int y)
        {

            _start = UnProject(new Vector3d(x, y, 0.0f), projMatrix, modelMatrix, new Size(viewport[2], viewport[3]));
            _end = UnProject(new Vector3d(x, y, 1.0f), projMatrix, modelMatrix, new Size(viewport[2], viewport[3]));
        }


        private static bool WithinEpsilon(double a, double b)
        {
            double num = a - b;
            return ((-1.401298E-45f <= num) && (num <= double.Epsilon));
        }

        public static Vector3d Project(Vector3d _source, Matrix4d projection, Matrix4d view, Matrix4d world, int[] viewport)
        {
            Vector4d source = new Vector4d(_source, 1);

            var w = viewport[2];
            var h = viewport[3];
            int x = 0;
            int y = 0;

            Matrix4d matrix = Matrix4d.Mult(Matrix4d.Mult(world, view), projection);
            Vector4d vector = Vector4d.Transform(source, matrix);
            var a = (((source.X * matrix.M14) + (source.Y * matrix.M24)) + (source.Z * matrix.M34)) + matrix.M44;
            if (!WithinEpsilon(a, 1f))
            {
                vector = (vector / a);
            }
            vector.X = (((vector.X + 1f) * 0.5f) * w) + x;
            vector.Y = (((-vector.Y + 1f) * 0.5f) * h) + y;
            //vector.Z = (vector.Z * (this.MaxDepth - this.MinDepth)) + this.MinDepth;
            return vector.Xyz;
        }

        public static OpenTK.Vector3? Project(OpenTK.Vector3 v)
        {
            float objx = v.X;
            float objy = v.Y;
            float objz = v.Z;

            float[] modelview = new float[16];
            float[] projection = new float[16];
            int[] viewport = new int[4];
            GL.GetFloat(GetPName.ModelviewMatrix, modelview);
            GL.GetFloat(GetPName.ProjectionMatrix, projection);
            GL.GetInteger(GetPName.Viewport, viewport);
            // Transformation vectors
            float[] fTempo = new float[8];
            // Modelview transform
            fTempo[0] = modelview[0] * objx + modelview[4] * objy + modelview[8] * objz + modelview[12]; // w is always 1
            fTempo[1] = modelview[1] * objx + modelview[5] * objy + modelview[9] * objz + modelview[13];
            fTempo[2] = modelview[2] * objx + modelview[6] * objy + modelview[10] * objz + modelview[14];
            fTempo[3] = modelview[3] * objx + modelview[7] * objy + modelview[11] * objz + modelview[15];
            // Projection transform, the final row of projection matrix is always [0 0 -1 0]
            // so we optimize for that.
            fTempo[4] = projection[0] * fTempo[0] + projection[4] * fTempo[1] + projection[8] * fTempo[2] + projection[12] * fTempo[3];
            fTempo[5] = projection[1] * fTempo[0] + projection[5] * fTempo[1] + projection[9] * fTempo[2] + projection[13] * fTempo[3];
            fTempo[6] = projection[2] * fTempo[0] + projection[6] * fTempo[1] + projection[10] * fTempo[2] + projection[14] * fTempo[3];
            fTempo[7] = -fTempo[2];
            // The result normalizes between -1 and 1
            if (fTempo[7] == 0.0) // The w value
                return null;
            fTempo[7] = 1.0f / fTempo[7];
            // Perspective division
            fTempo[4] *= fTempo[7];
            fTempo[5] *= fTempo[7];
            fTempo[6] *= fTempo[7];
            // Window coordinates
            // Map x, y to range 0-1
            OpenTK.Vector3 ret;
            ret.X = (fTempo[4] * 0.5f + 0.5f) * viewport[2] + viewport[0];
            ret.Y = (fTempo[5] * 0.5f + 0.5f) * viewport[3] + viewport[1];
            // This is only correct when glDepthRange(0.0, 1.0)
            ret.Z = (1.0f + fTempo[6]) * 0.5f;  // Between 0 and 1
            return ret;
        }


        public static Vector3d UnProject(Vector3d mouse, Matrix4d projection, Matrix4d view, Size viewport)
        {
            Vector4d vec;

            vec.X = 2.0f * mouse.X / viewport.Width - 1;
            vec.Y = -(2.0f * mouse.Y / viewport.Height - 1);
            vec.Z = mouse.Z;
            vec.W = 1.0f;

            Matrix4d viewInv = Matrix4d.Invert(view);
            Matrix4d projInv = Matrix4d.Invert(projection);

            Vector4d.Transform(ref vec, ref projInv, out vec);
            Vector4d.Transform(ref vec, ref viewInv, out vec);

            if (vec.W > 0.000001f || vec.W < -0.000001f)
            {
                vec.X /= vec.W;
                vec.Y /= vec.W;
                vec.Z /= vec.W;
            }

            return vec.Xyz;
        }

    }
}
