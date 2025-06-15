using System.ComponentModel.DataAnnotations;

namespace TeamScheduler.Core.Entities
{
    public class Employee
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Le prénom est requis")]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Le nom est requis")]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? TeamsId { get; set; }
        
        public bool IsActive { get; set; } = true;
        public DateTime HireDate { get; set; } = DateTime.Today;
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        
        public bool EmailNotification { get; set; } = true;
        public bool SmsNotification { get; set; } = false;
        public bool TeamsNotification { get; set; } = false;
        
        // Team relation
        public int? TeamId { get; set; }
        public virtual Team? Team { get; set; }
        
        public string? Position { get; set; }
        public int SkillLevel { get; set; } = 1; // 1-5
        public int MinWeeklyPresences { get; set; } = 3;
        public int MaxWeeklyPresences { get; set; } = 5;
        
        // Relations
        public virtual ICollection<EmployeeSkill> Skills { get; set; } = new List<EmployeeSkill>();
        public virtual ICollection<AssignmentHistory> AssignmentHistories { get; set; } = new List<AssignmentHistory>();
        
        // Subordonnes
        public virtual ICollection<Employee> Subordonnes { get; set; } = new List<Employee>();
        
        // --- RH avancé ---
        public DateTime? DateEntree { get; set; } // Alias de HireDate pour DTO
        public DateTime? DateSortie { get; set; }
        public string? TypeContrat { get; set; }
        public ICollection<string> DocumentsRH { get; set; } = new List<string>();

        // --- Hiérarchie ---
        public int? ManagerId { get; set; }
        public virtual Employee? Manager { get; set; }
        // Subordonnes déjà présent

        // --- Temps de travail ---
        public ICollection<PresenceRecord> Presences { get; set; } = new List<PresenceRecord>();
        public int JoursTeletravail { get; set; }
        public int JoursSite { get; set; }
        public int JoursClient { get; set; }
        public string? PlageHorairePreferee { get; set; }
        
        // Computed properties
        public string FullName => $"{FirstName} {LastName}";
        
        [Required]
        public string Role { get; set; } = "Utilisateur"; // Valeurs possibles : Administrateur, Manager, Utilisateur
    }
}
