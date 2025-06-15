using System.ComponentModel.DataAnnotations;

namespace PlanningPresenceBlazor.Core.Entities
{
    public class UserSettings
    {
        [Key]
        public int Id { get; set; }
        public int EmployeId { get; set; }
        public string? Theme { get; set; }
        public string? AccentColor { get; set; }
        public string? FontSize { get; set; }
        public bool NotifEmail { get; set; }
        public bool NotifPlanning { get; set; }
        public bool NotifConge { get; set; }
        public bool NotifReport { get; set; }
        public bool NotifReminder { get; set; }
        public string? NotifDaily { get; set; }
        public string? NotifWeekly { get; set; }
        // Ajoutez d'autres paramètres si besoin
    }
}
