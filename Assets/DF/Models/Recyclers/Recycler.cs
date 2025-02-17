using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DF.Abstractions.Boosters;
using DF.Abstractions.Diamonds;
using DF.Abstractions.Recyclers;
using DF.Abstractions.Storage;
using DF.Models.Boosters;
using DF.Models.SkipTime;

namespace DF.Models.Recyclers
{
    public class Recycler : IRecycler
    {
        private readonly IInventory _inventory;
        private readonly DfTime _dfTime;
        private readonly IBoosterController _boosterController;
        
        private readonly Queue<IDiamond> _diamonds;
        private CancellationTokenSource _cancellationTokenSource;
        private TimeStampOperation _timeStampOperation;
        private float _currentModifier;

        public Recycler(IInventory inventory, DfTime dfTime, IBoosterController boosterController)
        {
            _inventory = inventory;
            _dfTime = dfTime;
            _boosterController = boosterController;
            
            _diamonds = new Queue<IDiamond>();
            Subscribe();
        }

        public event Action QueueDiamondsUpdated;
        
        public void AddDiamond(IDiamond diamond)
        {
            _diamonds.Enqueue(diamond);
            DiamondsUpdated();
        }

        public IReadOnlyCollection<IDiamond> GetAllCurrentDiamonds() => _diamonds;

        private int GetCurrentDiamondCountBrilliant()
        {
            return _diamonds.Count > 0 ? _diamonds.Peek().GetCountBrilliant() : default;
        }
        
        private float GetCurrentProcessingDiamondPeriod()
        {
            return _diamonds.Count > 0 ? GetProcessingDiamondPeriod(_diamonds.Peek()) : default;
        }
        
        public void SkipTime(List<SkipTimeDiamondParams> skipTimeCollection, double period)
        {
            var currentTime = _dfTime.GetTime();
            var oldTime = currentTime - period;
            double lastProcessEndTime = default;
            var diamondsUpdated = false;
            var countBrilliants = 0;
            
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;

                var lastProcessPeriod = CalculateLastProcessDiamondPeriod(oldTime);
                if (lastProcessPeriod > period)
                {
                    diamondsUpdated |= AddSkippedDiamonds(skipTimeCollection);
                    if (diamondsUpdated) QueueDiamondsUpdated?.Invoke();
                    
                    var currentPosition = CalculateProcessDiamondPosition(currentTime, GetCurrentProcessingDiamondPeriod());
                    TryStartProcessDiamond(currentPosition).Forget();
                    return;
                }

                lastProcessEndTime = oldTime + lastProcessPeriod;
                countBrilliants += GetCurrentDiamondCountBrilliant();
                _diamonds.Dequeue();
                diamondsUpdated = true;
            }

            double firstFailedProcessStartTime = default;
            diamondsUpdated |= SkipProcessDiamonds(currentTime, ref lastProcessEndTime, ref countBrilliants, ref firstFailedProcessStartTime);

            if (!firstFailedProcessStartTime.Equals(default))
            {
                UpdateBrilliants(countBrilliants);

                diamondsUpdated |= AddSkippedDiamonds(skipTimeCollection);
                if (diamondsUpdated) QueueDiamondsUpdated?.Invoke();
                TryStartProcessDiamond(firstFailedProcessStartTime, currentTime);
                return;
            }

            firstFailedProcessStartTime = default;
            diamondsUpdated |= AddSkippedDiamondsByTime(
                skipTimeCollection,
                currentTime,
                lastProcessEndTime,
                ref countBrilliants,
                ref firstFailedProcessStartTime);
            if (diamondsUpdated) QueueDiamondsUpdated?.Invoke();

            UpdateBrilliants(countBrilliants);

            if (firstFailedProcessStartTime.Equals(default)) return;
            
            TryStartProcessDiamond(firstFailedProcessStartTime, currentTime);
        }

        private void TryStartProcessDiamond(double timeStamp, double time)
        {
            UpdateTimeStampOperation(timeStamp, 0);
            var position = CalculateProcessDiamondPosition(time, GetCurrentProcessingDiamondPeriod());
            TryStartProcessDiamond(position).Forget();
        }

        private bool SkipProcessDiamonds(double time, ref double lastProcessEndTime, ref int countBrilliants, ref double firstFailedProcessStartTime)
        {
            var result = false;
            while (true)
            {
                if (_diamonds.Count == 0) break;
                
                var curProcessEndTime = lastProcessEndTime + GetCurrentProcessingDiamondPeriod();
                if (curProcessEndTime > time)
                {
                    firstFailedProcessStartTime = lastProcessEndTime;
                    break;
                }

                lastProcessEndTime = curProcessEndTime;
                countBrilliants += GetCurrentDiamondCountBrilliant();
                _diamonds.Dequeue();
                result = true;
            }

            return result;
        }

        private bool AddSkippedDiamonds(List<SkipTimeDiamondParams> skipTimeCollection)
        {
            if (skipTimeCollection.Count == 0) return false;
            
            foreach (var skipParams in skipTimeCollection) _diamonds.Enqueue(skipParams.Diamond);
            return true;
        }

