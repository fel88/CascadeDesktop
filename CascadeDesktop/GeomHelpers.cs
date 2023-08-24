using OpenTK;
using System;

namespace CascadeDesktop
{
    public class GeomHelpers
    {
        public static Vector3d? GetAdjointFacesShift(PlaneSurfInfo plane1, PlaneSurfInfo plane2, float eps = 1e-8f)
        {
            var cross1 = Vector3d.Cross(plane1.Normal.ToVector3d(), plane2.Normal.ToVector3d());
            if (Math.Abs(cross1.Length) < eps)
            {//colinear
             //just translate

                var temp = new Plane() { Location = plane2.Position.ToVector3d(), Normal = plane2.Normal.ToVector3d() };
                var proj = temp.GetProjPoint(plane1.Position.ToVector3d());
                proj = proj - plane1.Position.ToVector3d();//shift
                if (proj.Length > eps)
                {
                    return proj;
                }
            }
            return null;
        }
    }
}
