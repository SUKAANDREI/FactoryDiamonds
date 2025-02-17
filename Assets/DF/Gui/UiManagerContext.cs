using System;
using DF.Abstractions.Boosters;
using DF.Abstractions.Miners;
using DF.Abstractions.Recyclers;
using DF.Abstractions.Storage;

namespace DF.Gui
{
    public class UiManagerContext
    {
        public IInventory Inventory;
        public IRecycler Recycler;
        public IMinersController MinersController;
        public IBoosterDefinitionsProvider BoosterDefinitionsProvider;
        public IBoosterController BoosterController;
        public Action<double> SkipTimeCallback;
    }
}