using DF.Abstractions.Boosters;
using DF.Abstractions.Recyclers;
using DF.Abstractions.Storage;

namespace DF.Models.Recyclers
{
    public class RecyclersConstructor : IRecyclersConstructor
    {
        private readonly IInventory _inventory;
        private readonly DfTime _dfTime;
        private readonly IBoosterController _boosterController;
        
        public RecyclersConstructor(IInventory inventory, DfTime dfTime, IBoosterController boosterController)
        {
            _inventory = inventory;
            _dfTime = dfTime;
            _boosterController = boosterController;
        }
        
        public IRecycler Construct(object definition)
        {
            var result = new Recycler(_inventory, _dfTime, _boosterController);
            return result;
        }
    }
}