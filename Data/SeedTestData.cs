using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PlanningPresenceBlazor.Data;

namespace PlanningPresenceBlazor.DataTools
{
    public static class TestDataSeeder
    {
        public static void SeedPresences(PlanningDbContext db)
        {
            if (db.Presences.Any()) return;
            var employe = db.Employes.FirstOrDefault();
            if (employe == null) return;
            var today = DateTime.Today;
            for (int i = 0; i < 10; i++)
            {
                db.Presences.Add(new Presence
                {
                    EmployeId = employe.Id,
                    Date = today.AddDays(-i),
                    Status = "present"
                });
            }
            db.SaveChanges();
        }
    }
}
