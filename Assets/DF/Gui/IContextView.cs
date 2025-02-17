using System;

namespace DF.Gui
{
    public interface IContextView<T> : IDisposable
    {
        T Context { get; }
        void Setup(T context);
    }
}