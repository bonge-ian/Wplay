using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wplay.Models;

namespace Wplay.Data.Configurations;

public class WebhookRequestConfiguration : IEntityTypeConfiguration<WebhookRequest>
{
	public void Configure(EntityTypeBuilder<WebhookRequest> builder)
	{
		// Indexes
		builder.HasIndex(w => w.Uuid).HasDatabaseName("webhook_requests_uuid_index");

		// Composite Index for real-time feed queries (order by CreatedAt descending per endpoint)
		builder.HasIndex(w => new { w.EndpointId, w.CreatedAt })
			.HasDatabaseName("webhook_requests_endpoint_created_index");

		// Column Constraints
		builder.Property(w => w.Method).HasMaxLength(10).HasDefaultValue("POST");
		builder.Property(w => w.Url).HasMaxLength(2048);
		builder.Property(w => w.IpAddress).HasMaxLength(45);
		builder.Property(w => w.ContentType).HasMaxLength(255);
		builder.Property(w => w.ResponseCode).HasDefaultValue(200);

		// JSON Columns
		builder.Property(w => w.Headers).HasColumnType("json");
		builder.Property(w => w.QueryParameters).HasColumnType("json");

		// Relationship & Cascade Delete
		builder.HasOne(w => w.Endpoint)
			.WithMany()
			.HasForeignKey(w => w.EndpointId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
