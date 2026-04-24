using Ardalis.SmartEnum;

namespace EntityTagCaching.Models;

public sealed class EntityTagStrength : SmartEnum<EntityTagStrength, int>
{
    public static readonly EntityTagStrength Strong = new(nameof(Strong), 1);
    public static readonly EntityTagStrength Weak = new(nameof(Weak), 2);

    private EntityTagStrength(string name, int value)
        : base(name, value)
    {
    }
}

