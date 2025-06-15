using System.ComponentModel.DataAnnotations;

namespace TeamScheduler.Core.Entities
{
    public class Team
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Le nom de l'équipe est requis")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public string? Code { get; set; } // Code unique
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Team leader
        public int? TeamLeaderId { get; set; }
        public virtual Employee? TeamLeader { get; set; }
        
        // Team-specific configuration
        public int MinDailyPresences { get; set; } = 2;
        public int MaxDailyPresences { get; set; } = 4;
        public int MinPersonPresences { get; set; } = 3;
        public int MaxPersonPresences { get; set; } = 5;
        
        // Critical days (stored as JSON)
        public string? CriticalDays { get; set; } = "[\"Lundi\",\"Mardi\",\"Vendredi\"]";
        public int MinCriticalDayPresences { get; set; } = 2;
        
        // Relations
        public virtual ICollection<Employee> Members { get; set; } = new List<Employee>();
        public virtual ICollection<TeamClientAssignment> ClientAssignments { get; set; } = new List<TeamClientAssignment>();
        public virtual ICollection<TeamProjectAssignment> ProjectAssignments { get; set; } = new List<TeamProjectAssignment>();
    }
}
