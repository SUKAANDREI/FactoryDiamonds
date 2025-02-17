namespace DF.Models.Miners
{
    public class TimeStampOperation
    {
        public double Time;
        public MinerState MinerState;
        public float TaskCompletedCoefficient;

        public TimeStampOperation(double time, MinerState minerState, float taskCompletedCoefficient)
        {
            Time = time;
            MinerState = minerState;
            TaskCompletedCoefficient = taskCompletedCoefficient;
        }
    }
}