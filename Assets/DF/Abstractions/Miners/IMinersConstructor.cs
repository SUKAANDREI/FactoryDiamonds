using DF.Models.Miners;

namespace DF.Abstractions.Miners
{
    public interface IMinersConstructor : IConstructor<IMiner, MinerDefinition>
    {
    }
}