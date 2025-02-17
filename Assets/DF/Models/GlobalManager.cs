using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DF.Abstractions.Boosters;
using DF.Abstractions.Storage;
using DF.Abstractions.Diamonds;
using DF.Abstractions.Miners;
using DF.Abstractions.Recyclers;
using DF.Gui;
using DF.Models.Boosters;
using DF.Models.Storage;
using DF.Models.Diamonds;
using DF.Models.Miners;
using DF.Models.Recyclers;
using DF.Models.SkipTime;
using UnityEngine;

namespace DF.Models
{
    public class GlobalManager : MonoBehaviour, IDisposable
    {
        [SerializeField] private UiManager _uiManager;

        private DfTime _dfTime;
        private IMinersController _minersController;
        private Inventory _inventory;
        private IRecycler _recycler;
        private IBoosterDefinitionsProvider _boosterDefinitionsProvider;
        private IBoosterController _boosterController;

        private Dictionary<uint, CancellationTokenSource> _cancellationTokenSources;
        
        private void Awake()
        {
            Init();
            StartExtract();
        }

        private void Init()
        {
            _dfTime = new DfTime();
            _inventory = new Inventory();
            InitBoosters();
            InitMiners();
            InitRecycler();

            var uiContext = new UiManagerContext()
            {
                Inventory = _inventory,
                Recycler = _recycler,
                MinersController = _minersController,
                BoosterDefinitionsProvider = _boosterDefinitionsProvider,
                BoosterController = _boosterController,
                SkipTimeCallback = SkipTime,
            };
            _uiManager.Setup(uiContext);
        }

        private void StartExtract()
        {
            _minersController.Extract(CompleteExtract);

            void CompleteExtract(IDiamond diamond)
            {
                if (diamond != null)_recycler.AddDiamond(diamond);
            }
        }
        
        private void SkipTime(double period)
        {
            var skippedTimes = new List<double>(); 
            
            var activeBoosters = _boosterController.GetAllActiveBoosters().Values;
            foreach (var booster in activeBoosters)
            {
                var timeLeft = _boosterController.GetTimeLeft(booster.Id);
                if (timeLeft > period) continue;

                if (skippedTimes.Contains(timeLeft)) continue;
                
                skippedTimes.Add(timeLeft);
            }
            
            if (!skippedTimes.Contains(period)) skippedTimes.Add(period);

            double saveTime = default;
            skippedTimes.Sort();
            foreach (var skippedTime in skippedTimes)
            {
                var skipPeriod = skippedTime - saveTime;
                saveTime = skippedTime;

                _dfTime.SkipTime(skipPeriod);
                var skipTimeCollection = new List<SkipTimeDiamondParams>();
                _minersController.SkipTime(ref skipTimeCollection, skipPeriod);
                skipTimeCollection = skipTimeCollection.OrderBy(x => x.Time).ToList();
                _recycler.SkipTime(skipTimeCollection, skipPeriod);
                _boosterController.SkipTime();
            }
        }

        private void InitMiners()
        {
            IDiamondsConstructor diamondsConstructor = new DiamondsConstructor();
            IDiamondsProvider diamondsProvider = new DiamondsProvider(diamondsConstructor);
                
            IMinersConstructor minersConstructor = new MinersConstructor(diamondsProvider, _dfTime);
            IMinerDefinitionsProvider minerDefinitionsProvider = new MinerDefinitionsProvider();
            _minersController = new MinersController(minersConstructor, minerDefinitionsProvider, _boosterController);
        }

        private void InitRecycler()
        {
            IRecyclersConstructor recyclersConstructor = new RecyclersConstructor(_inventory, _dfTime, _boosterController);
            _recycler = recyclersConstructor.Construct(null);
        }

        private void InitBoosters()
        {
            IBoosterConstructor boosterConstructor = new BoosterConstructor(_inventory);
            _boosterDefinitionsProvider = new BoosterDefinitionsProvider();
            _boosterController = new BoosterController(boosterConstructor, _boosterDefinitionsProvider, _inventory, _dfTime);
        }
        
        private void OnDestroy()
        {
            Dispose();
        }

        public void Dispose()
        {
            _uiManager.Dispose();
            _recycler.Dispose();
            _boosterController.Dispose();
        }
    }
}