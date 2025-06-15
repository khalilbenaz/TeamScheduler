using TeamScheduler.Core.Entities;
using TeamScheduler.Core.Interfaces;
using PlanningPresenceBlazor.Data;
using Microsoft.EntityFrameworkCore;

namespace TeamScheduler.Infrastructure.Repositories
{
    public class TeamProjectAssignmentRepository : ITeamProjectAssignmentRepository
    {
        private readonly PlanningDbContext _context;

        public TeamProjectAssignmentRepository(PlanningDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TeamProjectAssignment>> GetByTeamIdAsync(int teamId)
        {
            var oldAssignments = await _context.AffectationsEquipeProjet
                .Where(a => a.EquipeId == teamId)
                .ToListAsync();

            return oldAssignments.Select(MapToNew);
        }

        public async Task<IEnumerable<TeamProjectAssignment>> GetByProjectIdAsync(int projectId)
        {
            var oldAssignments = await _context.AffectationsEquipeProjet
                .Where(a => a.ProjetId == projectId)
                .ToListAsync();

            return oldAssignments.Select(MapToNew);
        }

        public async Task<TeamProjectAssignment> AddAsync(TeamProjectAssignment assignment)
        {
            var oldAssignment = MapToOld(assignment);
            _context.AffectationsEquipeProjet.Add(oldAssignment);
            await _context.SaveChangesAsync();
            
            assignment.Id = oldAssignment.Id;
            return assignment;
        }

        public async Task UpdateAsync(TeamProjectAssignment assignment)
        {
            var existing = await _context.AffectationsEquipeProjet.FindAsync(assignment.Id);
            if (existing != null)
            {
                existing.EquipeId = assignment.TeamId;
                existing.ProjetId = assignment.ProjectId;
                existing.DateDebut = assignment.StartDate;
                existing.DateFin = assignment.EndDate;
                existing.EstActive = assignment.IsActive;
                existing.Notes = assignment.Notes;
                
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var assignment = await _context.AffectationsEquipeProjet.FindAsync(id);
            if (assignment != null)
            {
                _context.AffectationsEquipeProjet.Remove(assignment);
                await _context.SaveChangesAsync();
            }
        }

        private static TeamProjectAssignment MapToNew(AffectationEquipeProjet old)
        {
            return new TeamProjectAssignment
            {
                Id = old.Id,
                TeamId = old.EquipeId,
                ProjectId = old.ProjetId,
                StartDate = old.DateDebut,
                EndDate = old.DateFin,
                IsActive = old.EstActive,
                Notes = old.Notes
            };
        }

        private static AffectationEquipeProjet MapToOld(TeamProjectAssignment assignment)
        {
            return new AffectationEquipeProjet
            {
                Id = assignment.Id,
                EquipeId = assignment.TeamId,
                ProjetId = assignment.ProjectId,
                DateDebut = assignment.StartDate,
                DateFin = assignment.EndDate,
                EstActive = assignment.IsActive,
                Notes = assignment.Notes
            };
        }
    }
}
