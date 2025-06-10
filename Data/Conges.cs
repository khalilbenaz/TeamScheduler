using System.ComponentModel.DataAnnotations;

namespace PlanningPresenceBlazor.Data
{
    public class Conge
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire")]
        [StringLength(100, ErrorMessage = "Le nom ne peut pas d�passer 100 caract�res")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "La date de d�but est obligatoire")]
        public DateTime DateDebut { get; set; }

        [Required(ErrorMessage = "La date de fin est obligatoire")]
        public DateTime DateFin { get; set; }

        [StringLength(500, ErrorMessage = "La raison ne peut pas d�passer 500 caract�res")]
        public string? Raison { get; set; }

        public TypeConge Type { get; set; } = TypeConge.CongeAnnuel;

        public DateTime DateCreation { get; set; } = DateTime.Now;

        // Propri�t�s calcul�es
        public int NombreJours => (DateFin - DateDebut).Days + 1;

        public bool EstActif => DateDebut <= DateTime.Today && DateFin >= DateTime.Today;
    }

    public enum TypeConge
    {
        CongeAnnuel,
        CongeMaladie,
        CongeMaternite,
        CongePaternite,
        CongePersonnel,
        Formation
    }
}