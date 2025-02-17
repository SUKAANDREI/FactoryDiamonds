using DF.Models.Miners;
using TMPro;
using UnityEngine;

namespace DF.Gui.Miners
{
    public class MinerView : ContextView<MinerViewContext>
    {
        [SerializeField] private TextMeshProUGUI _stateText;

        public override void Setup(MinerViewContext context)
        {
            UnSubscribe();
            base.Setup(context);
            Subscribe();
        }
        
        public override void UpdateView()
        {
            SetState();
        }
        
        private void Subscribe()
        {
            if (Context != null) Context.Miner.StateUpdated += OnStateUpdated;
        }

        private void UnSubscribe()
        {
            if (Context != null) Context.Miner.StateUpdated -= OnStateUpdated;
        }

        private void SetState()
        {
            _stateText.text = SetStateText(Context.Miner.GetState());
        }

        private string SetStateText(MinerState state)
        {
            return state switch
            {
                MinerState.None => "-",
                MinerState.WayStraight => "WS",
                MinerState.Mine => "M",
                MinerState.WayBack => "WB",
                _ => string.Empty
            };
        }

        private void OnStateUpdated()
        {
            SetState();
        }
        
        public override void Dispose()
        {
            UnSubscribe();
            base.Dispose();
        }
    }
}