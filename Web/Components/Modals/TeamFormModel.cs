using System.ComponentModel.DataAnnotations;

namespace TeamScheduler.Web.Components.Modals
{
    public class TeamFormModel
    {
        [Required(ErrorMessage = "Le nom de l'équipe est requis")]
        [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "La description ne peut pas dépasser 500 caractères")]
        public string? Description { get; set; }
        
        [StringLength(20, ErrorMessage = "Le code ne peut pas dépasser 20 caractères")]
        public string? Code { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public int? TeamLeaderId { get; set; }
        
        [Range(1, 10, ErrorMessage = "Le minimum doit être entre 1 et 10")]
        public int MinDailyPresences { get; set; } = 2;
        
        [Range(1, 10, ErrorMessage = "Le maximum doit être entre 1 et 10")]
        public int MaxDailyPresences { get; set; } = 4;
        
        [Range(1, 7, ErrorMessage = "Le minimum doit être entre 1 et 7")]
        public int MinPersonPresences { get; set; } = 3;
        
        [Range(1, 7, ErrorMessage = "Le maximum doit être entre 1 et 7")]
        public int MaxPersonPresences { get; set; } = 5;
        
        [Range(0, 5, ErrorMessage = "Le minimum doit être entre 0 et 5")]
        public int MinCriticalDayPresences { get; set; } = 2;
    }
}
