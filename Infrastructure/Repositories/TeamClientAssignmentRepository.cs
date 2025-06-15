using TeamScheduler.Core.Entities;
using TeamScheduler.Core.Interfaces;
using PlanningPresenceBlazor.Data;
using Microsoft.EntityFrameworkCore;

namespace TeamScheduler.Infrastructure.Repositories
{
    public class TeamClientAssignmentRepository : ITeamClientAssignmentRepository
    {
        private readonly PlanningDbContext _context;

        public TeamClientAssignmentRepository(PlanningDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TeamClientAssignment>> GetByTeamIdAsync(int teamId)
        {
            var oldAssignments = await _context.AffectationsEquipeClient
                .Where(a => a.EquipeId == teamId)
                .ToListAsync();

            return oldAssignments.Select(MapToNew);
        }

        public async Task<IEnumerable<TeamClientAssignment>> GetByClientIdAsync(int clientId)
        {
            var oldAssignments = await _context.AffectationsEquipeClient
                .Where(a => a.ClientId == clientId)
                .ToListAsync();

            return oldAssignments.Select(MapToNew);
        }

        public async Task<TeamClientAssignment> AddAsync(TeamClientAssignment assignment)
        {
            var oldAssignment = MapToOld(assignment);
            _context.AffectationsEquipeClient.Add(oldAssignment);
            await _context.SaveChangesAsync();
            
            assignment.Id = oldAssignment.Id;
            return assignment;
        }

        public async Task UpdateAsync(TeamClientAssignment assignment)
        {
            var existing = await _context.AffectationsEquipeClient.FindAsync(assignment.Id);
            if (existing != null)
            {
                existing.EquipeId = assignment.TeamId;
                existing.ClientId = assignment.ClientId;
                existing.DateDebut = assignment.StartDate;
                existing.DateFin = assignment.EndDate;
                existing.EstActive = assignment.IsActive;
                existing.Notes = assignment.Notes;
                
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var assignment = await _context.AffectationsEquipeClient.FindAsync(id);
            if (assignment != null)
            {
                _context.AffectationsEquipeClient.Remove(assignment);
                await _context.SaveChangesAsync();
            }
        }

        private static TeamClientAssignment MapToNew(AffectationEquipeClient old)
        {
            return new TeamClientAssignment
            {
                Id = old.Id,
                TeamId = old.EquipeId,
                ClientId = old.ClientId,
                StartDate = old.DateDebut,
                EndDate = old.DateFin,
                IsActive = old.EstActive,
                Notes = old.Notes
            };
        }

        private static AffectationEquipeClient MapToOld(TeamClientAssignment assignment)
        {
            return new AffectationEquipeClient
            {
                Id = assignment.Id,
                EquipeId = assignment.TeamId,
                ClientId = assignment.ClientId,
                DateDebut = assignment.StartDate,
                DateFin = assignment.EndDate,
                EstActive = assignment.IsActive,
                Notes = assignment.Notes
            };
        }
    }
}
