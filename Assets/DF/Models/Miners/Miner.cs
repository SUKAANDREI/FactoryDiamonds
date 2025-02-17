using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DF.Abstractions.Diamonds;
using DF.Abstractions.Miners;
using DF.Models.SkipTime;
using UnityEngine;

namespace DF.Models.Miners
{
    public class Miner : IMiner
    {
        private readonly MinerDefinition _minerDefinition;
        private readonly IDiamondsProvider _diamondsProvider;
        private readonly DfTime _dfTime;

        private MinerState _minerState;
        private TimeStampOperation _timeStampOperation;
        private readonly List<MinerState> _minerOperations;
        private readonly Dictionary<MinerState, float> _minerOperationTimesMap;
        private float _sumTimeOperations;

        public Miner(MinerDefinition minerDefinition, IDiamondsProvider diamondsProvider, DfTime dfTime)
        {
            _minerDefinition = minerDefinition;
            _diamondsProvider = diamondsProvider;
            _dfTime = dfTime;

            _minerOperations = new List<MinerState>(3)
            {
                MinerState.WayStraight,
                MinerState.Mine,
                MinerState.WayBack,
            };
            _minerOperationTimesMap = new Dictionary<MinerState, float>(3);
            ReCalculateTimeOperations();
        }

        public event Action StateUpdated;
        public string GetId() => _minerDefinition.Id;
        public MinerState GetState() => _minerState;
        
        public async UniTask<IDiamond> Extract(CancellationToken cancellationToken, ExtractPositionParams extractPositionParams = default)
        {
            if (await FollowRoute(MinerState.WayStraight, cancellationToken, extractPositionParams)) extractPositionParams = default;
            if (cancellationToken.IsCancellationRequested) return null;

            if (await MineDiamond(cancellationToken, extractPositionParams)) extractPositionParams = default;
            if (cancellationToken.IsCancellationRequested) return null;

            await FollowRoute(MinerState.WayBack, cancellationToken, extractPositionParams);
            return !cancellationToken.IsCancellationRequested ? _diamondsProvider.GetDiamond() : null;
        }

        public ExtractPositionParams SkipTime(ref List<SkipTimeDiamondParams> skipParams, double period)
        {
            var currentTime = _dfTime.GetTime();
            var oldTime = currentTime - period;
            var lastPeriod = CollectSkipDiamonds(ref skipParams, oldTime, period);
            foreach (var operation in _minerOperations)
            {
                if (lastPeriod > _minerOperationTimesMap[operation])
                {
                    lastPeriod -= _minerOperationTimesMap[operation];
                    continue;
                }
                    
                UpdateTimeStampOperation(currentTime - lastPeriod, operation, 0);
                break;
            }

            return CalculateExtractPosition(currentTime);
        }

        public ExtractPositionParams ApplyMultipliers(float minerSpeed, float mineDuration)
        {
            var result = CalculateExtractPosition(_dfTime.GetTime());
            ReCalculateTimeOperations(minerSpeed, mineDuration);
            return result;
        }
        
        private ExtractPositionParams CalculateExtractPosition(double time)
        {
            if (_timeStampOperation == null) return default;
            
            var period = time - _timeStampOperation.Time;
            var percentOperation = (float)(period / _minerOperationTimesMap[_timeStampOperation.MinerState]);
            var resultCoefficient = _timeStampOperation.TaskCompletedCoefficient + percentOperation;
            resultCoefficient = Math.Min(resultCoefficient, 1);
            return new ExtractPositionParams
            {
                MinerState = _timeStampOperation.MinerState,
                TaskCompletedCoefficient = resultCoefficient,
            };
        }

        private double CalculateLastExtractPeriod(double time)
        {
            if (_timeStampOperation == null) return default;
            
            var currentPosition = CalculateExtractPosition(time);
            var result = CalculateLastOperationPeriod(currentPosition);
            foreach (var operation in _minerOperations)
            {
                if (operation <= currentPosition.MinerState) continue;
                result += _minerOperationTimesMap[operation];
            }

            return result;
        }

