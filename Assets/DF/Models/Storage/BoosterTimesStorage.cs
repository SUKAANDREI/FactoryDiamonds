using System;
using System.Collections.Generic;
using DF.Abstractions.Storage;

namespace DF.Models.Storage
{
    public class BoosterTimesStorage : IBoosterTimesStorage
    {
        private readonly Dictionary<string, double> _data;

        public BoosterTimesStorage()
        {
            _data = new Dictionary<string, double>(3);
        }

        public event Action<string> ValueChanged;
        
        public double Get(string id)
        {
            return _data.ContainsKey(id) ? _data[id] : default;
        }

        public bool Set(string id, double value)
        {
            var valueChanged = !value.Equals(default)
                ? UpdateValue(id, value)
                : RemoveValue(id);
            
            if (!valueChanged) return false;
            
            ValueChanged?.Invoke(id);
            return true;
        }

        private bool UpdateValue(string id, double value)
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