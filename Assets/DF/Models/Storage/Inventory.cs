using System;
using System.Collections.Generic;
using DF.Abstractions.Storage;

namespace DF.Models.Storage
{
    public class Inventory : IInventory
    {
        private readonly Dictionary<string, IItemStorage>  _storagesMap;
        
        public Inventory()
        {
            _storagesMap = new Dictionary<string, IItemStorage>(2);
        }
        
        public event Action<string, string> InventoryUpdated;

        public int GetCount(string type, string id) => GetStorage(type).Get(id);

        public bool Update(string type, string id, int value)
        {
            if (value == 0) return false;

            var storage = GetStorage(type);
            var oldValue = storage.Get(id);
            var newValue = Math.Max(0, oldValue + value);
            
            var result = storage.Set(id, newValue);
            if (result) InventoryUpdated?.Invoke(type, id);
            return result;
        }

        public bool Delete(string type, string id)
        {
            var storage = GetStorage(type);
            
            var result = storage.Set(id, default);
            if (result) InventoryUpdated?.Invoke(type, id);
            return result;
        }

        private IItemStorage GetStorage(string type)
        {
            if (!_storagesMap.ContainsKey(type)) _storagesMap[type] = new ItemStorage();
            return _storagesMap[type];
        }
    }
}