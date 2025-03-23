
namespace Cascade.Common
{
    public class BlueprintItem3d
    {
        public Vertex Start { get; set; }
        public Vertex End { get; set; }

        public void Reverse()
        {
            (Start, End) = (End, Start);
        }
    }
}
