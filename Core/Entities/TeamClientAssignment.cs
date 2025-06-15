using TeamScheduler.Core.Enums;

namespace TeamScheduler.Core.Entities
{
    public class TeamClientAssignment
    {
        public int Id { get; set; }
        
        public int TeamId { get; set; }
        public virtual Team Team { get; set; } = null!;
        
        public int ClientId { get; set; }
        public virtual Client Client { get; set; } = null!;
        
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public string? Notes { get; set; }
        
        // Days of presence at client (JSON array)
        public string? PresenceDays { get; set; } = "[\"Lundi\",\"Mardi\",\"Mercredi\",\"Jeudi\",\"Vendredi\"]";
        
        // Assignment-specific configuration
        public int MinPersons { get; set; } = 2;
        public int MaxPersons { get; set; } = 4;
        
        // Mission type
        public MissionType MissionType { get; set; } = MissionType.Regular;
        public string? MissionFrequency { get; set; } // "Daily", "Weekly", "Monthly"
        
        // Relations
        public virtual ICollection<TeamClientPlanning> Plannings { get; set; } = new List<TeamClientPlanning>();
    }
}
