using OpenTK;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace CascadeDesktop.Interfaces
{
    public interface IPointsProvider
    {
        IEnumerable<Vector3d> GetPoints();
        bool Visible { get; }
    }
}
