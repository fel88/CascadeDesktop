using OpenTK;
using System.Collections.Generic;

namespace CascadeDesktop
{
    public interface IPointsProvider
    {
        IEnumerable<Vector3d> GetPoints();
        bool Visible { get; }
    }
}
