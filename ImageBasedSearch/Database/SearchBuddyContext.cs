using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ImageBasedSearch.Database
{
	public class SearchBuddyContext : IdentityDbContext<User>
	{
		public SearchBuddyContext(DbContextOptions<SearchBuddyContext> options) : base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);
			builder.Entity<User>()
				.HasIndex(u => u.ApiKey)
				.IsUnique();
		}
	}
}
