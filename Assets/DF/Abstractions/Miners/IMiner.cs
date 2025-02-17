using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DF.Abstractions.Diamonds;
using DF.Models.Miners;
using DF.Models.SkipTime;

namespace DF.Abstractions.Miners
{
    public interface IMiner : IDisposable
    {
        event Action StateUpdated; 
        string GetId();
        MinerState GetState();
        UniTask<IDiamond> Extract(CancellationToken cancellationToken, ExtractPositionParams extractPositionParams = default);
        ExtractPositionParams SkipTime(ref List<SkipTimeDiamondParams> skipParams, double period);
        ExtractPositionParams ApplyMultipliers(float minerSpeed, float mineDuration);
    }
}