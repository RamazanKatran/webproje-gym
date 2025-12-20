using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebProjeGym.Models;

namespace WebProjeGym.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<GymBranch> GymBranches { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<TrainerService> TrainerServices { get; set; }
        public DbSet<TrainerAvailability> TrainerAvailabilities { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<MemberProfile> MemberProfiles { get; set; }
        public DbSet<AIRecommendation> AIRecommendations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Trainer - Service çoktan çoğa
            builder.Entity<TrainerService>()
                .HasKey(ts => new { ts.TrainerId, ts.ServiceId });

            // TrainerService - Multiple cascade paths hatası nedeniyle Restrict
            // Manuel silme controller'larda yapılacak
            builder.Entity<TrainerService>()
                .HasOne(ts => ts.Trainer)
                .WithMany(t => t.TrainerServices)
                .HasForeignKey(ts => ts.TrainerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TrainerService>()
                .HasOne(ts => ts.Service)
                .WithMany(s => s.TrainerServices)
                .HasForeignKey(ts => ts.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Appointment ilişkileri - Multiple cascade paths hatası nedeniyle Restrict
            // Randevular controller'larda manuel olarak silinecek
            builder.Entity<Appointment>()
                .HasOne(a => a.Trainer)
                .WithMany(t => t.Appointments)
                .HasForeignKey(a => a.TrainerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(a => a.Service)
                .WithMany(s => s.Appointments)
                .HasForeignKey(a => a.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Üye silinince randevuları silinmesin (geçmiş kayıtlar için)
            builder.Entity<Appointment>()
                .HasOne(a => a.MemberProfile)
                .WithMany()
                .HasForeignKey(a => a.MemberProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            // Trainer - ApplicationUser ilişkisi
            builder.Entity<Trainer>()
                .HasOne(t => t.ApplicationUser)
                .WithMany()
                .HasForeignKey(t => t.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // AIRecommendation - ApplicationUser ilişkisi
            builder.Entity<AIRecommendation>()
                .HasOne(ar => ar.ApplicationUser)
                .WithMany()
                .HasForeignKey(ar => ar.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
