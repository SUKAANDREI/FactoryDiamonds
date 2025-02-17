using DF.Abstractions.Diamonds;
using DF.Abstractions.Miners;

namespace DF.Models.Miners
{
    public class MinersConstructor : IMinersConstructor
    {
        private readonly IDiamondsProvider _diamondsProvider;
        private readonly DfTime _dfTime;
        
        public MinersConstructor(IDiamondsProvider diamondsProvider, DfTime dfTime)
        {
            _diamondsProvider = diamondsProvider;
            _dfTime = dfTime;
        }
        
        public IMiner Construct(MinerDefinition minerDefinition)
        {
            var result = new Miner(minerDefinition, _diamondsProvider, _dfTime);
            return result;
        }
    }
}