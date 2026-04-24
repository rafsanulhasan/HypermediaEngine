namespace HypermediaEngine.Abstractions;

public interface ILinkHandler<T, TBuilder>
    where T : notnull
    where TBuilder : IHypermediaBuilder<T>  
{
    TBuilder Handle(IEnumerable<HateoasLinkAttribute> attributes);
}
