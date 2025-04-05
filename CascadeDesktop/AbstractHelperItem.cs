using CascadeDesktop.Interfaces;
using OpenTK;
using System;
using System.Drawing;
using System.Text;

namespace CascadeDesktop
{
    public abstract class AbstractHelperItem : IHelperItem
    {
        public int ZIndex { get; set; }
        public bool PickEnabled { get; set; } = true;
        public Action Changed { get; set; }
        public virtual void Shift(Vector2d vector) { }

        public abstract void Draw(IDrawingContext gr);
        bool _selected;
        public bool Selected
        {
            get => _selected;
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    SelectedChanged?.Invoke();
                }
            }
        }

        public virtual AbstractHelperItem Clone()
        {
            throw new NotImplementedException();
        }

        public virtual void ClearSelection()
        {
            Selected = false;
        }

        public event Action SelectedChanged;
        public string Name { get; set; }
        public string TypeName => GetType().Name;
        public bool Visible { get; set; } = true;

        public abstract RectangleF? BoundingBox();

        public virtual void AppendToXml(StringBuilder sb) { }
    }
}
