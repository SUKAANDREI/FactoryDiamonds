using System.Collections.Generic;
using System.Linq;
using DF.Abstractions.Diamonds;

namespace DF.Models.Diamonds
{
    public class DiamondsProvider : IDiamondsProvider
    {
        private readonly IDiamondsConstructor _diamondsConstructor;
        
        private readonly List<string> _diamondTypes;
        private readonly Dictionary<string, IDiamond> _diamondsPool;
        
        private readonly Dictionary<string, DiamondDefinition> _diamondDefinitionsMap = new(3)
        {
            { Const.Diamonds.Small, Const.Diamonds.SmallDefinition },
            { Const.Diamonds.Middle, Const.Diamonds.MiddleDefinition },
            { Const.Diamonds.Big, Const.Diamonds.BigDefinition },
        };

        public DiamondsProvider(IDiamondsConstructor diamondsConstructor)
        {
            _diamondsConstructor = diamondsConstructor;
            
            _diamondTypes = _diamondDefinitionsMap.Keys.ToList();
            _diamondsPool = new Dictionary<string, IDiamond>(3);
        }

        public IDiamond GetDiamond()
        {
            var randomType = _diamondTypes.GetRandom();
            if (_diamondsPool.ContainsKey(randomType)) return _diamondsPool[randomType];

            var result = _diamondsConstructor.Construct(_diamondDefinitionsMap[randomType]);
            _diamondsPool[randomType] = result;
            return result;
        }
    }
}