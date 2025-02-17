using System;
using System.Collections.Generic;
using DF.Models.Boosters;

namespace DF.Abstractions.Boosters
{
    public interface IBoosterController : IDisposable
    {
        event Action<string, bool> BoosterActiveUpdated;
        event Action<IReadOnlyList<BuffType>> BuffsUpdated;
        
        bool IsActive(string id);
        bool Activate(string id);
        bool Deactivate(string id);
        IReadOnlyDictionary<string, IBooster> GetAllActiveBoosters();
        float GetBuffModifier(BuffType buffType);
        IReadOnlyDictionary<BuffType, float> GetAllBuffs();
        double GetTimeLeft(string id);
        void SkipTime();
    }
}