namespace DF.Models.Miners
{
    public class MinerDefinition
    {
        public MinerSpecification MinerSpecification;
        public string Id;
        public float ShaftDistance;

        public MinerDefinition(MinerSpecification minerSpecification, string id, float shaftDistance)
        {
            MinerSpecification = minerSpecification;
            Id = id;
            ShaftDistance = shaftDistance;
        } 
    }
}