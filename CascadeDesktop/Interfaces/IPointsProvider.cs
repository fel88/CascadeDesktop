using OpenTK;
using System.Collections.Generic;

namespace CascadeDesktop.Interfaces
{
    public interface IPointsProvider
    {
        IEnumerable<Vector3d> GetPoints();
        bool Visible { get; }
    }
}
