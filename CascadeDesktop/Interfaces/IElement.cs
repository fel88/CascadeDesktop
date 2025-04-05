using OpenTK;
using System.Collections.Generic;
using System.Drawing;

namespace CascadeDesktop.Interfaces
{
    public interface IElement
    {
        IElement Clone();
        void Reverse();
        IEnumerable<Vector2d> GetPoints(double eps = 0.00001);

        Vector2d Start { get; }
        Vector2d End { get; }
        double Length { get; }
    }
}
