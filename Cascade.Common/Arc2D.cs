using System;
using System.Runtime.InteropServices;

namespace Cascade.Common
{
    public class Arc2d : BlueprintItem
    {
        public Vertex2D Middle { get; set; }
        public Vertex2D Center { get; set; }
        public double AngleSweep { get; set; }
        public double Radius { get; set; }
        public double AngleStart { get; set; }
        
        public bool CCW { get; set; }
            

        public void UpdateMiddle()
        {            
            var ang = AngleStart * Math.PI / 180f;
            var xx = Center.X + Radius * Math.Cos(ang);
            var yy = Center.Y - Radius * Math.Sin(ang);

            Start = new Vertex2D(xx, yy);

            ang = (AngleStart + AngleSweep) * Math.PI / 180f;
            xx = Center.X + Radius * Math.Cos(ang);
            yy = Center.Y - Radius * Math.Sin(ang);
            End = new Vertex2D(xx, yy);

            ang = (AngleStart + AngleSweep/2) * Math.PI / 180f;
            xx = Center.X + Radius * Math.Cos(ang);
            yy = Center.Y - Radius * Math.Sin(ang);
            Middle = new Vertex2D(xx, yy);
            if (CCW)
            {
                var temp = Start;
                Start = End;
                End = temp;
            }
        }
    }
}
