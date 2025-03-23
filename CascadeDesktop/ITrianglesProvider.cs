using System.Collections.Generic;

namespace CascadeDesktop
{
    public interface ITrianglesProvider
    {
        IEnumerable<TriangleInfo> GetTriangles();
        bool Visible { get; }
    }
}
