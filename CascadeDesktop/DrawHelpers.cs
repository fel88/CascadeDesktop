using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace CascadeDesktop
{
    public static class DrawHelpers
    {
        public static void DrawCross(Vector3d pos, double g, bool beginEnd = true, bool is3d = true)
        {
            if (beginEnd)
            {
                GL.Begin(PrimitiveType.Lines);
            }

            GL.Vertex3(pos.X, pos.Y - g, pos.Z);
            GL.Vertex3(pos.X, pos.Y + g, pos.Z);
            if (is3d)
            {
                GL.Vertex3(pos.X, pos.Y, pos.Z - g);
                GL.Vertex3(pos.X, pos.Y, pos.Z + g);
            }

            GL.Vertex3(pos.X + g, pos.Y, pos.Z);
            GL.Vertex3(pos.X - g, pos.Y, pos.Z);
            if (beginEnd)
            {
                GL.End();
            }
        }

        internal static void DrawCube(Vector3d pos, float g)
        {
            GL.Begin(PrimitiveType.Lines);
            for (int i = -1; i <= 1; i += 2)
            {
                GL.Vertex3(pos.X - g, pos.Y - g, pos.Z + g * i);
                GL.Vertex3(pos.X - g, pos.Y + g, pos.Z + g * i);
                GL.Vertex3(pos.X + g, pos.Y - g, pos.Z + g * i);
                GL.Vertex3(pos.X + g, pos.Y + g, pos.Z + g * i);

                GL.Vertex3(pos.X - g, pos.Y - g, pos.Z + g * i);
                GL.Vertex3(pos.X + g, pos.Y - g, pos.Z + g * i);
                GL.Vertex3(pos.X - g, pos.Y + g, pos.Z + g * i);
                GL.Vertex3(pos.X + g, pos.Y + g, pos.Z + g * i);
            }
            for (int i = -1; i <= 1; i += 2)
            {
                GL.Vertex3(pos.X - g, pos.Y + g * i, pos.Z - g);
                GL.Vertex3(pos.X - g, pos.Y + g * i, pos.Z + g);
                GL.Vertex3(pos.X + g, pos.Y + g * i, pos.Z - g);
                GL.Vertex3(pos.X + g, pos.Y + g * i, pos.Z + g);

                GL.Vertex3(pos.X - g, pos.Y + g * i, pos.Z - g);
                GL.Vertex3(pos.X + g, pos.Y + g * i, pos.Z - g);
                GL.Vertex3(pos.X - g, pos.Y + g * i, pos.Z + g);
                GL.Vertex3(pos.X + g, pos.Y + g * i, pos.Z + g);
            }
            GL.End();
        }
    }
}
