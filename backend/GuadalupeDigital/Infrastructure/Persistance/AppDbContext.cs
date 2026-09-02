namespace GuadalupeDigital.Infrastructure.Persistance
{
    using GuadalupeDigital.Domain.Admin;
    using GuadalupeDigital.Domain.Students;
	using Microsoft.EntityFrameworkCore;

	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options)
			: base(options) { }

		public DbSet<Student> Students => Set<Student>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Student>(entity =>
			{
				entity.HasKey(e => e.Id);
			});

            modelBuilder.Entity<Admin>(entity =>
            {
                entity.HasKey(e => e.Id);

				entity.HasIndex(e => e.Email)
					.IsUnique();
            });
        }
	}
}
