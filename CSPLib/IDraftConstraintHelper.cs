using OpenTK;
using CSPLib.Interfaces;

namespace CSPLib
{
    public interface IDraftConstraintHelper : IDraftHelper
    {
        DraftConstraint Constraint { get; }
        Vector2d SnapPoint { get; set; }


    }
}
