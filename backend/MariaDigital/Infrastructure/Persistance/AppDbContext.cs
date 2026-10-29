namespace MariaDigital.Infrastructure.Persistance
{
	using MariaDigital.Domain.Admin;
	using MariaDigital.Domain.Students;
	using Microsoft.EntityFrameworkCore;

	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options)
			: base(options) { }

		public DbSet<Student> Students => Set<Student>();
		public DbSet<Admin> Admins => Set<Admin>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Student>(entity =>
			{
				entity.HasKey(e => e.Id);
			});

			modelBuilder.Entity<Admin>(entity =>
			{
				entity.ToTable("Admin");
				entity.HasKey(e => e.Id);

				entity.HasIndex(e => e.Email).IsUnique();
			});
		}
	}
}
