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
                return proj;

            }
            return null;
        }

        public static Vector3d? GetAdjointFacesShift(CylinderSurfInfo cylinder1, CylinderSurfInfo cylinder2, float eps = 1e-8f)
        {
            var cross1 = Vector3d.Cross(cylinder1.Axis.ToVector3d(), cylinder2.Axis.ToVector3d());
            if (Math.Abs(cross1.Length) < eps)
            {//colinear
             //just translate

                var temp = new Plane() { Location = cylinder1.Position.ToVector3d(), Normal = cylinder1.Axis.ToVector3d() };
                var proj = temp.GetProjPoint(cylinder2.COM.ToVector3d());
                var proj2 = temp.GetProjPoint(cylinder1.COM.ToVector3d());
                proj -= proj2;//shift
                return proj;
            }
            return null;
        }
    }
}
