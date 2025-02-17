using UnityEngine;

namespace DF.Gui
{
    public abstract class ContextView<T> : MonoBehaviour, IContextView<T>
    {
        protected bool AlreadyInitialized;
        public T Context { get; protected set; }

        public virtual void Setup(T context)
        {
            Context = context;
			
            if (!AlreadyInitialized)
            {
                Init();
                AlreadyInitialized = true;
            }
			
            UpdateView();
        }

        public abstract void UpdateView();
        protected virtual void Init() { }

        public virtual void Dispose()
        {
        }
    }
}