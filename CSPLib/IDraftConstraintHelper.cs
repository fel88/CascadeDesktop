using OpenTK;
using CSPLib.Interfaces;
using OpenTK.Mathematics;

namespace CSPLib
{
    public interface IDraftConstraintHelper : IDraftHelper
    {
        DraftConstraint Constraint { get; }
        Vector2d SnapPoint { get; set; }


    }
}
