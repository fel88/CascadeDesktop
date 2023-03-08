namespace Cascade.Common
{
    public class Arc3d : BlueprintItem3d
    {
        public Vertex Center;
        public Vertex Axis;
        public Vertex Dir0;
        public double AngleSweep;
        public double AngleStart;
        public bool CCW;
        public double Radius;
    }
}
