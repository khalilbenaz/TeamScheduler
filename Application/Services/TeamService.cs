using TeamScheduler.Application.DTOs;
using TeamScheduler.Core.Entities;
using TeamScheduler.Core.Interfaces;

namespace TeamScheduler.Application.Services
{
    public interface ITeamService
    {
        Task<IEnumerable<TeamDto>> GetAllTeamsAsync(bool includeMembers = false);
        Task<TeamDto?> GetTeamByIdAsync(int id, bool includeDetails = false);
        Task<TeamDto> CreateTeamAsync(CreateTeamDto createTeamDto);
        Task<TeamDto> UpdateTeamAsync(UpdateTeamDto updateTeamDto);
        Task DeleteTeamAsync(int id);
        Task<bool> IsTeamCodeUniqueAsync(string code, int? excludeId = null);
        Task AddMemberToTeamAsync(int teamId, int employeeId);
        Task RemoveMemberFromTeamAsync(int teamId, int employeeId);
        Task<IEnumerable<EmployeeDto>> GetAvailableEmployeesAsync();
        Task<TeamStatisticsDto> GetTeamStatisticsAsync(int teamId);
    }

    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _teamRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ITeamClientAssignmentRepository _clientAssignmentRepository;
        private readonly ITeamProjectAssignmentRepository _projectAssignmentRepository;

        public TeamService(
            ITeamRepository teamRepository,
            IEmployeeRepository employeeRepository,
            ITeamClientAssignmentRepository clientAssignmentRepository,
            ITeamProjectAssignmentRepository projectAssignmentRepository)
        {
            _teamRepository = teamRepository;
            _employeeRepository = employeeRepository;
            _clientAssignmentRepository = clientAssignmentRepository;
            _projectAssignmentRepository = projectAssignmentRepository;
        }

        public async Task<IEnumerable<TeamDto>> GetAllTeamsAsync(bool includeMembers = false)
        {
            var teams = await _teamRepository.GetAllAsync(includeMembers);
            return teams.Select(MapToDto);
        }

        public async Task<TeamDto?> GetTeamByIdAsync(int id, bool includeDetails = false)
        {
            var team = await _teamRepository.GetByIdAsync(id, includeDetails);
            return team != null ? MapToDto(team) : null;
        }

        public async Task<TeamDto> CreateTeamAsync(CreateTeamDto createTeamDto)
        {
            var team = new Team
            {
                Name = createTeamDto.Name,
                Description = createTeamDto.Description,
                Code = createTeamDto.Code,
                IsActive = createTeamDto.IsActive,
                TeamLeaderId = createTeamDto.TeamLeaderId,
                MinDailyPresences = createTeamDto.MinDailyPresences,
                MaxDailyPresences = createTeamDto.MaxDailyPresences,
                MinPersonPresences = createTeamDto.MinPersonPresences,
                MaxPersonPresences = createTeamDto.MaxPersonPresences,
                MinCriticalDayPresences = createTeamDto.MinCriticalDayPresences
            };

            var createdTeam = await _teamRepository.AddAsync(team);
            return MapToDto(createdTeam);
        }

        public async Task<TeamDto> UpdateTeamAsync(UpdateTeamDto updateTeamDto)
        {
            var team = await _teamRepository.GetByIdAsync(updateTeamDto.Id);
            if (team == null)
                throw new ArgumentException($"Team with ID {updateTeamDto.Id} not found");

            team.Name = updateTeamDto.Name;
            team.Description = updateTeamDto.Description;
            team.Code = updateTeamDto.Code;
            team.IsActive = updateTeamDto.IsActive;
            team.TeamLeaderId = updateTeamDto.TeamLeaderId;
            team.MinDailyPresences = updateTeamDto.MinDailyPresences;
            team.MaxDailyPresences = updateTeamDto.MaxDailyPresences;
            team.MinPersonPresences = updateTeamDto.MinPersonPresences;
            team.MaxPersonPresences = updateTeamDto.MaxPersonPresences;
            team.MinCriticalDayPresences = updateTeamDto.MinCriticalDayPresences;

            await _teamRepository.UpdateAsync(team);
            return MapToDto(team);
        }

        public async Task DeleteTeamAsync(int id)
        {
            await _teamRepository.DeleteAsync(id);
        }

        public async Task<bool> IsTeamCodeUniqueAsync(string code, int? excludeId = null)
        {
            return !await _teamRepository.CodeExistsAsync(code, excludeId);
        }