        private double CalculateLastOperationPeriod(ExtractPositionParams positionParams)
        {
            var lastOperationCoefficient = 1 - positionParams.TaskCompletedCoefficient;
            return _minerOperationTimesMap[positionParams.MinerState] * lastOperationCoefficient;
        }

        private void ReCalculateTimeOperations(float minerSpeed = default, float mineDuration = default)
        {
            var walkingPeriod = _minerDefinition.ShaftDistance / _minerDefinition.MinerSpecification.MoveSpeed / (1 + minerSpeed);
            var minePeriod = _minerDefinition.MinerSpecification.MineDuration / (1 + mineDuration);

            _minerOperationTimesMap[MinerState.WayStraight] = walkingPeriod;
            _minerOperationTimesMap[MinerState.Mine] = minePeriod;
            _minerOperationTimesMap[MinerState.WayBack] = walkingPeriod;
            
            _sumTimeOperations = walkingPeriod * 2 + minePeriod;
        }

        private double CollectSkipDiamonds(ref List<SkipTimeDiamondParams> skipParams, double startTime, double skipPeriod)
        {
            var lastExtractPeriod = CalculateLastExtractPeriod(startTime);
            if (lastExtractPeriod > skipPeriod) return skipPeriod;
            
            double deltaTime = 0;
            skipPeriod -= lastExtractPeriod;
            var timeFirstDiamond = startTime + lastExtractPeriod;
            skipParams.Add(CreateSkipTimeDiamondParams(timeFirstDiamond));

            while (skipPeriod > _sumTimeOperations)
            {
                skipPeriod -= _sumTimeOperations;
                skipParams.Add(CreateSkipTimeDiamondParams(timeFirstDiamond + deltaTime + _sumTimeOperations));
                deltaTime += _sumTimeOperations;
            }

            return skipPeriod;

            SkipTimeDiamondParams CreateSkipTimeDiamondParams(double time)
            {
                return new SkipTimeDiamondParams()
                {
                    Time = time,
                    Diamond = _diamondsProvider.GetDiamond(),
                };
            }
        }

        private async UniTask<bool> FollowRoute(
            MinerState minerState, 
            CancellationToken cancellationToken, 
            ExtractPositionParams extractPositionParams)
        {
            if (extractPositionParams.MinerState != MinerState.None && extractPositionParams.MinerState != minerState)
            {
                await UniTask.CompletedTask;
                return false;
            }

            await WaitOperation(minerState, extractPositionParams, cancellationToken);
            return true;
        }
        
        private async UniTask<bool> MineDiamond(
            CancellationToken cancellationToken, 
            ExtractPositionParams extractPositionParams)
        {
            if (extractPositionParams.MinerState != MinerState.None && extractPositionParams.MinerState != MinerState.Mine)
            {
                await UniTask.CompletedTask;
                return false;
            }

            await WaitOperation(MinerState.Mine, extractPositionParams, cancellationToken);
            return true;
        }

        private async UniTask WaitOperation(
            MinerState minerState, 
            ExtractPositionParams extractPositionParams, 
            CancellationToken cancellationToken)
        {
            UpdateMinerState(minerState);
            UpdateTimeStampOperation(_dfTime.GetTime(), minerState, extractPositionParams.TaskCompletedCoefficient);
            var coefficient = 1 - extractPositionParams.TaskCompletedCoefficient;
            var delayTimeSpan = TimeSpan.FromSeconds(_minerOperationTimesMap[minerState] * coefficient);
            await UniTask.Delay(delayTimeSpan, DelayType.Realtime, cancellationToken: cancellationToken);
        }

        private void UpdateMinerState(MinerState minerState)
        {
            _minerState = minerState;
            StateUpdated?.Invoke();
        }

        private void UpdateTimeStampOperation(double time, MinerState minerState, float taskCompletedCoefficient)
        {
            _timeStampOperation = new TimeStampOperation(time, minerState, taskCompletedCoefficient);
        }

        public void Dispose()
        {
        }
    }
}