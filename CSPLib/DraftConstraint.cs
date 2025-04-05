using System;
using System.IO;

namespace CSPLib
{
    public abstract class DraftConstraint
    {
        public Draft Parent { get; private set; }
        public DraftConstraint(Draft parent)
        {
            Parent = parent;
            Id = FactoryHelper.NewId++;
        }
        public int Id { get; set; }
        public abstract bool IsSatisfied(float eps = 1e-6f);
        public abstract void RandomUpdate(ConstraintSolverContext ctx);
        public bool Enabled { get; set; } = true;
        public Action BeforeChanged;
        public abstract bool ContainsElement(DraftElement de);

        internal abstract void Store(TextWriter writer);
    }
}
