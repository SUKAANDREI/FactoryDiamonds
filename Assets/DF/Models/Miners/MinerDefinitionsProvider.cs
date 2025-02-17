using System.Collections.Generic;
using DF.Abstractions.Miners;

namespace DF.Models.Miners
{
    public class MinerDefinitionsProvider : IMinerDefinitionsProvider
    {
        private Dictionary<string, MinerDefinition> _minerDefinitionsMap;
        
        public MinerDefinitionsProvider()
        {
            InitDefinitions();
        }

        public MinerDefinition GetDefinition(string id) => throw new System.NotImplementedException();
        public IReadOnlyDictionary<string, MinerDefinition> GetAllDefinitions() => _minerDefinitionsMap;

        private void InitDefinitions()
        {
            _minerDefinitionsMap = new Dictionary<string, MinerDefinition>
            {
                [Const.Miners.First] = Const.Miners.FirstDefinition,
                [Const.Miners.Second] = Const.Miners.SecondDefinition,
                [Const.Miners.Third] = Const.Miners.ThirdDefinition,
                [Const.Miners.Fourth] = Const.Miners.FourthDefinition,
            };
        }
    }
}