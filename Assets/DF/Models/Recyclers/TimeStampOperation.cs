namespace DF.Models.Recyclers
{
    public class TimeStampOperation
    {
        public double Time;
        public float TaskCompletedCoefficient;
        
        public TimeStampOperation(double time, float taskCompletedCoefficient)
        {
            Time = time;
            TaskCompletedCoefficient = taskCompletedCoefficient;
        }
    }
}