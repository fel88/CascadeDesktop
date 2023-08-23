using System.Collections.Generic;

namespace Cascade.Common
{
    public class BlueprintPolyline : BlueprintItem
    {
        public List<Vertex2D> Points = new List<Vertex2D>();
        public override Vertex2D Start => Points[0];
        public override Vertex2D End => Points[Points.Count - 1];
    }
}
