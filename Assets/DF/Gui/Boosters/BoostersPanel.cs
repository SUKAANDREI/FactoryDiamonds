using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DF.Gui.Boosters
{
    public class BoostersPanel: ContextView<BoostersPanelContext>
    {
        [SerializeField] private BoosterView _viewPrototype;
        [SerializeField] private LayoutGroup _layoutGroup;

        private List<BoosterView> _views;

        protected override void Init()
        {
            base.Init();

            _viewPrototype.SetActiveSafeSelf(false);
            _views = new List<BoosterView>(10);
        }

        public override void UpdateView()
        {
            SetViews();
        }

        private void SetViews()
        {
            ClearViews();

            var definitions = Context.BoosterDefinitionsProvider.GetAllDefinitions();
            foreach (var pair in definitions)
            {
                var view = _viewPrototype.CorrectInstantiate(_layoutGroup.transform);
                var diamondViewContext = new BoosterViewContext()
                {
                    BoosterId = pair.Key,
                    BoosterController = Context.BoosterController,
                };
                view.Setup(diamondViewContext);
                view.SetActiveSafeSelf(true);

                _views.Add(view);
            }
        }

        private void ClearViews()
        {
            foreach (var view in _views)
            {
                view.Dispose();
                view.CorrectDestroy();
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