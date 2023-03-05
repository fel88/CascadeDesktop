namespace Cascade.Common
{
    public class Arc : BlueprintItem
    {
        public Vertex Center;
        public Vertex Axis;
        public Vertex Dir0;
        public double AngleSweep;
        public double AngleStart;
        public bool CCW;
    }
}
