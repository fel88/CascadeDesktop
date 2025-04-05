using CascadeDesktop.Interfaces;
using OpenTK;
using System;
using System.Drawing;
using System.Text;

namespace CascadeDesktop.Interfaces
{
    public interface IHelperItem
    {
        string Name { get; set; }
        bool PickEnabled { get; set; }
        int ZIndex { get; set; }

        bool Selected { get; set; }
        bool Visible { get; set; }
        void Draw(IDrawingContext gr);
        Action Changed { get; set; }
        void Shift(Vector2d vector);

        void ClearSelection();

        RectangleF? BoundingBox();

        void AppendToXml(StringBuilder sb);
    }
}
