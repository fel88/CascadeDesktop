using OpenTK;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace CascadeDesktop
{
    public class LocalContour
    {
        public LocalContour Parent;
        public List<LocalContour> Childrens = new List<LocalContour>();

        public float Len
        {
            get
            {
                float len = 0;
                for (int i = 1; i <= Points.Count; i++)
                {
                    var p1 = Points[i - 1];
                    var p2 = Points[i % Points.Count];
                    len += (float)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
                }
                return len;
            }
        }
        public List<Vector2d> Points = new List<Vector2d>();
        public bool Enable = true;
    }
}
