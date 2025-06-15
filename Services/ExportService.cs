using System.IO;
using System.Threading.Tasks;
using System.Text;
using PlanningPresenceBlazor.Data;
using Microsoft.EntityFrameworkCore;

namespace PlanningPresenceBlazor.Services
{
    public class ExportService
    {
        private readonly PlanningDbContext _db;

        public ExportService(PlanningDbContext db)
        {
            _db = db;
        }

        public async Task<string> ExportPlanningAsync()
        {
            var presences = await _db.Presences
                .Include(p => p.Employe)
                .OrderBy(p => p.Date)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Date,Employé,Statut");
            foreach (var p in presences)
            {
                csv.AppendLine($"{p.Date:yyyy-MM-dd},{p.Employe.Nom},{p.Status}");
            }
            return csv.ToString();
        }

        public async Task<string> ExportCongesAsync()
        {
            var conges = await _db.Conges
                .Include(c => c.Employe)
                .OrderBy(c => c.DateDebut)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Employé,Date Début,Date Fin,Type");
            foreach (var c in conges)
            {
                csv.AppendLine($"{c.Employe?.Nom},{c.DateDebut:yyyy-MM-dd},{c.DateFin:yyyy-MM-dd},{c.Type}");
            }
            return csv.ToString();
        }

        public Task<byte[]> ExportToExcelAsync(object data)
        {
            // TODO: Implémenter l'export Excel
            return Task.FromResult(new byte[0]);
        }

        public Task<byte[]> ExportToPdfAsync(object data)
        {
            // TODO: Implémenter l'export PDF
            return Task.FromResult(new byte[0]);
        }
    }
}
