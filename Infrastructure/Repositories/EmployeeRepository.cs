using Microsoft.EntityFrameworkCore;
using TeamScheduler.Core.Entities;
using TeamScheduler.Core.Interfaces;
using PlanningPresenceBlazor.Data;

namespace TeamScheduler.Infrastructure.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly PlanningDbContext _context;

        public EmployeeRepository(PlanningDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            var employes = await _context.Employes
                .Include(e => e.Equipe)
                .ToListAsync();
            
            return employes.Select(MapToNewEmployee);
        }

        public async Task<Employee?> GetByIdAsync(int id)
        {
            var employe = await _context.Employes
                .Include(e => e.Equipe)
                .FirstOrDefaultAsync(e => e.Id == id);
            
            return employe != null ? MapToNewEmployee(employe) : null;
        }

        public async Task<IEnumerable<Employee>> GetAvailableAsync()
        {
            var employes = await _context.Employes
                .Where(e => e.EstActif && e.EquipeId == null)
                .ToListAsync();
            
            return employes.Select(MapToNewEmployee);
        }

        public async Task<IEnumerable<Employee>> GetByTeamIdAsync(int teamId)
        {
            var employes = await _context.Employes
                .Where(e => e.EquipeId == teamId)
                .ToListAsync();
            
            return employes.Select(MapToNewEmployee);
        }

        public async Task<Employee> AddAsync(Employee employee)
        {
            var employe = MapToOldEmploye(employee);
            _context.Employes.Add(employe);
            await _context.SaveChangesAsync();
            
            employee.Id = employe.Id;
            return employee;
        }

        public async Task UpdateAsync(Employee employee)
        {
            var existingEmploye = await _context.Employes.FindAsync(employee.Id);
            if (existingEmploye != null)
            {
                UpdateEmployeFromEmployee(existingEmploye, employee);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var employe = await _context.Employes.FindAsync(id);
            if (employe != null)
            {
                _context.Employes.Remove(employe);
                await _context.SaveChangesAsync();
            }
        }

        private static Employee MapToNewEmployee(PlanningPresenceBlazor.Data.Employe employe)
        {
            return new Employee
            {
                Id = employe.Id,
                FirstName = employe.Prenom,
                LastName = employe.Nom,
                Email = employe.Email,
                Phone = employe.Telephone,
                TeamsId = employe.TeamsId,
                IsActive = employe.EstActif,
                HireDate = employe.DateEmbauche,
                CreatedAt = employe.DateCreation,
                EmailNotification = employe.NotificationEmail,
                SmsNotification = employe.NotificationSMS,
                TeamsNotification = employe.NotificationTeams,
                TeamId = employe.EquipeId,
                Position = employe.Poste,
                SkillLevel = employe.NiveauCompetence,
                MinWeeklyPresences = employe.PresencesMinSemaine,
                MaxWeeklyPresences = employe.PresencesMaxSemaine
            };
        }

        private static PlanningPresenceBlazor.Data.Employe MapToOldEmploye(Employee employee)
        {
            return new PlanningPresenceBlazor.Data.Employe
            {
                Prenom = employee.FirstName,
                Nom = employee.LastName,
                Email = employee.Email,
                Telephone = employee.Phone,
                TeamsId = employee.TeamsId,
                EstActif = employee.IsActive,
                DateEmbauche = employee.HireDate,
                DateCreation = employee.CreatedAt,
                NotificationEmail = employee.EmailNotification,
                NotificationSMS = employee.SmsNotification,
                NotificationTeams = employee.TeamsNotification,
                EquipeId = employee.TeamId,
                Poste = employee.Position,
                NiveauCompetence = employee.SkillLevel,
                PresencesMinSemaine = employee.MinWeeklyPresences,
                PresencesMaxSemaine = employee.MaxWeeklyPresences
            };
        }

        private static void UpdateEmployeFromEmployee(PlanningPresenceBlazor.Data.Employe employe, Employee employee)
        {
            employe.Prenom = employee.FirstName;
            employe.Nom = employee.LastName;
            employe.Email = employee.Email;
            employe.Telephone = employee.Phone;
            employe.TeamsId = employee.TeamsId;
            employe.EstActif = employee.IsActive;
            employe.DateEmbauche = employee.HireDate;
            employe.NotificationEmail = employee.EmailNotification;
            employe.NotificationSMS = employee.SmsNotification;
            employe.NotificationTeams = employee.TeamsNotification;
            employe.EquipeId = employee.TeamId;
            employe.Poste = employee.Position;
            employe.NiveauCompetence = employee.SkillLevel;
            employe.PresencesMinSemaine = employee.MinWeeklyPresences;
            employe.PresencesMaxSemaine = employee.MaxWeeklyPresences;
        }
    }
}
