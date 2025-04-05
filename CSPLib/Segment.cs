using OpenTK;

namespace CSPLib
{
    public class Segment
    {
        public override string ToString()
        {
            return "Start: " + Start + "; End: " + End;
        }
        public Vector2d Start;
        public Vector2d End;
        public double Length()
        {
            return (End - Start).Length;
        }
    }
}
