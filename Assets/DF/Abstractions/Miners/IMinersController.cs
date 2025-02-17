using System;
using System.Collections.Generic;
using DF.Abstractions.Diamonds;
using DF.Models.SkipTime;

namespace DF.Abstractions.Miners
{
    public interface IMinersController : IDisposable
    {
        IReadOnlyDictionary<string, IMiner> GetAllMiners();
        void Extract(Action<IDiamond> complete);
        void SkipTime(ref List<SkipTimeDiamondParams> skipParams, double period);
    }
}