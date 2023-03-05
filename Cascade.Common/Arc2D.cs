namespace Cascade.Common
{
    public class Arc2D : BlueprintItem
    {
        public Vertex2D Center { get; set; }
        public double AngleSweep { get; set; }
        public double Radius { get; set; }
        public double AngleStart { get; set; }
        public bool CCW { get; set; }
    }
}