        private bool AddSkippedDiamondsByTime(
            List<SkipTimeDiamondParams> skipTimeCollection, 
            double time,
            double lastProcessEndTime,
            ref int countBrilliants,
            ref double firstFailedProcessStartTime)
        {
            if (skipTimeCollection.Count == 0) return false;

            var result = false;
            foreach (var skipParams in skipTimeCollection)
            {
                _diamonds.Enqueue(skipParams.Diamond);

                var curProcessStartTime = Math.Max(lastProcessEndTime, skipParams.Time);
                var curProcessEndTime = curProcessStartTime + GetCurrentProcessingDiamondPeriod();
                if (curProcessEndTime > time)
                {
                    if (firstFailedProcessStartTime.Equals(default)) firstFailedProcessStartTime = curProcessStartTime;
                    continue;
                }
                
                lastProcessEndTime = curProcessEndTime;
                countBrilliants += GetCurrentDiamondCountBrilliant();
                _diamonds.Dequeue();
                result = true;
            }
            
            return result;
        }

        private async UniTaskVoid TryStartProcessDiamond(float completedCoefficient = default)
        {
            if (_cancellationTokenSource != null || _diamonds.Count == 0)
            {
                await UniTask.CompletedTask;
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;
            var diamond = _diamonds.Peek();
            var countBrilliants = await TryStartProcessDiamond(diamond, cancellationToken, completedCoefficient);
            if (cancellationToken.IsCancellationRequested) return;

            UpdateBrilliants(countBrilliants);
            _diamonds.Dequeue();
            _cancellationTokenSource = null;
            
            DiamondsUpdated();
        }
        
        private async UniTask<int> TryStartProcessDiamond(IDiamond diamond, CancellationToken cancellationToken, float completedCoefficient)
        {
            var result = await ProcessDiamond(diamond, cancellationToken, completedCoefficient);
            return !cancellationToken.IsCancellationRequested ? result : 0;
        }

        private async UniTask<int> ProcessDiamond(IDiamond diamond, CancellationToken cancellationToken, float completedCoefficient)
        {
            UpdateTimeStampOperation(_dfTime.GetTime(), completedCoefficient);
            var coefficient = 1 - completedCoefficient;
            var delayTimeSpan = TimeSpan.FromSeconds(GetProcessingDiamondPeriod(diamond) * coefficient);
            await UniTask.Delay(delayTimeSpan, DelayType.Realtime, cancellationToken: cancellationToken);
            return diamond.GetCountBrilliant();
        }

        private double CalculateLastProcessDiamondPeriod(double time)
        {
            if (_timeStampOperation == null) return default;
            
            var currentDiamond = _diamonds.Peek();
            var processingDiamondPeriod = GetProcessingDiamondPeriod(currentDiamond);
            var currentPosition = CalculateProcessDiamondPosition(time, processingDiamondPeriod);
            return (1 - currentPosition) * processingDiamondPeriod;
        }
        
        private void OnBuffsUpdated(IReadOnlyList<BuffType> buffsUpdated)
        {
            if (!buffsUpdated.Contains(BuffType.ProcessingDiamondSpeed)) return;
            
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            
            float currentPosition = 0;
            if (_diamonds.Count != 0)
            {
                var currentDiamond = _diamonds.Peek();
                currentPosition = CalculateProcessDiamondPosition(_dfTime.GetTime(), GetProcessingDiamondPeriod(currentDiamond));
            }
            
            _currentModifier = _boosterController.GetBuffModifier(BuffType.ProcessingDiamondSpeed);
            TryStartProcessDiamond(currentPosition).Forget();
        }

        private float CalculateProcessDiamondPosition(double time, float processTime)
        {
            if (_timeStampOperation == null) return default;

            var period = time - _timeStampOperation.Time;
            var percentOperation = (float)(period / processTime);
            var result = _timeStampOperation.TaskCompletedCoefficient + percentOperation;
            return result;
        }

        private void DiamondsUpdated()
        {
            QueueDiamondsUpdated?.Invoke();
            TryStartProcessDiamond().Forget();
        }

        private void UpdateBrilliants(int value)
        {
            _inventory.Update(Const.Items.Types.Currency, Const.Items.Currency.Brilliants, value);
        }
        
        private void Subscribe()
        {
            _boosterController.BuffsUpdated += OnBuffsUpdated;
        }

        private void UnSubscribe()
        {
            _boosterController.BuffsUpdated -= OnBuffsUpdated;
        }
        
        private void UpdateTimeStampOperation(double time, float taskCompletedCoefficient)
        {
            _timeStampOperation = new TimeStampOperation(time, taskCompletedCoefficient);
        }

        private float GetProcessingDiamondPeriod(IDiamond diamond)
        {
            return diamond.GetProcessingDiamondTime() / (1 + _currentModifier);
        }

        public void Dispose()
        {
            UnSubscribe();
            _cancellationTokenSource?.Dispose();
        }
    }
}
