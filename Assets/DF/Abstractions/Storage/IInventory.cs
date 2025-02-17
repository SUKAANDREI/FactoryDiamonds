using System;

namespace DF.Abstractions.Storage
{
    public interface IInventory
    {
        event Action<string, string> InventoryUpdated;
        int GetCount(string type, string id);
        bool Update(string type, string id, int value);
        bool Delete(string type, string id);
    }
}