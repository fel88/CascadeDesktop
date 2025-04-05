using CascadeDesktop.Interfaces;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Drawing;

namespace CascadeDesktop
{
    public class LineElement : IElement
    {
        public Vector2d Start { get; set; }
        public Vector2d End { get; set; }

        public double Length => (End - Start).Length;

        public IElement Clone()
        {
            return new LineElement()
            {
                Start = End,
                End = Start
            };
        }

        public IEnumerable<Vector2d> GetPoints(double eps = 1E-05)
        {
            return new[]
            {
                Start,
                End
            };
        }

        public void Reverse()
        {
            var temp = Start;
            Start = End;
            End = temp;
        }
    }
}
