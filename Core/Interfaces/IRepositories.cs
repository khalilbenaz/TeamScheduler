using TeamScheduler.Core.Entities;

namespace TeamScheduler.Core.Interfaces
{
    public interface ITeamRepository
    {
        Task<IEnumerable<Team>> GetAllAsync(bool includeMembers = false);
        Task<Team?> GetByIdAsync(int id, bool includeDetails = false);
        Task<Team> AddAsync(Team team);
        Task UpdateAsync(Team team);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> CodeExistsAsync(string code, int? excludeId = null);
    }

    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetAllAsync();
        Task<Employee?> GetByIdAsync(int id);
        Task<IEnumerable<Employee>> GetAvailableAsync();
        Task<IEnumerable<Employee>> GetByTeamIdAsync(int teamId);
        Task<Employee> AddAsync(Employee employee);
        Task UpdateAsync(Employee employee);
        Task DeleteAsync(int id);
    }

    public interface IClientRepository
    {
        Task<IEnumerable<Client>> GetAllAsync(bool includeAssignments = false);
        Task<Client?> GetByIdAsync(int id, bool includeDetails = false);
        Task<Client> AddAsync(Client client);
        Task UpdateAsync(Client client);
        Task DeleteAsync(int id);
    }

    public interface IProjectRepository
    {
        Task<IEnumerable<Project>> GetAllAsync();
        Task<Project?> GetByIdAsync(int id, bool includeDetails = false);
        Task<IEnumerable<Project>> GetByClientIdAsync(int clientId);
        Task<Project> AddAsync(Project project);
        Task UpdateAsync(Project project);
        Task DeleteAsync(int id);
    }

    public interface ITeamClientAssignmentRepository
    {
        Task<IEnumerable<TeamClientAssignment>> GetByTeamIdAsync(int teamId);
        Task<IEnumerable<TeamClientAssignment>> GetByClientIdAsync(int clientId);
        Task<TeamClientAssignment> AddAsync(TeamClientAssignment assignment);
        Task UpdateAsync(TeamClientAssignment assignment);
        Task DeleteAsync(int id);
    }

    public interface ITeamProjectAssignmentRepository
    {
        Task<IEnumerable<TeamProjectAssignment>> GetByTeamIdAsync(int teamId);
        Task<IEnumerable<TeamProjectAssignment>> GetByProjectIdAsync(int projectId);
        Task<TeamProjectAssignment> AddAsync(TeamProjectAssignment assignment);
        Task UpdateAsync(TeamProjectAssignment assignment);
        Task DeleteAsync(int id);
    }
}
