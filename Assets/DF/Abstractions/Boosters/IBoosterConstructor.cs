using DF.Models.Boosters;

namespace DF.Abstractions.Boosters
{
    public interface IBoosterConstructor : IConstructor<IBooster, BoosterDefinition>
    {
    }
}