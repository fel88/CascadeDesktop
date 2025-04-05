using System;
using CSPLib.Interfaces;

namespace CSPLib
{
    public interface ICommand
    {
        string Name { get; }
        Action<IDrawable, object> Process { get; }
    }
}
