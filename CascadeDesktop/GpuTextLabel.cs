using OpenTK.Mathematics;
using System.Drawing;
using System.Windows.Forms;
using TriangleNet.Topology.DCEL;

namespace CascadeDesktop
{
    public class GpuTextLabel
    {
        public void Draw(GpuDrawingContext ctx)
        {
            if (!Visible)
                return;

            ctx.TextRenderer.RenderText(Text, Position.X, Position.Y, new Vector3()
            {
                X = Color.R / 255.0f,
                Y = Color.G / 255.0f,
                Z = Color.B / 255.0f,
            }, Scale);
        }
        public Color Color = Color.Blue;
        public float Scale = 1;
        public Vector2 Position { get; set; }

        public string Text { get; set; }
        public bool Visible { get; internal set; } = true;
    }
}
