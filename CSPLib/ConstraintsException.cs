using System;

namespace CSPLib
{
    public class ConstraintsException : Exception
    {
        public ConstraintsException(string msg) : base(msg) { }
    }
}
