namespace DF.Models.Diamonds
{
    public struct DiamondDefinition
    {
        public string Type;
        public int CountBrilliant;
        public float ProcessingDiamondTime;

        public DiamondDefinition(string type, int countBrilliant, float processingDiamondTime)
        {
            Type = type;
            CountBrilliant = countBrilliant;
            ProcessingDiamondTime = processingDiamondTime;
        }
    }
}