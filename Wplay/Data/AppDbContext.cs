using Microsoft.EntityFrameworkCore;
using Wplay.Models;

namespace Wplay.Data;

public class AppDbContext(DbContextOptions<AppDbContext> contextOptions) : DbContext(contextOptions)
{
	#region DBSets
	public DbSet<Models.Endpoint> Endpoints { get; set; }

	public DbSet<WebhookRequest> WebhookRequests { get; set; }

	public DbSet<EmailRequest> EmailRequests { get; set; }
	#endregion

	public override int SaveChanges()
	{
		UpdateTimestamps();
		return base.SaveChanges();
	}

	public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		UpdateTimestamps();
		return base.SaveChangesAsync(cancellationToken);
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
	}

	private void UpdateTimestamps()
	{
		var entries = ChangeTracker.Entries()
			.Where(e => e.Entity is BaseModel &&
					(e.State == EntityState.Added || e.State == EntityState.Modified)
			);

		foreach (var entry in entries)
		{
			var entity = (BaseModel)entry.Entity;

			if (entry.State == EntityState.Added)
			{
				entity.CreatedAt = DateTime.UtcNow;
			}

			entity.UpdatedAt = DateTime.UtcNow;
		}
	}
}
