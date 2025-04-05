using System.Collections.Generic;

namespace CascadeDesktop.Interfaces
{
    public interface ITrianglesProvider
    {
        IEnumerable<TriangleInfo> GetTriangles();
        bool Visible { get; }
    }
}
