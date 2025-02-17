using System.Collections.Generic;
using DF.Abstractions.Boosters;

namespace DF.Models.Boosters
{
    public class BoosterDefinitionsProvider : IBoosterDefinitionsProvider
    {
        private Dictionary<string, BoosterDefinition> _definitionsMap;
        
        public BoosterDefinitionsProvider()
        {
            InitDefinitions();
        }

        public BoosterDefinition GetDefinition(string id) => _definitionsMap.ContainsKey(id) ? _definitionsMap[id] : null;
        public IReadOnlyDictionary<string, BoosterDefinition> GetAllDefinitions() => _definitionsMap;

        private void InitDefinitions()
        {
            _definitionsMap = new Dictionary<string, BoosterDefinition>
            {
                [Const.Items.Boosters.First] = Const.Items.Boosters.FirstDefinition,
                [Const.Items.Boosters.Second] = Const.Items.Boosters.SecondDefinition,
                [Const.Items.Boosters.Third] = Const.Items.Boosters.ThirdDefinition,
                [Const.Items.Boosters.Fourth] = Const.Items.Boosters.FourthDefinition,
                
            };
        }
    }
}