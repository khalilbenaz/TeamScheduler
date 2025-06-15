using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace PlanningPresenceBlazor.Data
{
    public class PlanningDbContext : DbContext
    {
        public PlanningDbContext(DbContextOptions<PlanningDbContext> options)
            : base(options)
        {
        }

        // Tables principales
        public DbSet<Employe> Employes { get; set; }
        public DbSet<Equipe> Equipes { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Conge> Conges { get; set; }
        public DbSet<Competence> Competences { get; set; }
        public DbSet<Presence> Presences { get; set; }
        public DbSet<Projet> Projets { get; set; }
        
        // Tables de relation
        public DbSet<AffectationEquipeClient> AffectationsEquipeClient { get; set; }
        public DbSet<AffectationEquipeProjet> AffectationsEquipeProjet { get; set; }
        public DbSet<PlanningEquipeClient> PlanningsEquipeClient { get; set; }
        public DbSet<EmployeCompetence> EmployeCompetences { get; set; }
        public DbSet<ClientCompetenceRequise> ClientCompetencesRequises { get; set; }
        public DbSet<HistoriqueAffectation> HistoriquesAffectation { get; set; }
        
        // Configuration
        public DbSet<ConfigurationPlanning> ConfigurationsPlanning { get; set; }
        public DbSet<PlanningPresenceBlazor.Core.Entities.UserSettings> UserSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration Employe
            modelBuilder.Entity<Employe>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(150);
                entity.Property(e => e.Telephone).HasMaxLength(20);
                entity.Property(e => e.TeamsId).HasMaxLength(150);
                entity.Property(e => e.Poste).HasMaxLength(100);
                
                entity.HasOne(e => e.Equipe)
                    .WithMany(eq => eq.Membres)
                    .HasForeignKey(e => e.EquipeId)
                    .OnDelete(DeleteBehavior.SetNull);
                
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Nom);
                entity.HasIndex(e => e.EquipeId);
            });

            // Configuration Equipe
            modelBuilder.Entity<Equipe>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CodeEquipe).HasMaxLength(20);
                entity.Property(e => e.Description).HasMaxLength(500);
                
                entity.HasOne(e => e.ChefEquipe)
                    .WithMany()
                    .HasForeignKey(e => e.ChefEquipeId)
                    .OnDelete(DeleteBehavior.SetNull);
                
                entity.HasIndex(e => e.CodeEquipe).IsUnique();
                entity.HasIndex(e => e.Nom);
            });

            // Configuration Client
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Nom).IsRequired().HasMaxLength(200);
                entity.Property(c => c.CodeClient).HasMaxLength(50);
                entity.Property(c => c.Adresse).HasMaxLength(500);
                entity.Property(c => c.Ville).HasMaxLength(100);
                entity.Property(c => c.CodePostal).HasMaxLength(20);
                entity.Property(c => c.Pays).HasMaxLength(100);
                entity.Property(c => c.ContactPrincipal).HasMaxLength(100);
                entity.Property(c => c.EmailContact).HasMaxLength(150);
                entity.Property(c => c.TelephoneContact).HasMaxLength(20);
                
                entity.HasIndex(c => c.CodeClient).IsUnique();
                entity.HasIndex(c => c.Nom);
                entity.HasIndex(c => c.EstActif);
            });

            // Configuration Conge
            modelBuilder.Entity<Conge>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Nom).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Raison).HasMaxLength(500);
                entity.Property(c => c.ValidePar).HasMaxLength(100);
                entity.Property(c => c.CommentaireValidation).HasMaxLength(500);
                
                entity.HasOne(c => c.Employe)
                    .WithMany()
                    .HasForeignKey(c => c.EmployeId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(c => c.EmployeId);
                entity.HasIndex(c => new { c.DateDebut, c.DateFin });
                entity.HasIndex(c => c.Status);
            });

            // Configuration Competence
            modelBuilder.Entity<Competence>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Nom).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Description).HasMaxLength(500);
                
                entity.HasIndex(c => c.Nom).IsUnique();
                entity.HasIndex(c => c.Categorie);
            });

            // Configuration AffectationEquipeClient
            modelBuilder.Entity<AffectationEquipeClient>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Notes).HasMaxLength(500);
                
                entity.HasOne(a => a.Equipe)
                    .WithMany(e => e.Affectations)
                    .HasForeignKey(a => a.EquipeId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(a => a.Client)
                    .WithMany(c => c.Affectations)
                    .HasForeignKey(a => a.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasIndex(a => new { a.EquipeId, a.ClientId, a.DateDebut });
                entity.HasIndex(a => a.EstActive);
            });

            // Configuration AffectationEquipeProjet
            modelBuilder.Entity<AffectationEquipeProjet>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Notes).HasMaxLength(500);
                
                entity.HasOne(a => a.Equipe)
                    .WithMany(e => e.AffectationsProjets)
                    .HasForeignKey(a => a.EquipeId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(a => a.Projet)
                    .WithMany(p => p.Affectations)
                    .HasForeignKey(a => a.ProjetId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasIndex(a => new { a.EquipeId, a.ProjetId })
                    .IsUnique()
                    .HasDatabaseName("IX_AffectationEquipeProjet_EquipeProjet");
            });

            // Configuration Projet
            modelBuilder.Entity<Projet>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Nom).IsRequired().HasMaxLength(200);
                entity.Property(p => p.Description).HasMaxLength(1000);
                
                entity.HasOne(p => p.Client)
                    .WithMany()
                    .HasForeignKey(p => p.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasIndex(p => p.Nom);
                entity.HasIndex(p => p.ClientId);
            });

            // Configuration PlanningEquipeClient
            modelBuilder.Entity<PlanningEquipeClient>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.ValidePar).HasMaxLength(100);
                
                entity.HasOne(p => p.Affectation)
                    .WithMany(a => a.Plannings)
                    .HasForeignKey(p => p.AffectationId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(p => new { p.AffectationId, p.DatePlanning });
                entity.HasIndex(p => new { p.Annee, p.SemaineNumero });
                entity.HasIndex(p => p.Status);
            });

            // Configuration EmployeCompetence (Many-to-Many)
            modelBuilder.Entity<EmployeCompetence>(entity =>
            {
                entity.HasKey(ec => new { ec.EmployeId, ec.CompetenceId });
                
                entity.HasOne(ec => ec.Employe)
                    .WithMany(e => e.Competences)
                    .HasForeignKey(ec => ec.EmployeId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(ec => ec.Competence)
                    .WithMany(c => c.Employes)
                    .HasForeignKey(ec => ec.CompetenceId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(ec => ec.Niveau);
                entity.HasIndex(ec => ec.DateExpiration);
            });

            // Configuration ClientCompetenceRequise (Many-to-Many)
            modelBuilder.Entity<ClientCompetenceRequise>(entity =>
            {
                entity.HasKey(cc => new { cc.ClientId, cc.CompetenceId });
                
                entity.HasOne(cc => cc.Client)
                    .WithMany()
                    .HasForeignKey(cc => cc.ClientId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(cc => cc.Competence)
                    .WithMany(c => c.ClientsRequis)
                    .HasForeignKey(cc => cc.CompetenceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuration HistoriqueAffectation
            modelBuilder.Entity<HistoriqueAffectation>(entity =>
            {
                entity.HasKey(h => h.Id);
                entity.Property(h => h.Motif).HasMaxLength(200);
                entity.Property(h => h.Notes).HasMaxLength(500);
                entity.Property(h => h.CreePar).HasMaxLength(100);
                
                entity.HasOne(h => h.Employe)
                    .WithMany(e => e.Historiques)
                    .HasForeignKey(h => h.EmployeId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(h => h.Equipe)
                    .WithMany()
                    .HasForeignKey(h => h.EquipeId)
                    .OnDelete(DeleteBehavior.SetNull);
                
                entity.HasOne(h => h.Client)
                    .WithMany()
                    .HasForeignKey(h => h.ClientId)
                    .OnDelete(DeleteBehavior.SetNull);
                
                entity.HasIndex(h => h.EmployeId);
                entity.HasIndex(h => h.DateDebut);
            });

            // Configuration ConfigurationPlanning
            modelBuilder.Entity<ConfigurationPlanning>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Nom).IsRequired().HasMaxLength(100);
                entity.Property(c => c.ModifiePar).HasMaxLength(100);
                
                entity.HasIndex(c => c.EstActive);
            });

            // Configuration Employee (rôle)
            modelBuilder.Entity<TeamScheduler.Core.Entities.Employee>(entity =>
            {
                entity.Property(e => e.Role).IsRequired().HasMaxLength(30);
            });

            // Relation explicite Employee <-> Team
            modelBuilder.Entity<TeamScheduler.Core.Entities.Employee>()
                .HasOne(e => e.Team)
                .WithMany(t => t.Members)
                .HasForeignKey(e => e.TeamId)
                .OnDelete(DeleteBehavior.SetNull);

            // Clé composite pour ClientRequiredSkill
            modelBuilder.Entity<TeamScheduler.Core.Entities.ClientRequiredSkill>()
                .HasKey(crs => new { crs.ClientId, crs.SkillId });

            // Clé composite pour EmployeeSkill
            modelBuilder.Entity<TeamScheduler.Core.Entities.EmployeeSkill>()
                .HasKey(es => new { es.EmployeeId, es.SkillId });

            // Seed data - Configuration par défaut
            modelBuilder.Entity<ConfigurationPlanning>().HasData(
                new ConfigurationPlanning
                {
                    Id = 1,
                    Nom = "Configuration par défaut",
                    EstActive = true,
                    PresencesMinParPersonne = 3,
                    PresencesMaxParPersonne = 5,
                    PresencesMinParJour = 2,
                    PresencesMaxParJour = 4,
                    JoursCritiques = "[\"Lundi\",\"Mardi\",\"Vendredi\"]",
                    PresencesMinJoursCritiques = 2,
                    PresencesMaxJoursCritiques = 3,
                    JoursFlexibles = "[\"Mercredi\",\"Jeudi\"]",
                    PresencesMinJoursFlexibles = 0,
                    PresencesMaxJoursFlexibles = 3,
                    RotationEquitable = true,
                    RespectCompetences = true,
                    OptimiserDeplacements = true,
                    DelaiNotificationJours = 7,
                    DateCreation = new DateTime(2025, 1, 1)
                }
            );

            // Seed data - Compétences de base
            modelBuilder.Entity<Competence>().HasData(
                new Competence { Id = 1, Nom = "Développement Web", Categorie = CategorieCompetence.Technique },
                new Competence { Id = 2, Nom = "Support Technique", Categorie = CategorieCompetence.Technique },
                new Competence { Id = 3, Nom = "Gestion de Projet", Categorie = CategorieCompetence.Fonctionnelle },
                new Competence { Id = 4, Nom = "Anglais", Categorie = CategorieCompetence.Linguistique },
                new Competence { Id = 5, Nom = "Permis B", Categorie = CategorieCompetence.Permis },
                new Competence { Id = 6, Nom = "ITIL", Categorie = CategorieCompetence.Certification },
                new Competence { Id = 7, Nom = "Sécurité Informatique", Categorie = CategorieCompetence.Technique },
                new Competence { Id = 8, Nom = "Administration Système", Categorie = CategorieCompetence.Technique }
            );
        }

        // Méthode pour initialiser la base de données avec des données de test
        public async Task InitializeTestDataAsync()
        {
            // Vérifier si des données existent déjà
            if (await Employes.AnyAsync())
                return;

            // Créer des équipes
            var equipe1 = new Equipe
            {
                Nom = "Équipe Développement",
                CodeEquipe = "DEV01",
                Description = "Équipe de développement web et mobile",
                PresencesMinParJour = 2,
                PresencesMaxParJour = 3,
                PresencesMinParPersonne = 3,
                PresencesMaxParPersonne = 4
            };

            var equipe2 = new Equipe
            {
                Nom = "Équipe Support",
                CodeEquipe = "SUP01",
                Description = "Équipe de support technique et maintenance",
                PresencesMinParJour = 2,
                PresencesMaxParJour = 4,
                PresencesMinParPersonne = 3,
                PresencesMaxParPersonne = 5
            };

            var equipe3 = new Equipe
            {
                Nom = "Équipe Infrastructure",
                CodeEquipe = "INF01",
                Description = "Équipe infrastructure et réseaux",
                PresencesMinParJour = 1,
                PresencesMaxParJour = 2,
                PresencesMinParPersonne = 3,
                PresencesMaxParPersonne = 4
            };

            await Equipes.AddRangeAsync(equipe1, equipe2, equipe3);
            await SaveChangesAsync();

            // Créer des employés
            var employes = new List<Employe>
            {
                // Équipe Développement
                new Employe { Nom = "Haytame Benali", Email = "haytame@company.ma", Telephone = "+212600000001", EquipeId = equipe1.Id, Poste = "Développeur Senior", NiveauCompetence = 4 },
                new Employe { Nom = "Ayoub El Amrani", Email = "ayoub@company.ma", Telephone = "+212600000002", EquipeId = equipe1.Id, Poste = "Développeur", NiveauCompetence = 3 },
                new Employe { Nom = "Khalil Moussaoui", Email = "khalil@company.ma", Telephone = "+212600000003", EquipeId = equipe1.Id, Poste = "Développeur Junior", NiveauCompetence = 2 },
                new Employe { Nom = "Yasmine Tahiri", Email = "yasmine@company.ma", Telephone = "+212600000010", EquipeId = equipe1.Id, Poste = "Développeuse", NiveauCompetence = 3 },
                
                // Équipe Support
                new Employe { Nom = "Sara Bennis", Email = "sara@company.ma", Telephone = "+212600000004", EquipeId = equipe2.Id, Poste = "Support Technique", NiveauCompetence = 3 },
                new Employe { Nom = "Omar Tazi", Email = "omar@company.ma", Telephone = "+212600000005", EquipeId = equipe2.Id, Poste = "Support Senior", NiveauCompetence = 4 },
                new Employe { Nom = "Mehdi Alaoui", Email = "mehdi@company.ma", Telephone = "+212600000006", EquipeId = equipe2.Id, Poste = "Support", NiveauCompetence = 2 },
                new Employe { Nom = "Laila Benjelloun", Email = "laila@company.ma", Telephone = "+212600000011", EquipeId = equipe2.Id, Poste = "Support Senior", NiveauCompetence = 4 },
                
                // Équipe Infrastructure
                new Employe { Nom = "Ahmed Fassi", Email = "ahmed@company.ma", Telephone = "+212600000007", EquipeId = equipe3.Id, Poste = "Admin Système", NiveauCompetence = 4 },
                new Employe { Nom = "Nadia Berrada", Email = "nadia@company.ma", Telephone = "+212600000008", EquipeId = equipe3.Id, Poste = "Admin Réseau", NiveauCompetence = 3 },
                new Employe { Nom = "Rachid Idrissi", Email = "rachid@company.ma", Telephone = "+212600000009", EquipeId = equipe3.Id, Poste = "Technicien Infrastructure", NiveauCompetence = 2 }
            };

            await Employes.AddRangeAsync(employes);
            await SaveChangesAsync();

            // Affecter les chefs d'équipe
            equipe1.ChefEquipeId = employes.First(e => e.Nom == "Haytame Benali").Id;
            equipe2.ChefEquipeId = employes.First(e => e.Nom == "Omar Tazi").Id;
            equipe3.ChefEquipeId = employes.First(e => e.Nom == "Ahmed Fassi").Id;
            await SaveChangesAsync();

            // Créer des clients
            var clients = new List<Client>
            {
                new Client
                {
                    Nom = "Bank Al Maghrib",
                    CodeClient = "BAM001",
                    Adresse = "277 Avenue Mohammed V",
                    Ville = "Rabat",
                    CodePostal = "10000",
                    ContactPrincipal = "Ahmed Benjelloun",
                    EmailContact = "contact@bam.ma",
                    TelephoneContact = "+212537000000",
                    DistanceKm = 90,
                    TempsDeplacement = 75
                },
                new Client
                {
                    Nom = "OCP Group",
                    CodeClient = "OCP001",
                    Adresse = "2 Rue Al Abtal, Hay Erraha",
                    Ville = "Casablanca",
                    CodePostal = "20200",
                    ContactPrincipal = "Fatima Zahra El Idrissi",
                    EmailContact = "contact@ocpgroup.ma",
                    TelephoneContact = "+212522000000",
                    DistanceKm = 10,
                    TempsDeplacement = 20
                },
                new Client
                {
                    Nom = "Royal Air Maroc",
                    CodeClient = "RAM001",
                    Adresse = "Aéroport Mohammed V",
                    Ville = "Casablanca",
                    CodePostal = "20250",
                    ContactPrincipal = "Mohammed Alami",
                    EmailContact = "contact@royalairmaroc.com",
                    TelephoneContact = "+212522539040",
                    DistanceKm = 30,
                    TempsDeplacement = 35
                },
                new Client
                {
                    Nom = "Maroc Telecom",
                    CodeClient = "IAM001",
                    Adresse = "Avenue Annakhil, Hay Riad",
                    Ville = "Rabat",
                    CodePostal = "10100",
                    ContactPrincipal = "Karim Zidane",
                    EmailContact = "contact@iam.ma",
                    TelephoneContact = "+212537100000",
                    DistanceKm = 85,
                    TempsDeplacement = 70
                }
            };

            await Clients.AddRangeAsync(clients);
            await SaveChangesAsync();

            // Créer des affectations
            var affectations = new List<AffectationEquipeClient>
            {
                // Bank Al Maghrib
                new AffectationEquipeClient
                {
                    EquipeId = equipe1.Id,
                    ClientId = clients[0].Id,
                    DateDebut = DateTime.Today.AddMonths(-3),
                    TypeMission = TypeMission.Projet,
                    NombreMinPersonnes = 2,
                    NombreMaxPersonnes = 3,
                    Notes = "Projet de modernisation du système bancaire"
                },
                
                // OCP Group
                new AffectationEquipeClient
                {
                    EquipeId = equipe2.Id,
                    ClientId = clients[1].Id,
                    DateDebut = DateTime.Today.AddMonths(-6),
                    TypeMission = TypeMission.Support,
                    NombreMinPersonnes = 2,
                    NombreMaxPersonnes = 2,
                    Notes = "Support et maintenance des applications"
                },
                new AffectationEquipeClient
                {
                    EquipeId = equipe3.Id,
                    ClientId = clients[1].Id,
                    DateDebut = DateTime.Today.AddMonths(-4),
                    TypeMission = TypeMission.Regulier,
                    NombreMinPersonnes = 1,
                    NombreMaxPersonnes = 2,
                    Notes = "Maintenance infrastructure serveurs"
                },
                
                // Royal Air Maroc
                new AffectationEquipeClient
                {
                    EquipeId = equipe1.Id,
                    ClientId = clients[2].Id,
                    DateDebut = DateTime.Today.AddMonths(-2),
                    TypeMission = TypeMission.Projet,
                    NombreMinPersonnes = 1,
                    NombreMaxPersonnes = 2,
                    Notes = "Développement application mobile"
                },
                
                // Maroc Telecom
                new AffectationEquipeClient
                {
                    EquipeId = equipe2.Id,
                    ClientId = clients[3].Id,
                    DateDebut = DateTime.Today.AddMonths(-1),
                    TypeMission = TypeMission.Support,
                    NombreMinPersonnes = 1,
                    NombreMaxPersonnes = 2,
                    Notes = "Support niveau 2 applications métier"
                }
            };

            await AffectationsEquipeClient.AddRangeAsync(affectations);
            await SaveChangesAsync();

            // Ajouter des compétences aux employés
            var competences = await Competences.ToListAsync();
            if (competences.Any())
            {
                var employeCompetences = new List<EmployeCompetence>();
                
                // Développeurs
                var devs = employes.Where(e => e.Poste != null && e.Poste.Contains("Développ")).ToList();
                foreach (var dev in devs)
                {
                    employeCompetences.Add(new EmployeCompetence
                    {
                        EmployeId = dev.Id,
                        CompetenceId = competences.First(c => c.Nom == "Développement Web").Id,
                        Niveau = dev.NiveauCompetence,
                        EstCertifie = dev.NiveauCompetence >= 3
                    });
                }

                // Support
                var supports = employes.Where(e => e.Poste != null && e.Poste.Contains("Support")).ToList();
                foreach (var support in supports)
                {
                    employeCompetences.Add(new EmployeCompetence
                    {
                        EmployeId = support.Id,
                        CompetenceId = competences.First(c => c.Nom == "Support Technique").Id,
                        Niveau = support.NiveauCompetence,
                        EstCertifie = support.NiveauCompetence >= 3
                    });
                }

                // Ajout de l'anglais pour certains
                var englishSpeakers = employes.Where(e => e.NiveauCompetence >= 3).Take(5).ToList();
                foreach (var emp in englishSpeakers)
                {
                    employeCompetences.Add(new EmployeCompetence
                    {
                        EmployeId = emp.Id,
                        CompetenceId = competences.First(c => c.Nom == "Anglais").Id,
                        Niveau = 3
                    });
                }

                await EmployeCompetences.AddRangeAsync(employeCompetences);
                await SaveChangesAsync();
            }
        }
    }
}