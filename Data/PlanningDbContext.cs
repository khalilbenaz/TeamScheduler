using Microsoft.EntityFrameworkCore;

namespace PlanningPresenceBlazor.Data
{
    public class PlanningDbContext : DbContext
    {
        public PlanningDbContext(DbContextOptions<PlanningDbContext> options) : base(options) { }

        public DbSet<Conge> Conges { get; set; }
        public DbSet<Employe> Employes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration des entités
            modelBuilder.Entity<Conge>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Raison).HasMaxLength(500);
                entity.HasIndex(e => new { e.Nom, e.DateDebut, e.DateFin });
            });

            modelBuilder.Entity<Employe>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.Property(e => e.Telephone).HasMaxLength(20);
                entity.Property(e => e.TeamsId).HasMaxLength(100);
                entity.HasIndex(e => e.Nom).IsUnique();
            });

            // Données de base
            modelBuilder.Entity<Employe>().HasData(
                new Employe { Id = 1, Nom = "Haytame", Email = "haytame@company.com", Telephone = "+212600000001", EstActif = true, NotificationEmail = true },
                new Employe { Id = 2, Nom = "Ayoub", Email = "ayoub@company.com", Telephone = "+212600000002", EstActif = true, NotificationEmail = true },
                new Employe { Id = 3, Nom = "Khalil", Email = "khalil@company.com", Telephone = "+212600000003", EstActif = true, NotificationEmail = true },
                new Employe { Id = 4, Nom = "Abdellah", Email = "abdellah@company.com", Telephone = "+212600000004", EstActif = true, NotificationEmail = true },
                new Employe { Id = 5, Nom = "Anouar", Email = "anouar@company.com", Telephone = "+212600000005", EstActif = true, NotificationEmail = true }
            );
        }
    }

    public class Employe
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public string? TeamsId { get; set; }
        public bool EstActif { get; set; } = true;
        public DateTime DateEmbauche { get; set; } = DateTime.Now;
        public bool NotificationEmail { get; set; } = true;
        public bool NotificationSMS { get; set; } = false;
        public bool NotificationTeams { get; set; } = false;
    }
}