namespace DF.Abstractions
{
    public interface IConstructor<out TModel, in TDefinition>
    {
        TModel Construct(TDefinition minerDefinition);
    }
}