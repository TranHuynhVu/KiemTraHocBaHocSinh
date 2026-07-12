using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TuyenSinh.Models;

namespace TuyenSinh.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<MonHoc> MonHocs { get; set; } = null!;
        public DbSet<ToHopMon> ToHopMons { get; set; } = null!;
        public DbSet<Nganh> Nganhs { get; set; } = null!;
        public DbSet<ToHopNganh> ToHopNganhs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure implicit many-to-many relationship with custom table name
            builder.Entity<ToHopMon>()
                .HasMany(t => t.MonHocs)
                .WithMany(m => m.ToHopMons)
                .UsingEntity(j => j.ToTable("ToHopMonMonHoc"));

            // Configure ToHopNganh
            builder.Entity<ToHopNganh>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(d => d.Nganh)
                    .WithMany(p => p.ToHopNganhs)
                    .HasForeignKey(d => d.MaNganhId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.ToHopMon)
                    .WithMany()
                    .HasForeignKey(d => d.ToHopId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
