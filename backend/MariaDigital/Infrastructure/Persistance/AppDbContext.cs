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
		public DbSet<StudentDocument> StudentDocuments => Set<StudentDocument>();
		public DbSet<Admin> Admins => Set<Admin>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Student>(entity =>
			{
				entity.HasKey(e => e.Id);
			});

			modelBuilder.Entity<StudentDocument>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(e => e.Type).HasConversion<string>().HasMaxLength(32);
				entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(32);
				entity.Property(e => e.StorageKey).HasMaxLength(512);
				entity.Property(e => e.ContentType).HasMaxLength(128);

				// The webhook looks the row up by key.
				entity.HasIndex(e => e.StorageKey).IsUnique();
				// One file per document type per submission.
				entity.HasIndex(e => new { e.SubmissionId, e.Type }).IsUnique();
				// The job attaches a submission's documents to the student it creates.
				entity.HasIndex(e => e.StudentId);

				entity
					.HasOne(e => e.Student)
					.WithMany()
					.HasForeignKey(e => e.StudentId)
					.OnDelete(DeleteBehavior.Cascade);
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
