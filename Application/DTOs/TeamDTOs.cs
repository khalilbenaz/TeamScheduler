namespace TeamScheduler.Application.DTOs
{
    public class TeamDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Code { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public int? TeamLeaderId { get; set; }
        public string? TeamLeaderName { get; set; }
        
        public int MinDailyPresences { get; set; }
        public int MaxDailyPresences { get; set; }
        public int MinPersonPresences { get; set; }
        public int MaxPersonPresences { get; set; }
        public int MinCriticalDayPresences { get; set; }
        
        public List<EmployeeDto> Members { get; set; } = new();
        public List<ClientAssignmentDto> ClientAssignments { get; set; } = new();
        public List<ProjectAssignmentDto> ProjectAssignments { get; set; } = new();
        
        // Statistics
        public int TotalMembers { get; set; }
        public int ActiveMembers { get; set; }
        public int ActiveClientAssignments { get; set; }
        public int ActiveProjectAssignments { get; set; }
    }

    public class CreateTeamDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Code { get; set; }
        public bool IsActive { get; set; } = true;
        public int? TeamLeaderId { get; set; }
        public int MinDailyPresences { get; set; } = 2;
        public int MaxDailyPresences { get; set; } = 4;
        public int MinPersonPresences { get; set; } = 3;
        public int MaxPersonPresences { get; set; } = 5;
        public int MinCriticalDayPresences { get; set; } = 2;
    }

    public class UpdateTeamDto : CreateTeamDto
    {
        public int Id { get; set; }
    }

    public class EmployeeDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Position { get; set; }
        public bool IsActive { get; set; }
        public int? TeamId { get; set; }
        public string? TeamName { get; set; }
        public int SkillLevel { get; set; }

        // --- RH avancé ---
        public DateTime? DateEntree { get; set; } // Correction : nullable pour correspondre à l'entité
        public DateTime? DateSortie { get; set; }
        public string? TypeContrat { get; set; } // CDI, CDD, Freelance, etc.
        public List<string> DocumentsRH { get; set; } = new();

        // --- Hiérarchie ---
        public int? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public List<int> SubordonnesIds { get; set; } = new();
        public List<string> SubordonnesNoms { get; set; } = new();

        // --- Temps de travail ---
        public List<PresenceRecordDto> HistoriquePresences { get; set; } = new();
        public int JoursTeletravail { get; set; }
        public int JoursSite { get; set; }
        public int JoursClient { get; set; }
        public string? PlageHorairePreferee { get; set; }
    }

    public class PresenceRecordDto
    {
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty; // Présence, Télétravail, Site, Client, etc.
        public string? Commentaire { get; set; }
    }

    public class ClientAssignmentDto
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
        public int MinPersons { get; set; }
        public int MaxPersons { get; set; }
    }

    public class ProjectAssignmentDto
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
    }
    
    public class TeamStatisticsDto
    {
        public int TotalMembers { get; set; }
        public int ActiveMembers { get; set; }
        public int ActiveClients { get; set; }
        public int ActiveProjects { get; set; }
        public double AveragePresenceRate { get; set; }
        public DateTime LastActivity { get; set; }
    }
}
