using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wplay.Models;

namespace Wplay.Data.Configurations;

public class EmailRequestConfiguration : IEntityTypeConfiguration<EmailRequest>
{
	public void Configure(EntityTypeBuilder<EmailRequest> builder)
	{
		// Indexes
		builder.HasIndex(e => e.Uuid).HasDatabaseName("email_requests_uuid_index");

		// Composite Index for real-time email listing
		builder.HasIndex(e => new { e.EndpointId, e.CreatedAt })
			.HasDatabaseName("email_requests_endpoint_created_index");

		// Column Constraints
		builder.Property(e => e.Sender).HasMaxLength(255);
		builder.Property(e => e.Recipient).HasMaxLength(255);
		builder.Property(e => e.Subject).HasMaxLength(512);

		// JSON Columns
		builder.Property(e => e.RawHeaders).HasColumnType("json");
		builder.Property(e => e.AttachmentsMetadata).HasColumnType("json");

		// Relationship & Cascade Delete
		builder.HasOne(e => e.Endpoint)
			.WithMany()
			.HasForeignKey(e => e.EndpointId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
