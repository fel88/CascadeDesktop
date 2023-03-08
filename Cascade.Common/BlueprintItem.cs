using System;

namespace Cascade.Common
{
    public class BlueprintItem
    {
        public Vertex2D Start { get; set; }
        public Vertex2D End { get; set; }

        public virtual void Reverse()
        {
            var temp = End;
            End = Start;
            Start = temp;
        }
    }
}
