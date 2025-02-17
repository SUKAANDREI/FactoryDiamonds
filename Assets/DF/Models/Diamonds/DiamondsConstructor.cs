using DF.Abstractions.Diamonds;

namespace DF.Models.Diamonds
{
    public class DiamondsConstructor : IDiamondsConstructor
    {
        public IDiamond Construct(DiamondDefinition diamondDefinition)
        {
            var result = new Diamond(diamondDefinition);
            return result;
        }
    }
}