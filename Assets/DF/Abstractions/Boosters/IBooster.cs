using System.Collections.Generic;
using DF.Models.Boosters;

namespace DF.Abstractions.Boosters
{
    public interface IBooster
    {
        string Id { get; }
        float Duration { get; }
        IReadOnlyList<BuffDefinition> GetAllBuffDefinitions();
        
        void Apply();
        void Cancel();
    }
}