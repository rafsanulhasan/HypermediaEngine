using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

using System.Globalization;
using System.Text;

namespace HypermediaEngine.OpenApi;

internal sealed class OneOfSchemaTransformer
    : IOpenApiSchemaTransformer
{
    public async Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        Type type = context.JsonTypeInfo.Type;

        bool isOneOf = type.Name.StartsWith("OneOf", StringComparison.Ordinal);
        bool extendsOneOf = type is { BaseType: Type baseType }
                         && baseType.Name.StartsWith("OneOfBase", StringComparison.Ordinal);

        if (!isOneOf && !extendsOneOf)
        {
            return;
        }

        Type[] genericArguments = isOneOf
                                ? type.GetGenericArguments()
                                : type.BaseType!.GetGenericArguments();

        // Clear the default object properties
        schema.Properties = null;
        schema.Type = null;
        schema.OneOf = [];

        // Map each generic argument to a sub-schema in the 'oneOf' array
        OpenApiDiscriminator discriminator = new()
        {
            PropertyName = "type",
            Mapping = new Dictionary<string, OpenApiSchemaReference>(StringComparer.OrdinalIgnoreCase),
        };
        int i = 0;
        StringBuilder schemaTitleBuilder = new();
        foreach (Type argType in genericArguments)
        {
            // Generate the schema for each member of the union
            OpenApiSchema subSchema = await context
                .GetOrCreateSchemaAsync(argType, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            string subTitle = argType.Name;
            subSchema.Title = subTitle;
            schema.OneOf.Add(subSchema);
            if (i < genericArguments.Length - 1)
                schemaTitleBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}Or", subTitle);
            else
                schemaTitleBuilder.Append(subTitle);
            i++;
        }
        schema.Title = schemaTitleBuilder.ToString();
        schema.Discriminator = discriminator;
    }
}

