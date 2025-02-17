using System.Collections.Generic;

namespace DF.Abstractions
{
    public interface IDefinitionsProvider<T>
    {
        T GetDefinition(string id);
        IReadOnlyDictionary<string, T> GetAllDefinitions();
    }
}