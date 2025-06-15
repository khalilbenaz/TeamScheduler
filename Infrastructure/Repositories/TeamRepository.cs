using Microsoft.EntityFrameworkCore;
using TeamScheduler.Core.Entities;
using TeamScheduler.Core.Interfaces;
using PlanningPresenceBlazor.Data;

namespace TeamScheduler.Infrastructure.Repositories
{
    public class TeamRepository : ITeamRepository
    {
        private readonly PlanningDbContext _context;

        public TeamRepository(PlanningDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Team>> GetAllAsync(bool includeMembers = false)
        {
            var query = _context.Equipes.Where(e => e.EstActive);

            if (includeMembers)
            {
                query = query.Include(e => e.Membres)
                             .Include(e => e.ChefEquipe)
                             .Include(e => e.Affectations)
                             .Include(e => e.AffectationsProjets);
            }

            var equipes = await query.OrderBy(e => e.Nom).ToListAsync();
            
            // Map old entities to new entities
            return equipes.Select(MapToNewTeam);
        }

        public async Task<Team?> GetByIdAsync(int id, bool includeDetails = false)
        {
            var query = _context.Equipes.Where(e => e.Id == id);

            if (includeDetails)
            {
                query = query.Include(e => e.Membres)
                             .Include(e => e.ChefEquipe)
                             .Include(e => e.Affectations)
                             .ThenInclude(a => a.Client)
                             .Include(e => e.AffectationsProjets)
                             .ThenInclude(ap => ap.Projet);
            }

            var equipe = await query.FirstOrDefaultAsync();
            return equipe != null ? MapToNewTeam(equipe) : null;
        }

        public async Task<Team> AddAsync(Team team)
        {
            var equipe = MapToOldEquipe(team);
            _context.Equipes.Add(equipe);
            await _context.SaveChangesAsync();
            
            team.Id = equipe.Id;
            return team;
        }

        public async Task UpdateAsync(Team team)
        {
            var existingEquipe = await _context.Equipes.FindAsync(team.Id);
            if (existingEquipe != null)
            {
                UpdateEquipeFromTeam(existingEquipe, team);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var equipe = await _context.Equipes.FindAsync(id);
            if (equipe != null)
            {
                // Clean up members
                var membres = await _context.Employes.Where(e => e.EquipeId == id).ToListAsync();
                foreach (var membre in membres)
                {
                    membre.EquipeId = null;
                }

                // Clean up client assignments
                var clientAssignments = await _context.AffectationsEquipeClient.Where(a => a.EquipeId == id).ToListAsync();
                _context.AffectationsEquipeClient.RemoveRange(clientAssignments);

                // Clean up project assignments
                var projectAssignments = await _context.AffectationsEquipeProjet.Where(a => a.EquipeId == id).ToListAsync();
                _context.AffectationsEquipeProjet.RemoveRange(projectAssignments);

                _context.Equipes.Remove(equipe);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Equipes.AnyAsync(e => e.Id == id);
        }

        public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
        {
            var query = _context.Equipes.Where(e => e.CodeEquipe == code);
            if (excludeId.HasValue)
            {
                query = query.Where(e => e.Id != excludeId.Value);
            }
            return await query.AnyAsync();
        }

        private static Team MapToNewTeam(PlanningPresenceBlazor.Data.Equipe equipe)
        {
            return new Team
            {
                Id = equipe.Id,
                Name = equipe.Nom,
                Description = equipe.Description,
                Code = equipe.CodeEquipe,
                IsActive = equipe.EstActive,
                CreatedAt = equipe.DateCreation,
                TeamLeaderId = equipe.ChefEquipeId,
                MinDailyPresences = equipe.PresencesMinParJour,
                MaxDailyPresences = equipe.PresencesMaxParJour,
                MinPersonPresences = equipe.PresencesMinParPersonne,
                MaxPersonPresences = equipe.PresencesMaxParPersonne,
                CriticalDays = equipe.JoursCritiques,
                MinCriticalDayPresences = equipe.PresencesMinJoursCritiques,
                TeamLeader = equipe.ChefEquipe != null ? MapToNewEmployee(equipe.ChefEquipe) : null,
                Members = equipe.Membres.Select(MapToNewEmployee).ToList()
            };
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
                IsActive = employe.EstActif,
                TeamId = employe.EquipeId,
                Position = employe.Poste,
                SkillLevel = employe.NiveauCompetence
            };
        }

        private static PlanningPresenceBlazor.Data.Equipe MapToOldEquipe(Team team)
        {
            return new PlanningPresenceBlazor.Data.Equipe
            {
                Nom = team.Name,
                Description = team.Description,
                CodeEquipe = team.Code,
                EstActive = team.IsActive,
                DateCreation = team.CreatedAt,
                ChefEquipeId = team.TeamLeaderId,
                PresencesMinParJour = team.MinDailyPresences,
                PresencesMaxParJour = team.MaxDailyPresences,
                PresencesMinParPersonne = team.MinPersonPresences,
                PresencesMaxParPersonne = team.MaxPersonPresences,
                JoursCritiques = team.CriticalDays,
                PresencesMinJoursCritiques = team.MinCriticalDayPresences
            };
        }

        private static void UpdateEquipeFromTeam(PlanningPresenceBlazor.Data.Equipe equipe, Team team)
        {
            equipe.Nom = team.Name;
            equipe.Description = team.Description;
            equipe.CodeEquipe = team.Code;
            equipe.EstActive = team.IsActive;
            equipe.ChefEquipeId = team.TeamLeaderId;
            equipe.PresencesMinParJour = team.MinDailyPresences;
            equipe.PresencesMaxParJour = team.MaxDailyPresences;
            equipe.PresencesMinParPersonne = team.MinPersonPresences;
            equipe.PresencesMaxParPersonne = team.MaxPersonPresences;
            equipe.JoursCritiques = team.CriticalDays;
            equipe.PresencesMinJoursCritiques = team.MinCriticalDayPresences;
        }
    }
}
