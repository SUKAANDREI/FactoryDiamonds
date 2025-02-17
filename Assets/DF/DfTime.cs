using UnityEngine;

namespace DF
{
    public sealed class DfTime
    {
        private double _incrementTime;

        public double GetTime() => Time.time + _incrementTime;

        public void SkipTime(double period)
        {
            _incrementTime += period;
        }
    }
}