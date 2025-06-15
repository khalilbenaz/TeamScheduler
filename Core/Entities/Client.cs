using System.ComponentModel.DataAnnotations;

namespace TeamScheduler.Core.Entities
{
    public class Client
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Le nom du client est requis")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string? Code { get; set; }
        
        [StringLength(500)]
        public string? Address { get; set; }
        
        [StringLength(100)]
        public string? City { get; set; }
        
        [StringLength(20)]
        public string? PostalCode { get; set; }
        
        [StringLength(100)]
        public string? Country { get; set; } = "Maroc";
        
        [StringLength(100)]
        public string? PrimaryContact { get; set; }
        
        [EmailAddress]
        public string? ContactEmail { get; set; }
        
        [Phone]
        public string? ContactPhone { get; set; }
        
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        
        // Client-specific configuration
        public string? OpeningHours { get; set; } // JSON: {"lundi": {"debut": "08:00", "fin": "17:00"}, ...}
        public int DistanceKm { get; set; } = 0;
        public int TravelTimeMinutes { get; set; } = 0;
        
        // Relations
        public virtual ICollection<TeamClientAssignment> TeamAssignments { get; set; } = new List<TeamClientAssignment>();
        public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

        [StringLength(500)]
        public string? Description { get; set; }
    }
}
