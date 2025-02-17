using System;
using System.Globalization;
using DF.Gui.Boosters;
using DF.Gui.Diamonds;
using DF.Gui.Miners;
using TMPro;
using UnityEngine;

namespace DF.Gui
{
    public class UiManager : ContextView<UiManagerContext>
    {
        [SerializeField] private TextMeshProUGUI _brilliantsText;
        [SerializeField] private TMP_InputField _skipTimeInputField;
        [SerializeField] private DiamondsPanel _diamondsPanel;
        [SerializeField] private MinersPanel _minersPanel;
        [SerializeField] private BoostersPanel _boostersPanel;

        public override void Setup(UiManagerContext context)
        {
            UnSubscribe();
            base.Setup(context);
            Subscribe();
        }

        public override void UpdateView()
        {
            SetBrilliants();
            SetDiamonds();
            SetMiners();
            SetBoosters();
            SetSkipTime();
        }
        
        private void Subscribe()
        {
            if (Context != null) Context.Inventory.InventoryUpdated += OnInventoryUpdated;
        }

        private void UnSubscribe()
        {
            if (Context != null) Context.Inventory.InventoryUpdated -= OnInventoryUpdated;
        }
        
        private void SetBrilliants()
        {
            var brilliantsValue = Context.Inventory.GetCount(Const.Items.Types.Currency, Const.Items.Currency.Brilliants);
            _brilliantsText.text = $"Brilliants: {brilliantsValue}";
        }

        private void SetDiamonds()
        {
            var diamondsPanelContext = new DiamondsPanelContext() { Recycler = Context.Recycler };
            _diamondsPanel.Setup(diamondsPanelContext);
        }

        private void SetMiners()
        {
            var diamondsPanelContext = new MinersPanelContext() { MinersController = Context.MinersController };
            _minersPanel.Setup(diamondsPanelContext);
        }

        private void SetBoosters()
        {
            var boostersPanelContext = new BoostersPanelContext()
            {
                BoosterDefinitionsProvider = Context.BoosterDefinitionsProvider,
                BoosterController = Context.BoosterController,
            };
            _boostersPanel.Setup(boostersPanelContext);
        }
        
        private void SetSkipTime()
        {
            _skipTimeInputField.text = Const.DefaultSkipTime.ToString(CultureInfo.InvariantCulture);
        }

        private void OnInventoryUpdated(string type, string id)
        {
            if (type != Const.Items.Types.Currency || id != Const.Items.Currency.Brilliants) return;

            SetBrilliants();
        }

        public void U_SkipTime()
        {
            var period = Convert.ToDouble(_skipTimeInputField.text);
            Context.SkipTimeCallback?.Invoke(period);
        }
        
        public override void Dispose()
        {
            UnSubscribe();
            _diamondsPanel.Dispose();
            base.Dispose();
        }
    }
}