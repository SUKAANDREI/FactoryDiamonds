using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DF.Gui.Diamonds
{
    public class DiamondsPanel : ContextView<DiamondsPanelContext>
    {
        [SerializeField] private DiamondView _diamondViewPrototype;
        [SerializeField] private LayoutGroup _layoutGroup;
        
        private List<DiamondView> _diamondViews;

        public override void Setup(DiamondsPanelContext context)
        {
            UnSubscribe();
            base.Setup(context);
            Subscribe();
        }

        protected override void Init()
        {
            base.Init();
            
            _diamondViewPrototype.SetActiveSafeSelf(false);
            _diamondViews = new List<DiamondView>(10);
        }

        public override void UpdateView()
        {
            SetDiamonds();
        }

        private void Subscribe()
        {
            if (Context != null) Context.Recycler.QueueDiamondsUpdated += OnQueueDiamondsUpdated;
        }

        private void UnSubscribe()
        {
            if (Context != null) Context.Recycler.QueueDiamondsUpdated -= OnQueueDiamondsUpdated;
        }
        
        private void SetDiamonds()
        {
            ClearDiamonds();
            
            var diamonds = Context.Recycler.GetAllCurrentDiamonds();
            foreach (var diamond in diamonds)
            {
                var diamondView = _diamondViewPrototype.CorrectInstantiate(_layoutGroup.transform);
                var diamondViewContext = new DiamondViewContext() { Diamond = diamond };
                diamondView.Setup(diamondViewContext);
                diamondView.SetActiveSafeSelf(true);

                _diamondViews.Add(diamondView);
            }
        }

        private void ClearDiamonds()
        {
            foreach (var diamondView in _diamondViews) diamondView.CorrectDestroy();
            _diamondViews.Clear();
        }

        private void OnQueueDiamondsUpdated()
        {
            SetDiamonds();
        }

        public override void Dispose()
        {
            UnSubscribe();
            base.Dispose();
        }
    }
}