using System.ComponentModel.DataAnnotations;

namespace TeamScheduler.Core.Entities
{
    public class Project
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Le nom du projet est requis")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        public int ClientId { get; set; }
        public virtual Client Client { get; set; } = null!;
        
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Relations with assigned teams
        public virtual ICollection<TeamProjectAssignment> TeamAssignments { get; set; } = new List<TeamProjectAssignment>();
    }
}
