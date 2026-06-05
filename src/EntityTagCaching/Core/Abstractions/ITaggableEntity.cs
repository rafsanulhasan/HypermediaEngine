namespace SynergyFx.EntityTagCaching.Abstractions;

public interface ITaggableEntity
{
    int WriteBinary(Span<byte> dest);
}

