using System;

namespace TeamScheduler.Core.Entities
{
    public class PresenceRecord
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty; // Présence, Télétravail, Site, Client, etc.
        public string? Commentaire { get; set; }
        public virtual Employee? Employee { get; set; }
    }
}
