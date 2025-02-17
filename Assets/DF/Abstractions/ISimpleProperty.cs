using System;

namespace DF.Abstractions
{
    public interface ISimpleProperty<TValue, in TGetKey, TSetKey>
    {
        event Action<TSetKey> ValueChanged; 

        TValue Get(TGetKey key);
        bool Set(TSetKey key, TValue value);
    }
}