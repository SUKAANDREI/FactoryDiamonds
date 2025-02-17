using System;
using System.Collections.Generic;
using DF.Abstractions.Diamonds;
using DF.Models.SkipTime;

namespace DF.Abstractions.Recyclers
{
    public interface IRecycler : IDisposable
    {
        event Action QueueDiamondsUpdated;
        
        void AddDiamond(IDiamond diamond);
        IReadOnlyCollection<IDiamond> GetAllCurrentDiamonds();
        void SkipTime(List<SkipTimeDiamondParams> skipTimeCollection, double period);
    }
}