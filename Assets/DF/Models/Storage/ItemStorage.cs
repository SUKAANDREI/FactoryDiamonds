using System;
using System.Collections.Generic;
using DF.Abstractions.Storage;

namespace DF.Models.Storage
{
    public class ItemStorage : IItemStorage
    {
        private readonly Dictionary<string, int> _data;

        public ItemStorage()
        {
            _data = new Dictionary<string, int>(3);
        }

        public event Action<string> ValueChanged;
        
        public int Get(string id)
        {
            return _data.ContainsKey(id) ? _data[id] : default;
        }

        public bool Set(string id, int value)
        {
            var valueChanged = !value.Equals(default)
                ? UpdateValue(id, value)
                : RemoveValue(id);
            
            if (!valueChanged) return false;
            
            ValueChanged?.Invoke(id);
            return true;
        }

        private bool UpdateValue(string id, int value)
        {
            if (_data.ContainsKey(id) && _data[id].Equals(value)) return false;
            
            _data[id] = value;
            return true;
        }

        private bool RemoveValue(string id)
        {
            if (!_data.ContainsKey(id)) return false;

            _data.Remove(id);
            return true;
        }
    }
}