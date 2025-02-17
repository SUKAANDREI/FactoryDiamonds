namespace DF.Models.Miners
{
    public class MinerSpecification
    {
        public float MoveSpeed;
        public float MineDuration;
        
        public MinerSpecification(float moveSpeed, float mineDuration)
        {
            MoveSpeed = moveSpeed;
            MineDuration = mineDuration;
        } 
    }
}