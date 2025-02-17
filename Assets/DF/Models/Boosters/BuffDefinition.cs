namespace DF.Models.Boosters
{
    public struct BuffDefinition
    {
        public BuffType BuffType;
        public float Modifier;

        public BuffDefinition(BuffType buffType, float modifier)
        {
            BuffType = buffType;
            Modifier = modifier;
        }
    }
}