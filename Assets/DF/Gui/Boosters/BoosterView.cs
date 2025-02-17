using TMPro;
using UnityEngine;

namespace DF.Gui.Boosters
{
    public class BoosterView : ContextView<BoosterViewContext>
    {
        [SerializeField] private TextMeshProUGUI _text;

        public override void Setup(BoosterViewContext context)
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
            if (Context != null) Context.BoosterController.BoosterActiveUpdated += OnBoosterActiveUpdated;
        }

        private void UnSubscribe()
        {
            if (Context != null) Context.BoosterController.BoosterActiveUpdated -= OnBoosterActiveUpdated;
        }

        private void SetState() => SetState(Context.BoosterController.IsActive(Context.BoosterId));
        private void SetState(bool value)
        {
            _text.text = SetStateText(value);
        }

        private string SetStateText(bool value)
        {
            return value switch
            {
                true => $"{Context.BoosterId} active",
                _ => $"{Context.BoosterId} not active"
            };
        }

        private void OnBoosterActiveUpdated(string id, bool value)
        {
            if (Context.BoosterId != id) return;
            
            SetState(value);
        }

        public void U_Active()
        {
            Context.BoosterController.Activate(Context.BoosterId);
        }
        
        public override void Dispose()
        {
            UnSubscribe();
            base.Dispose();
        }
    }
}