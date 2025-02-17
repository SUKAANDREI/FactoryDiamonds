using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DF.Abstractions.Boosters;
using DF.Abstractions.Storage;
using DF.Models.Storage;

namespace DF.Models.Boosters
{
    public class BoosterController : IBoosterController
    {
        private readonly IBoosterConstructor _boosterConstructor;
        private readonly IBoosterDefinitionsProvider _boosterDefinitionsProvider;
        private readonly IInventory _inventory;
        private readonly DfTime _dfTime;
        private readonly IBoosterTimesStorage _boosterTimesStorage;
        
        private readonly Dictionary<string, IBooster> _activeBoostersMap;
        private readonly Dictionary<BuffType, float> _activeBuffsMap;
        private CancellationTokenSource _cancellationTokenSource;

        public BoosterController(
            IBoosterConstructor boosterConstructor,
            IBoosterDefinitionsProvider boosterDefinitionsProvider,
            IInventory inventory,
            DfTime dfTime)
        {
            _boosterConstructor = boosterConstructor;
            _boosterDefinitionsProvider = boosterDefinitionsProvider;
            _inventory = inventory;
            _dfTime = dfTime;
            
            _boosterTimesStorage = new BoosterTimesStorage();

            var allDefinitions = _boosterDefinitionsProvider.GetAllDefinitions();
            _activeBoostersMap = new Dictionary<string, IBooster>(allDefinitions.Count);

            _activeBuffsMap = new Dictionary<BuffType, float>(Enum.GetValues(typeof(BuffType)).Length);
        }
        
        public event Action<string, bool> BoosterActiveUpdated;
        public event Action<IReadOnlyList<BuffType>> BuffsUpdated;

        public bool IsActive(string id) => _activeBoostersMap.ContainsKey(id);
        
        public bool Activate(string id)
        {
            if (!IsActive(id))
            {
                var definition = _boosterDefinitionsProvider.GetDefinition(id);
                if (definition == null) return false;

                _activeBoostersMap[id] = _boosterConstructor.Construct(definition);
                _boosterTimesStorage.Set(id, _dfTime.GetTime());
                UpdateActiveBuffs();
                BoosterActiveUpdated?.Invoke(id, true);
            }
            
            _activeBoostersMap[id].Apply();
            TryResetTimers();
            return true;
        }

        public bool Deactivate(string id)
        {
            if (!IsActive(id)) return false;
           
            _boosterTimesStorage.Set(id, default);
            _activeBoostersMap[id].Cancel();
            _activeBoostersMap.Remove(id);
            UpdateActiveBuffs();

            BoosterActiveUpdated?.Invoke(id, false);
            return true;
        }

        public IReadOnlyDictionary<string, IBooster> GetAllActiveBoosters() => _activeBoostersMap;

        public float GetBuffModifier(BuffType buffType) => _activeBuffsMap.ContainsKey(buffType) ? _activeBuffsMap[buffType] : default;
        public IReadOnlyDictionary<BuffType, float> GetAllBuffs() => _activeBuffsMap;
        public double GetTimeLeft(string id) => IsActive(id) ? GetTimeLeft(_activeBoostersMap[id]) : default;

        public void SkipTime()
        {
            TryResetTimers();
        }
        
        private void TryResetTimers()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            
            TryResetTimersAsync().Forget();
        }
        
        private async UniTask TryResetTimersAsync()
        {
            var nearTime = double.MaxValue;
            var expiredBoosters = new List<string>();
            foreach (var pair in _activeBoostersMap)
            {
                var timeLeft = GetTimeLeft(pair.Value);
                if (timeLeft == 0)
                {
                    expiredBoosters.Add(pair.Key);
                    continue;
                }
                
                if (timeLeft > nearTime) continue;

                nearTime = timeLeft;
            }
            
            foreach (var boosterId in expiredBoosters) Deactivate(boosterId);

            if (nearTime.Equals(double.MaxValue)) return;
			
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;
            await UniTask.Delay(TimeSpan.FromSeconds(nearTime), DelayType.Realtime, cancellationToken: cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;

            TryResetTimers();
        }

        private double GetTimeLeft(IBooster booster)
        {
            var startTime = _boosterTimesStorage.Get(booster.Id);
            var boosterCounts = _inventory.GetCount(Const.Items.Types.Boosters, booster.Id);
            var duration = boosterCounts * booster.Duration;
            return _dfTime.GetTimeLeft(startTime, duration);
        }

        private void UpdateActiveBuffs()
        {
            var tempActiveBuffsMap = new Dictionary<BuffType, float>(_activeBuffsMap.Count);
            foreach (var pair in _activeBoostersMap)
            {
                var definitions = pair.Value.GetAllBuffDefinitions();
                foreach (var definition in definitions)
                {
                    if (!tempActiveBuffsMap.ContainsKey(definition.BuffType)) tempActiveBuffsMap[definition.BuffType] = default;
                    tempActiveBuffsMap[definition.BuffType] += definition.Modifier;
                }
            }

            var buffsUpdated = new List<BuffType>();
            foreach (var pair in tempActiveBuffsMap)
            {
                var currentModifier = GetBuffModifier(pair.Key);
                if (currentModifier.Equals(pair.Value)) continue;

                _activeBuffsMap[pair.Key] = pair.Value;
                buffsUpdated.Add(pair.Key);
            }

            var keys = _activeBuffsMap.Keys.ToList();
            foreach (var key in keys)
            {
                if (tempActiveBuffsMap.ContainsKey(key)) continue;

                if (!buffsUpdated.Contains(key)) buffsUpdated.Add(key);
                _activeBuffsMap.Remove(key);
            }

            BuffsUpdated?.Invoke(buffsUpdated);
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
        }
    }
}