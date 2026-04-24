using Ardalis.SmartEnum;
using Ardalis.SmartEnum.SystemTextJson;

using System.Text.Json.Serialization;

namespace HypermediaEngine.Responses;

/// <summary>
/// Represents standard IANA link relation types used in hypermedia responses.
/// Provides strongly-typed relation names based on RFC 8288 and common REST conventions.
/// </summary>
[JsonConverter(typeof(SmartEnumNameConverter<LinkRelations, int>))]
public sealed class LinkRelations : SmartEnum<LinkRelations, int>
{
    /// <summary>A link to the current resource itself.</summary>
    public static readonly LinkRelations Self = new("self", 0);

    /// <summary>A link to the collection that contains the current resource.</summary>
    public static readonly LinkRelations Collection = new("collection", 1);

    /// <summary>A link to an individual item within a collection.</summary>
    public static readonly LinkRelations Item = new("item", 2);

    /// <summary>A link to the next page in a paginated collection.</summary>
    public static readonly LinkRelations Next = new("next", 3);

    /// <summary>A link to the previous page in a paginated collection.</summary>
    public static readonly LinkRelations Previous = new("prev", 4);

    /// <summary>A link to the first page in a paginated collection.</summary>
    public static readonly LinkRelations First = new("first", 5);

    /// <summary>A link to the last page in a paginated collection.</summary>
    public static readonly LinkRelations Last = new("last", 6);

    /// <summary>A link to the endpoint for creating a new resource.</summary>
    public static readonly LinkRelations Create = new("create", 7);

    /// <summary>A link to the endpoint for fully replacing the current resource.</summary>
    public static readonly LinkRelations Update = new("update", 8);

    /// <summary>A link to the endpoint for partially updating the current resource.</summary>
    public static readonly LinkRelations PartialUpdate = new("partial_update", 9);

    /// <summary>A link to the endpoint for deleting the current resource.</summary>
    public static readonly LinkRelations Delete = new("delete", 10);

    internal LinkRelations() : base(string.Empty, -1) { }

    private LinkRelations(string name, int id)
        : base(name, id) { }

    /// <summary>Implicitly converts a <see cref="LinkRelations"/> to its string name, or <see langword="null"/> if the instance is null.</summary>
    public static implicit operator string?(LinkRelations? rel) => rel?.Name;

    /// <summary>Implicitly converts a <see cref="LinkRelations"/> to its integer value, or <see langword="null"/> if the instance is null.</summary>
    public static implicit operator int?(LinkRelations? rel) => rel?.Value;

    /// <summary>
    /// Implicitly converts a string to a <see cref="LinkRelations"/> instance.
    /// Returns <see langword="null"/> for null or whitespace input; otherwise creates a new relation with the next available value.
    /// </summary>
    public static implicit operator LinkRelations?(string? rel)
        => string.IsNullOrWhiteSpace(rel)
         ? null
         : new(rel, List.Last().Value + 1);
}
