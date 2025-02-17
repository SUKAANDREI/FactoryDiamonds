using DF.Abstractions.Boosters;
using DF.Abstractions.Storage;

namespace DF.Models.Boosters
{
    public class BoosterConstructor : IBoosterConstructor
    {
        private readonly IInventory _inventory;

        public BoosterConstructor(IInventory inventory)
        {
            _inventory = inventory;
        }
        
        public IBooster Construct(BoosterDefinition definition)
        {
            var result = new Booster(definition, _inventory);
            return result;
        }
    }
}