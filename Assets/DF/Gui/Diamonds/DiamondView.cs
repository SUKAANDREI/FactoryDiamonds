using TMPro;
using UnityEngine;

namespace DF.Gui.Diamonds
{
    public class DiamondView : ContextView<DiamondViewContext>
    {
        [SerializeField] private TextMeshProUGUI _typeText;

        public override void UpdateView()
        {
            SetType();
        }

        private void SetType()
        {
            _typeText.text = GetTypeText(Context.Diamond.GetType());
        }

        private string GetTypeText(string type)
        {
            return type switch
            {
                Const.Diamonds.Small => "S",
                Const.Diamonds.Middle => "M",
                Const.Diamonds.Big => "B",
                _ => string.Empty
            };
        }
    }
}