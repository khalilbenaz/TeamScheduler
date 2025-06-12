using System.ComponentModel.DataAnnotations;

namespace PlanningPresenceBlazor.Data
{
    // Entités existantes modifiées
    public class Employe
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Le nom est requis")]
        public string Nom { get; set; } = string.Empty;
        
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public string? TeamsId { get; set; }
        
        public bool EstActif { get; set; } = true;
        public DateTime DateEmbauche { get; set; } = DateTime.Today;
        public DateTime? DateCreation { get; set; } = DateTime.Now;
        
        public bool NotificationEmail { get; set; } = true;
        public bool NotificationSMS { get; set; } = false;
        public bool NotificationTeams { get; set; } = false;
        
        // Nouvelles propriétés
        public int? EquipeId { get; set; }
        public virtual Equipe? Equipe { get; set; }
        
        public string? Poste { get; set; }
        public int NiveauCompetence { get; set; } = 1; // 1-5
        public int PresencesMinSemaine { get; set; } = 3; // Configuration individuelle
        public int PresencesMaxSemaine { get; set; } = 5;
        
        // Relations
        public virtual ICollection<EmployeCompetence> Competences { get; set; } = new List<EmployeCompetence>();
        public virtual ICollection<HistoriqueAffectation> Historiques { get; set; } = new List<HistoriqueAffectation>();
    }

    // Nouvelle entité Équipe
    public class Equipe
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Le nom de l'équipe est requis")]
        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public string? CodeEquipe { get; set; } // Code unique
        public bool EstActive { get; set; } = true;
        public DateTime DateCreation { get; set; } = DateTime.Now;
        
        // Chef d'équipe
        public int? ChefEquipeId { get; set; }
        public virtual Employe? ChefEquipe { get; set; }
        
        // Configuration spécifique à l'équipe
        public int PresencesMinParJour { get; set; } = 2;
        public int PresencesMaxParJour { get; set; } = 4;
        public int PresencesMinParPersonne { get; set; } = 3;
        public int PresencesMaxParPersonne { get; set; } = 5;
        
        // Jours critiques spécifiques à l'équipe (stockés en JSON)
        public string? JoursCritiques { get; set; } = "[\"Lundi\",\"Mardi\",\"Vendredi\"]";
        public int PresencesMinJoursCritiques { get; set; } = 2;
        
        // Relations
        public virtual ICollection<Employe> Membres { get; set; } = new List<Employe>();
        public virtual ICollection<AffectationEquipeClient> Affectations { get; set; } = new List<AffectationEquipeClient>();
    }

    // Nouvelle entité Client
    public class Client
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Le nom du client est requis")]
        [StringLength(200)]
        public string Nom { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string? CodeClient { get; set; }
        
        [StringLength(500)]
        public string? Adresse { get; set; }
        
        [StringLength(100)]
        public string? Ville { get; set; }
        
        [StringLength(20)]
        public string? CodePostal { get; set; }
        
        [StringLength(100)]
        public string? Pays { get; set; } = "Maroc";
        
        [StringLength(100)]
        public string? ContactPrincipal { get; set; }
        
        [EmailAddress]
        public string? EmailContact { get; set; }
        
        [Phone]
        public string? TelephoneContact { get; set; }
        
        public bool EstActif { get; set; } = true;
        public DateTime DateCreation { get; set; } = DateTime.Now;
        public DateTime? DateDebutContrat { get; set; }
        public DateTime? DateFinContrat { get; set; }
        
        // Configuration spécifique au client
        public string? HeuresOuverture { get; set; } // JSON: {"lundi": {"debut": "08:00", "fin": "17:00"}, ...}
        public int DistanceKm { get; set; } = 0;
        public int TempsDeplacement { get; set; } = 0; // En minutes
        
        // Relations
        public virtual ICollection<AffectationEquipeClient> Affectations { get; set; } = new List<AffectationEquipeClient>();
    }

    // Affectation Équipe-Client
    public class AffectationEquipeClient
    {
        public int Id { get; set; }
        
        public int EquipeId { get; set; }
        public virtual Equipe Equipe { get; set; } = null!;
        
        public int ClientId { get; set; }
        public virtual Client Client { get; set; } = null!;
        
        public DateTime DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        
        public bool EstActive { get; set; } = true;
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        // Jours de présence chez le client (JSON array)
        public string? JoursPresence { get; set; } = "[\"Lundi\",\"Mardi\",\"Mercredi\",\"Jeudi\",\"Vendredi\"]";
        
        // Configuration spécifique à cette affectation
        public int NombreMinPersonnes { get; set; } = 2;
        public int NombreMaxPersonnes { get; set; } = 4;
        
        // Type de mission
        public TypeMission TypeMission { get; set; } = TypeMission.Regulier;
        public string? FrequenceMission { get; set; } // "Quotidien", "Hebdomadaire", "Mensuel"
        
        // Relations
        public virtual ICollection<PlanningEquipeClient> Plannings { get; set; } = new List<PlanningEquipeClient>();
    }

    // Planning spécifique Équipe-Client
    public class PlanningEquipeClient
    {
        public int Id { get; set; }
        
        public int AffectationId { get; set; }
        public virtual AffectationEquipeClient Affectation { get; set; } = null!;
        
        public DateTime DatePlanning { get; set; }
        public int SemaineNumero { get; set; }
        public int Annee { get; set; }
        
        // Employés assignés pour cette date (JSON array d'IDs)
        public string? EmployesAssignes { get; set; }
        
        public StatusPlanning Status { get; set; } = StatusPlanning.Brouillon;
        public DateTime DateCreation { get; set; } = DateTime.Now;
        public DateTime? DateValidation { get; set; }
        public string? ValidePar { get; set; }
        
        // Métriques
        public int NombrePresencesReelles { get; set; }
        public int NombrePresencesPrevues { get; set; }
        public decimal TauxPresence { get; set; }
    }

    // Compétences
    public class Competence
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public CategorieCompetence Categorie { get; set; }
        public bool EstActive { get; set; } = true;
        
        // Relations
        public virtual ICollection<EmployeCompetence> Employes { get; set; } = new List<EmployeCompetence>();
        public virtual ICollection<ClientCompetenceRequise> ClientsRequis { get; set; } = new List<ClientCompetenceRequise>();
    }

    // Relation Employé-Compétence
    public class EmployeCompetence
    {
        public int EmployeId { get; set; }
        public virtual Employe Employe { get; set; } = null!;
        
        public int CompetenceId { get; set; }
        public virtual Competence Competence { get; set; } = null!;
        
        public int Niveau { get; set; } = 1; // 1-5
        public DateTime DateAcquisition { get; set; } = DateTime.Now;
        public DateTime? DateExpiration { get; set; }
        public bool EstCertifie { get; set; } = false;
        public string? NumeroCertification { get; set; }
    }

    // Compétences requises par client
    public class ClientCompetenceRequise
    {
        public int ClientId { get; set; }
        public virtual Client Client { get; set; } = null!;
        
        public int CompetenceId { get; set; }
        public virtual Competence Competence { get; set; } = null!;
        
        public int NiveauMinimum { get; set; } = 1;
        public bool EstObligatoire { get; set; } = true;
        public string? Notes { get; set; }
    }

    // Historique des affectations
    public class HistoriqueAffectation
    {
        public int Id { get; set; }
        
        public int EmployeId { get; set; }
        public virtual Employe Employe { get; set; } = null!;
        
        public int? EquipeId { get; set; }
        public virtual Equipe? Equipe { get; set; }
        
        public int? ClientId { get; set; }
        public virtual Client? Client { get; set; }
        
        public DateTime DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        
        public TypeAffectation TypeAffectation { get; set; }
        public string? Motif { get; set; }
        public string? Notes { get; set; }
        
        public DateTime DateCreation { get; set; } = DateTime.Now;
        public string? CreePar { get; set; }
    }

    // Configuration globale du planning
    public class ConfigurationPlanning
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Nom { get; set; } = "Configuration par défaut";
        
        public bool EstActive { get; set; } = true;
        
        // Contraintes globales
        public int PresencesMinParPersonne { get; set; } = 3;
        public int PresencesMaxParPersonne { get; set; } = 5;
        public int PresencesMinParJour { get; set; } = 2;
        public int PresencesMaxParJour { get; set; } = 4;
        
        // Jours critiques (JSON array)
        public string JoursCritiques { get; set; } = "[\"Lundi\",\"Mardi\",\"Vendredi\"]";
        public int PresencesMinJoursCritiques { get; set; } = 2;
        public int PresencesMaxJoursCritiques { get; set; } = 3;
        
        // Jours flexibles
        public string JoursFlexibles { get; set; } = "[\"Mercredi\",\"Jeudi\"]";
        public int PresencesMinJoursFlexibles { get; set; } = 0;
        public int PresencesMaxJoursFlexibles { get; set; } = 3;
        
        // Règles spéciales
        public bool RotationEquitable { get; set; } = true;
        public bool RespectCompetences { get; set; } = true;
        public bool OptimiserDeplacements { get; set; } = true;
        public int DelaiNotificationJours { get; set; } = 7;
        
        public DateTime DateCreation { get; set; } = DateTime.Now;
        public DateTime? DateModification { get; set; }
        public string? ModifiePar { get; set; }
    }

    // Énumérations
    public enum TypeConge
    {
        CongeAnnuel,
        CongeMaladie,
        CongeMaternite,
        CongePaternite,
        CongePersonnel,
        Formation,
        RTT,
        CongeExceptionnel,
        Autre
    }

    public enum TypeMission
    {
        Regulier,
        Ponctuel,
        Projet,
        Urgence,
        Formation,
        Audit,
        Support
    }

    public enum StatusPlanning
    {
        Brouillon,
        EnCours,
        Valide,
        Modifie,
        Annule
    }

    public enum CategorieCompetence
    {
        Technique,
        Fonctionnelle,
        Linguistique,
        Certification,
        Permis,
        Autre
    }

    public enum TypeAffectation
    {
        NouvelleEquipe,
        ChangementEquipe,
        AffectationClient,
        FinMission,
        Conge,
        Formation,
        Autre
    }

    // Classe existante modifiée
    public class Conge
    {
        public int Id { get; set; }
        
        [Required]
        public string Nom { get; set; } = string.Empty;
        
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public string? Raison { get; set; }
        public TypeConge Type { get; set; } = TypeConge.CongeAnnuel;
        
        // Nouvelles propriétés
        public int? EmployeId { get; set; }
        public virtual Employe? Employe { get; set; }
        
        public StatusConge Status { get; set; } = StatusConge.EnAttente;
        public DateTime DateDemande { get; set; } = DateTime.Now;
        public DateTime? DateValidation { get; set; }
        public string? ValidePar { get; set; }
        public string? CommentaireValidation { get; set; }
        
        public bool EstActif => DateDebut <= DateTime.Today && DateFin >= DateTime.Today;
        public int NombreJours => (DateFin - DateDebut).Days + 1;
    }

    public enum StatusConge
    {
        EnAttente,
        Approuve,
        Refuse,
        Annule
    }
}