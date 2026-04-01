using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore;

namespace SIFS.Infrastructure.Database
{
    public partial class SIFSContext
    {
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            ConfigureGuidAsBinary16(modelBuilder);
        }

        private static void ConfigureGuidAsBinary16(ModelBuilder modelBuilder)
        {
            var guidToBytesConverter = new ValueConverter<Guid, byte[]>(
                v => v.ToByteArray(),
                v => new Guid(v)
            );

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(Guid))
                    {
                        property.SetColumnType("binary(16)");
                        property.SetValueConverter(guidToBytesConverter);
                    }
                }
            }
        }
    }
}
