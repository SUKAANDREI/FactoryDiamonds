using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DF.Gui.Miners
{
    public class MinersPanel : ContextView<MinersPanelContext>
    {
        [SerializeField] private MinerView _viewPrototype;
        [SerializeField] private LayoutGroup _layoutGroup;

        private List<MinerView> _views;

        protected override void Init()
        {
            base.Init();

            _viewPrototype.SetActiveSafeSelf(false);
            _views = new List<MinerView>(10);
        }

        public override void UpdateView()
        {
            SetDiamonds();
        }

        private void SetDiamonds()
        {
            ClearViews();

            var miners = Context.MinersController.GetAllMiners();
            foreach (var pair in miners)
            {
                var view = _viewPrototype.CorrectInstantiate(_layoutGroup.transform);
                var diamondViewContext = new MinerViewContext() { Miner = pair.Value };
                view.Setup(diamondViewContext);
                view.SetActiveSafeSelf(true);

                _views.Add(view);
            }
        }

        private void ClearViews()
        {
            foreach (var diamondView in _views)
            {
                diamondView.Dispose();
                diamondView.CorrectDestroy();
            }
            _views.Clear();
        }

        public override void Dispose()
        {
            ClearViews();
            base.Dispose();
        }
    }
}