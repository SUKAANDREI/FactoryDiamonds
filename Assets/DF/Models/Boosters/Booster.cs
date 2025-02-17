using System.Collections.Generic;
using DF.Abstractions.Boosters;
using DF.Abstractions.Storage;

namespace DF.Models.Boosters
{
    public class Booster : IBooster
    {
        private readonly BoosterDefinition _boosterDefinition;
        private readonly IInventory _inventory;
        
        public Booster(BoosterDefinition boosterDefinition, IInventory inventory)
        {
            _boosterDefinition = boosterDefinition;
            _inventory = inventory;
        }

        public string Id => _boosterDefinition.Id;
        public float Duration => _boosterDefinition.Duration;
        public IReadOnlyList<BuffDefinition> GetAllBuffDefinitions() => _boosterDefinition.BuffData;
        
        public void Apply()
        {
            _inventory.Update(Const.Items.Types.Boosters, _boosterDefinition.Id, 1);
        }

        public void Cancel()
        {
            _inventory.Delete(Const.Items.Types.Boosters, _boosterDefinition.Id);
        }
    }
}