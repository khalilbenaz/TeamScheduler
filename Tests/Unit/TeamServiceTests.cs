using Xunit;
using Microsoft.EntityFrameworkCore;
using TeamScheduler.Application.Services;
using TeamScheduler.Infrastructure.Repositories;
using TeamScheduler.Application.DTOs;
using PlanningPresenceBlazor.Data;

namespace TeamScheduler.Tests.Unit
{
    public class TeamServiceTests : IDisposable
    {
        private readonly PlanningDbContext _context;
        private readonly TeamService _teamService;

        public TeamServiceTests()
        {
            var options = new DbContextOptionsBuilder<PlanningDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new PlanningDbContext(options);
            
            var teamRepository = new TeamRepository(_context);
            var employeeRepository = new EmployeeRepository(_context);
            var clientAssignmentRepository = new TeamClientAssignmentRepository(_context);
            var projectAssignmentRepository = new TeamProjectAssignmentRepository(_context);

            _teamService = new TeamService(
                teamRepository,
                employeeRepository,
                clientAssignmentRepository,
                projectAssignmentRepository);
        }

        [Fact]
        public async Task CreateTeamAsync_ShouldCreateTeamSuccessfully()
        {
            // Arrange
            var createTeamDto = new CreateTeamDto
            {
                Name = "Équipe Test",
                Description = "Une équipe de test",
                Code = "TEST-01",
                IsActive = true,
                MinDailyPresences = 2,
                MaxDailyPresences = 4,
                MinPersonPresences = 3,
                MaxPersonPresences = 5,
                MinCriticalDayPresences = 2
            };

            // Act
            var result = await _teamService.CreateTeamAsync(createTeamDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Équipe Test", result.Name);
            Assert.Equal("TEST-01", result.Code);
            Assert.True(result.IsActive);
        }

        [Fact]
        public async Task GetAllTeamsAsync_ShouldReturnAllTeams()
        {
            // Arrange
            await CreateSampleTeam("Équipe 1", "EQ1");
            await CreateSampleTeam("Équipe 2", "EQ2");

            // Act
            var result = await _teamService.GetAllTeamsAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task IsTeamCodeUniqueAsync_ShouldReturnCorrectResult()
        {
            // Arrange
            await CreateSampleTeam("Équipe Test", "UNIQUE");

            // Act
            var isUniqueForNew = await _teamService.IsTeamCodeUniqueAsync("NEW-CODE");
            var isUniqueForExisting = await _teamService.IsTeamCodeUniqueAsync("UNIQUE");

            // Assert
            Assert.True(isUniqueForNew);
            Assert.False(isUniqueForExisting);
        }

        private async Task<TeamDto> CreateSampleTeam(string name, string code)
        {
            var createDto = new CreateTeamDto
            {
                Name = name,
                Code = code,
                IsActive = true,
                MinDailyPresences = 2,
                MaxDailyPresences = 4,
                MinPersonPresences = 3,
                MaxPersonPresences = 5,
                MinCriticalDayPresences = 2
            };

            return await _teamService.CreateTeamAsync(createDto);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
