using CascadeDesktop.Interfaces;
using OpenTK;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace CascadeDesktop
{
    public class ArcElement : IElement
    {
        public ArcElement(bool isFullCircle)
        {
            IsCircle = isFullCircle;
        }

        public Vector2d Center { get; set; }

        public Vector2d Start { get; set; }
        public Vector2d End { get; set; }
        public double Radius { get; set; }
        /// <summary>
        /// Sweep angle (radians)
        /// </summary>
        public double SweepAngle { get; set; }
        public bool CCW { get; set; }
        public bool IsCircle { get; private set; }
        public double Length => IsCircle ? 2 * Math.PI * Radius : SweepAngle * Radius;

        public IElement Clone()
        {
            return new ArcElement(IsCircle)
            {
                Center = Center,
                End = End,
                Start = Start,
                Radius = Radius,
                SweepAngle = SweepAngle,
                CCW = CCW
            };
        }

        public IEnumerable<Vector2d> GetPoints(double eps = 1E-05)
        {
            if (IsCircle)
            {
                for (int i = 0; i <= 360; i += 15)
                {
                    var ang = i * Math.PI / 180f;
                    var xx = Center.X + Radius * Math.Cos(ang);
                    var yy = Center.Y + Radius * Math.Sin(ang);
                    yield return new Vector2d(xx, yy);
                }
                yield break;
            }
            yield return Start;

            /*for (double ang = s; i < Radius; i++) { 
}*/

            yield return End;
        }

        public void Reverse()
        {
            CCW = !CCW;
            var temp = Start;
            Start = End;
            End = temp;
        }
    }
}