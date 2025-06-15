using TeamScheduler.Core.Enums;

namespace TeamScheduler.Core.Entities
{
    public class Skill
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public SkillCategory Category { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Relations
        public virtual ICollection<EmployeeSkill> Employees { get; set; } = new List<EmployeeSkill>();
        public virtual ICollection<ClientRequiredSkill> RequiredByClients { get; set; } = new List<ClientRequiredSkill>();
    }

    public class EmployeeSkill
    {
        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; } = null!;
        
        public int SkillId { get; set; }
        public virtual Skill Skill { get; set; } = null!;
        
        public int Level { get; set; } = 1; // 1-5
        public DateTime AcquisitionDate { get; set; } = DateTime.Now;
        public DateTime? ExpirationDate { get; set; }
        public bool IsCertified { get; set; } = false;
        public string? CertificationNumber { get; set; }
    }

    public class ClientRequiredSkill
    {
        public int ClientId { get; set; }
        public virtual Client Client { get; set; } = null!;
        
        public int SkillId { get; set; }
        public virtual Skill Skill { get; set; } = null!;
        
        public int MinimumLevel { get; set; } = 1;
        public bool IsRequired { get; set; } = true;
        public string? Notes { get; set; }
    }

    public class AssignmentHistory
    {
        public int Id { get; set; }
        
        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; } = null!;
        
        public int? TeamId { get; set; }
        public virtual Team? Team { get; set; }
        
        public int? ClientId { get; set; }
        public virtual Client? Client { get; set; }
        
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        public AssignmentType AssignmentType { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
    }

    public class TeamClientPlanning
    {
        public int Id { get; set; }
        
        public int AssignmentId { get; set; }
        public virtual TeamClientAssignment Assignment { get; set; } = null!;
        
        public DateTime PlanningDate { get; set; }
        public int WeekNumber { get; set; }
        public int Year { get; set; }
        
        // Assigned employees for this date (JSON array of IDs)
        public string? AssignedEmployees { get; set; }
        
        public PlanningStatus Status { get; set; } = PlanningStatus.Draft;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ValidatedAt { get; set; }
        public string? ValidatedBy { get; set; }
        
        // Metrics
        public int ActualPresences { get; set; }
        public int PlannedPresences { get; set; }
        public decimal PresenceRate { get; set; }
    }
}
