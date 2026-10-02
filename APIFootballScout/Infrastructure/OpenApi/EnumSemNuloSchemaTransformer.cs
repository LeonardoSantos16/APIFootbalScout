using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Text.Json;

namespace APIFootballScout.Infrastructure.OpenApi
{
    public sealed class EnumSemNuloSchemaTransformer : IOpenApiSchemaTransformer
    {
        public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
        {
            var tipo = Nullable.GetUnderlyingType(context.JsonTypeInfo.Type) ?? context.JsonTypeInfo.Type;

            if (schema.Enum is { Count: > 0 } valores && tipo.IsEnum)
            {
                var semNulo = valores
                    .Where(valor => valor is not null && valor.GetValueKind() != JsonValueKind.Null)
                    .ToList();

                schema.Enum = semNulo;

                if (semNulo.All(valor => valor!.GetValueKind() == JsonValueKind.String))
                    schema.Type = JsonSchemaType.String;
            }

            return Task.CompletedTask;
        }
    }
}
