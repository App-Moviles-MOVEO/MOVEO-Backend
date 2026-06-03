using Microsoft.EntityFrameworkCore;
using Moveo_backend.Rental.Domain.Model.Aggregates;
using Moveo_backend.Rental.Infrastructure.Persistence.EFC.Configuration;

namespace Moveo_backend.Rental.Infrastructure.Persistence.EFC.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyRentalConfiguration(this ModelBuilder builder)
    {
        // Entity configurations are applied in AppDbContext.OnModelCreating
        builder.ApplyConfiguration(new VehicleEntityTypeConfiguration());
    }

    /// <summary>
    /// Convierte todos los nombres de tablas y columnas a snake_case (opcional).
    /// </summary>
    public static void UseSnakeCaseNamingConvention(this ModelBuilder builder)
    {
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            entity.SetTableName(ToSnakeCase(entity.GetTableName() ?? string.Empty));

            foreach (var property in entity.GetProperties())
                property.SetColumnName(ToSnakeCase(property.GetColumnName() ?? string.Empty));

            foreach (var key in entity.GetKeys())
                key.SetName(ToSnakeCase(key.GetName() ?? string.Empty));

            foreach (var foreignKey in entity.GetForeignKeys())
                foreignKey.SetConstraintName(ToSnakeCase(foreignKey.GetConstraintName() ?? string.Empty));

            foreach (var index in entity.GetIndexes())
                index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName() ?? string.Empty));
        }
    }

    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var chars = new List<char>();
        for (int i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (char.IsUpper(c))
            {
                if (i > 0) chars.Add('_');
                chars.Add(char.ToLowerInvariant(c));
            }
            else
            {
                chars.Add(c);
            }
        }
        return new string(chars.ToArray());
    }
}
