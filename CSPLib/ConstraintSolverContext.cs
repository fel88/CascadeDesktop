using System.Collections.Generic;

namespace CSPLib
{
    public class ConstraintSolverContext
    {
        public ConstraintSolverContext Parent;
        public List<ConstraintSolverContext> Childs = new List<ConstraintSolverContext>();
        public List<DraftPoint> FreezedPoints = new List<DraftPoint>();
        public List<TopologyDraftLineInfo> FreezedLinesDirs = new List<TopologyDraftLineInfo>();
    }
}