        public async Task AddMemberToTeamAsync(int teamId, int employeeId)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee == null)
                throw new ArgumentException($"Employee with ID {employeeId} not found");

            employee.TeamId = teamId;
            await _employeeRepository.UpdateAsync(employee);
        }

        public async Task RemoveMemberFromTeamAsync(int teamId, int employeeId)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee == null)
                throw new ArgumentException($"Employee with ID {employeeId} not found");

            employee.TeamId = null;
            await _employeeRepository.UpdateAsync(employee);
        }

        public async Task<IEnumerable<EmployeeDto>> GetAvailableEmployeesAsync()
        {
            var employees = await _employeeRepository.GetAvailableAsync();
            return employees.Select(e => new EmployeeDto
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                FullName = e.FullName,
                Email = e.Email,
                Phone = e.Phone,
                Position = e.Position,
                IsActive = e.IsActive,
                TeamId = e.TeamId,
                SkillLevel = e.SkillLevel,
                // --- RH avancé ---
                DateEntree = e.DateEntree,
                DateSortie = e.DateSortie,
                TypeContrat = e.TypeContrat,
                DocumentsRH = e.DocumentsRH?.ToList() ?? new List<string>(),
                // --- Hiérarchie ---
                ManagerId = e.ManagerId,
                ManagerName = e.Manager?.FullName,
                SubordonnesIds = e.Subordonnes?.Select(s => s.Id).ToList() ?? new List<int>(),
                SubordonnesNoms = e.Subordonnes?.Select(s => s.FullName).ToList() ?? new List<string>(),
                // --- Temps de travail ---
                HistoriquePresences = e.Presences?.Select(p => new PresenceRecordDto {
                    Date = p.Date,
                    Type = p.Type,
                    Commentaire = p.Commentaire
                }).ToList() ?? new List<PresenceRecordDto>(),
                JoursTeletravail = e.JoursTeletravail,
                JoursSite = e.JoursSite,
                JoursClient = e.JoursClient,
                PlageHorairePreferee = e.PlageHorairePreferee
            });
        }

        public async Task<TeamStatisticsDto> GetTeamStatisticsAsync(int teamId)
        {
            var team = await _teamRepository.GetByIdAsync(teamId, includeDetails: true);
            if (team == null)
                throw new ArgumentException($"Team with ID {teamId} not found");

            var clientAssignments = await _clientAssignmentRepository.GetByTeamIdAsync(teamId);
            var projectAssignments = await _projectAssignmentRepository.GetByTeamIdAsync(teamId);

            return new TeamStatisticsDto
            {
                TotalMembers = team.Members.Count,
                ActiveMembers = team.Members.Count(m => m.IsActive),
                ActiveClients = clientAssignments.Count(ca => ca.IsActive),
                ActiveProjects = projectAssignments.Count(pa => pa.IsActive),
                AveragePresenceRate = CalculateAveragePresenceRate(team),
                LastActivity = DateTime.Now // TODO: Calculer la vraie dernière activité
            };
        }

        private double CalculateAveragePresenceRate(Team team)
        {
            // Logique de calcul du taux de présence moyen
            // Pour l'instant, retournons une valeur par défaut
            return team.Members.Count > 0 ? 85.5 : 0.0;
        }

        private static TeamDto MapToDto(Team team)
        {
            return new TeamDto
            {
                Id = team.Id,
                Name = team.Name,
                Description = team.Description,
                Code = team.Code,
                IsActive = team.IsActive,
                CreatedAt = team.CreatedAt,
                TeamLeaderId = team.TeamLeaderId,
                TeamLeaderName = team.TeamLeader?.FullName,
                MinDailyPresences = team.MinDailyPresences,
                MaxDailyPresences = team.MaxDailyPresences,
                MinPersonPresences = team.MinPersonPresences,
                MaxPersonPresences = team.MaxPersonPresences,
                MinCriticalDayPresences = team.MinCriticalDayPresences,
                TotalMembers = team.Members.Count,
                ActiveMembers = team.Members.Count(m => m.IsActive),
                ActiveClientAssignments = team.ClientAssignments.Count(a => a.IsActive),
                ActiveProjectAssignments = team.ProjectAssignments.Count(a => a.IsActive),
                Members = team.Members.Select(m => new EmployeeDto
                {
                    Id = m.Id,
                    FirstName = m.FirstName,
                    LastName = m.LastName,
                    FullName = m.FullName,
                    Email = m.Email,
                    Phone = m.Phone,
                    Position = m.Position,
                    IsActive = m.IsActive,
                    TeamId = m.TeamId,
                    SkillLevel = m.SkillLevel,
                    // --- RH avancé ---
                    DateEntree = m.DateEntree,
                    DateSortie = m.DateSortie,
                    TypeContrat = m.TypeContrat,
                    DocumentsRH = m.DocumentsRH?.ToList() ?? new List<string>(),
                    // --- Hiérarchie ---
                    ManagerId = m.ManagerId,
                    ManagerName = m.Manager?.FullName,
                    SubordonnesIds = m.Subordonnes?.Select(s => s.Id).ToList() ?? new List<int>(),
                    SubordonnesNoms = m.Subordonnes?.Select(s => s.FullName).ToList() ?? new List<string>(),
                    // --- Temps de travail ---
                    HistoriquePresences = m.Presences?.Select(p => new PresenceRecordDto {
                        Date = p.Date,
                        Type = p.Type,
                        Commentaire = p.Commentaire
                    }).ToList() ?? new List<PresenceRecordDto>(),
                    JoursTeletravail = m.JoursTeletravail,
                    JoursSite = m.JoursSite,
                    JoursClient = m.JoursClient,
                    PlageHorairePreferee = m.PlageHorairePreferee
                }).ToList()
            };
        }
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
