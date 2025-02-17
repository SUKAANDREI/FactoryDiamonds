using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DF.Abstractions.Boosters;
using DF.Abstractions.Diamonds;
using DF.Abstractions.Miners;
using DF.Models.Boosters;
using DF.Models.SkipTime;

namespace DF.Models.Miners
{
    public class MinersController : IMinersController
    {
        private readonly IMinersConstructor _minersConstructor;
        private readonly IBoosterController _boosterController;

        private readonly Dictionary<string, IMiner> _minersMap;
        private Dictionary<string, CancellationTokenSource> _cancellationTokenSources;
        private Action<IDiamond> _extractCompleteCallback;
        
        public MinersController(
            IMinersConstructor minersConstructor, 
            IMinerDefinitionsProvider minerDefinitionsProvider, 
            IBoosterController boosterController)
        {
            _minersConstructor = minersConstructor;
            _boosterController = boosterController;

            var allMinerDefinitions = minerDefinitionsProvider.GetAllDefinitions();
            _minersMap = new Dictionary<string, IMiner>(allMinerDefinitions.Count);
            _cancellationTokenSources = new Dictionary<string, CancellationTokenSource>(allMinerDefinitions.Count);
            InitMiners(allMinerDefinitions);
            Subscribe();
        }

        public IReadOnlyDictionary<string, IMiner> GetAllMiners() => _minersMap;

        public void Extract(Action<IDiamond> complete)
        {
            _extractCompleteCallback = complete;
            foreach (var pair in _minersMap) Extract(pair.Value, default);
        }

        public void SkipTime(ref List<SkipTimeDiamondParams> listParams, double period)
        {
            foreach (var pair in _minersMap)
            {
                if (!_cancellationTokenSources.ContainsKey(pair.Key)) continue;
                
                _cancellationTokenSources[pair.Key].Cancel();
                var extractParams = pair.Value.SkipTime(ref listParams, period);
                Extract(pair.Value, extractParams);
            }
        }

        private void Extract(IMiner miner, ExtractPositionParams extractPositionParams)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            _cancellationTokenSources[miner.GetId()] = cancellationTokenSource;
            ExtractAsync(miner, cancellationTokenSource.Token, extractPositionParams).Forget();
        }
        
        private async UniTask ExtractAsync(IMiner miner, CancellationToken cancellationToken, ExtractPositionParams extractPositionParams)
        {
            var diamond = await miner.Extract(cancellationToken, extractPositionParams);
            if (cancellationToken.IsCancellationRequested) return;
                
            _extractCompleteCallback?.Invoke(diamond);
            ExtractAsync(miner, cancellationToken, default).Forget();
        }
        
        private void InitMiners(IReadOnlyDictionary<string, MinerDefinition> minerDefinitions)
        {
            foreach (var pair in minerDefinitions)
            {
                var miner = _minersConstructor.Construct(pair.Value);
                _minersMap[miner.GetId()] = miner;
            }
        }

        private void Subscribe()
        {
            _boosterController.BuffsUpdated += OnBuffsUpdated;
        }

        private void UnSubscribe()
        {
            _boosterController.BuffsUpdated -= OnBuffsUpdated;
        }

        private void OnBuffsUpdated(IReadOnlyList<BuffType> buffsUpdated)
        {
            if (!buffsUpdated.Contains(BuffType.MinerSpeed) && !buffsUpdated.Contains(BuffType.MineDuration)) return;

            var minerSpeed = _boosterController.GetBuffModifier(BuffType.MinerSpeed);
            var mineDuration = _boosterController.GetBuffModifier(BuffType.MineDuration);
            
            foreach (var pair in _minersMap)
            {
                if (!_cancellationTokenSources.ContainsKey(pair.Key)) continue;
                
                _cancellationTokenSources[pair.Key].Cancel();
                var extractParams = pair.Value.ApplyMultipliers(minerSpeed, mineDuration);
                Extract(pair.Value, extractParams);
            }
        }
        
        public void Dispose()
        {
            UnSubscribe();
            foreach (var pair in _cancellationTokenSources) pair.Value.Dispose();
        }
    }
}