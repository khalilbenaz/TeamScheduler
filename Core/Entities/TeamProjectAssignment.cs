namespace TeamScheduler.Core.Entities
{
    public class TeamProjectAssignment
    {
        public int Id { get; set; }
        
        public int TeamId { get; set; }
        public virtual Team Team { get; set; } = null!;
        
        public int ProjectId { get; set; }
        public virtual Project Project { get; set; } = null!;
        
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public string? Notes { get; set; }
    }
}
