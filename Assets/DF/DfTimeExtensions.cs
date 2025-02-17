using System;

namespace DF
{
    public static class DfTimeExtensions
    {
        public static double GetTimeLeft(this DfTime dfTime, double startTime, float duration)
        {
            var timeOut = startTime + duration;
            
            var result = timeOut - dfTime.GetTime();
            return Math.Max(0, result);
        }
    }
}