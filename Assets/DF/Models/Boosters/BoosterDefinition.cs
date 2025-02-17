using System.Collections.Generic;

namespace DF.Models.Boosters
{
    public class BoosterDefinition
    {
        public string Id;
        public float Duration;
        public List<BuffDefinition> BuffData;

        public BoosterDefinition(string id, float duration, List<BuffDefinition> buffData)
        {
            Id = id;
            Duration = duration;
            BuffData = buffData;
        }
    }
}