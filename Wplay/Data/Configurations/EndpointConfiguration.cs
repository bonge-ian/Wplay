using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wplay.Enums;

namespace Wplay.Data.Configurations;

public class EndpointConfiguration : IEntityTypeConfiguration<Models.Endpoint>
{
	public void Configure(EntityTypeBuilder<Models.Endpoint> builder)
	{
		// Indexes
		builder.HasIndex(e => e.Uuid).HasDatabaseName("endpoints_uuid_index");
		builder.HasIndex(e => e.UserId).HasDatabaseName("endpoints_userid_index");

		builder.Property(e => e.Type)
			.HasConversion<string>()
			.HasDefaultValue(EndpointType.Webhook)
			.HasMaxLength(20);

		// Default values
		builder.Property(e => e.DefaultStatusCode).HasDefaultValue(200);
		builder.Property(e => e.ResponseDelay)
			.HasComment("Response delay in milliseconds")
			.HasDefaultValue(0);

		builder.Property(e => e.IsProtected).HasDefaultValue(false);

		builder.Property(e => e.DefaultResponseHeaders).HasColumnType("json");

		builder.Property(e => e.AuthCredentials).HasColumnType("json");
	}
}
