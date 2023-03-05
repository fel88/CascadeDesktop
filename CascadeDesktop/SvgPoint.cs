namespace CascadeDesktop
{
    public class SvgPoint
    {
        public bool exact = true;
        public override string ToString()
        {
            return "x: " + X + "; y: " + Y;
        }
        public int id;
        public SvgPoint(double _x, double _y)
        {
            X = _x;
            Y = _y;
        }
        public bool marked;
        public double X;
        public double Y;
    }
}
