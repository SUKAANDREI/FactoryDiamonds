using DF.Abstractions.Diamonds;

namespace DF.Models.Diamonds
{
    public class Diamond : IDiamond
    {
        private DiamondDefinition _diamondDefinition; 
        
        public Diamond(DiamondDefinition diamondDefinition)
        {
            _diamondDefinition = diamondDefinition;
        }

        public string GetType() => _diamondDefinition.Type;
        public int GetCountBrilliant() => _diamondDefinition.CountBrilliant;
        public float GetProcessingDiamondTime() => _diamondDefinition.ProcessingDiamondTime;
    }
}