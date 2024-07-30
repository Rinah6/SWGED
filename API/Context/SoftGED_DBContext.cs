using Microsoft.EntityFrameworkCore;
using API.Model;

namespace API.Context
{
    public class SoftGED_DBContext : DbContext
    {
        public SoftGED_DBContext(DbContextOptions<SoftGED_DBContext> options) : base(options) { }

        public DbSet<Project> Projects { get; set; }
        public DbSet<Site> Sites { get; set; }
        //public DbSet<Soa> Soas { get; set; }
        public virtual DbSet<Soa> Soas { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Attachement> Attachements { get; set; }
        public virtual DbSet<UsersProjectsSites> UsersProjectsSites { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Document>().Property(x => x.Id).HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<DocumentDynamicField>().HasKey(e => new { e.DocumentId, e.DynamicFieldId });
            modelBuilder.Entity<Project>().Property(x => x.Id).HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<User>().Property(x => x.Id).HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<DynamicField>().Property(x => x.Id).HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<DynamicFieldItem>().Property(x => x.Id).HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<Attachement>().Property(x => x.Id).HasDefaultValueSql("NEWID()");

            modelBuilder.Entity<Soa>(entity =>
            {
                entity.Property(e => e.CreationDate).HasColumnType("datetime");
                entity.Property(e => e.DeletionDate).HasColumnType("datetime");
            });
        }
    }
}
