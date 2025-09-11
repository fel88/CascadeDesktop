using System;
using System.Collections.Generic;
using OpenTK;
using System.Linq;
using OpenTK.Mathematics;

namespace CSPLib
{
    public class NFP
    {
        public Vector2d[] Points = new Vector2d[] { };
        public List<NFP> Childrens = new List<NFP>();
        public NFP Parent;
        public Vector2d this[int ind]
        {
            get
            {
                return Points[ind];
            }
        }
        public void Shift(Vector2d vector)
        {
            for (int i = 0; i < Points.Length; i++)
            {
                Points[i].X += vector.X;
                Points[i].Y += vector.Y;
            }
        }
        public double SignedArea()
        {
            return GeometryUtils.signed_area(Points);
        }

        public int Length
        {
            get
            {
                return Points.Length;
            }
        }

        public void Reverse()
        {
            Points = Points.Reverse().ToArray();
        }
    }
}
