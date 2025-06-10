namespace PlanningPresenceBlazor.Services
{
    public static class PlanningConfig
    {
        /// <summary>
        /// Nombre minimum de présences par personne par semaine
        /// </summary>
        public const int MIN_PRESENCES_PAR_PERSONNE = 3;

        /// <summary>
        /// Nombre minimum de présences pour les jours critiques (Lundi, Mardi, Vendredi)
        /// </summary>
        public const int MIN_PRESENCES_PAR_JOUR_CRITIQUE = 2;

        /// <summary>
        /// Nombre minimum de présences pour les jours normaux (Mercredi, Jeudi)
        /// </summary>
        public const int MIN_PRESENCES_PAR_JOUR_NORMAL = 1;

        /// <summary>
        /// Nombre maximum de présences par jour
        /// </summary>
        public const int MAX_PRESENCES_PAR_JOUR = 3;

        /// <summary>
        /// Jours de la semaine travaillés
        /// </summary>
        public static readonly string[] JOURS_TRAVAILLES = { "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi" };

        /// <summary>
        /// Jours critiques nécessitant plus de présences
        /// </summary>
        public static readonly string[] JOURS_CRITIQUES = { "Lundi", "Mardi", "Vendredi" };

        /// <summary>
        /// Vérification de cohérence des contraintes
        /// </summary>
        public static (bool IsValid, string[] Errors) ValidateConstraints(int nombreEmployes)
        {
            var errors = new List<string>();

            // Vérifier que les contraintes sont mathématiquement possibles
            var presencesRequises = nombreEmployes * MIN_PRESENCES_PAR_PERSONNE;
            var presencesMaxPossibles = JOURS_TRAVAILLES.Length * MAX_PRESENCES_PAR_JOUR;

            if (presencesRequises > presencesMaxPossibles)
            {
                errors.Add($"Impossible : {presencesRequises} présences requises > {presencesMaxPossibles} places disponibles");
            }

            // Vérifier la cohérence des minimums par jour
            var minimumTotalParJours = JOURS_CRITIQUES.Length * MIN_PRESENCES_PAR_JOUR_CRITIQUE +
                                      (JOURS_TRAVAILLES.Length - JOURS_CRITIQUES.Length) * MIN_PRESENCES_PAR_JOUR_NORMAL;

            if (minimumTotalParJours > presencesMaxPossibles)
            {
                errors.Add($"Minimums par jour incompatibles avec le maximum journalier");
            }

            return (errors.Count == 0, errors.ToArray());
        }

        /// <summary>
        /// Calculer les statistiques théoriques pour un nombre d'employés donné
        /// </summary>
        public static PlanningStats CalculateTheoreticalStats(int nombreEmployes)
        {
            return new PlanningStats
            {
                PresencesRequises = nombreEmployes * MIN_PRESENCES_PAR_PERSONNE,
                PresencesMaxPossibles = JOURS_TRAVAILLES.Length * MAX_PRESENCES_PAR_JOUR,
                TauxOccupationTheorique = (double)(nombreEmployes * MIN_PRESENCES_PAR_PERSONNE) /
                                         (JOURS_TRAVAILLES.Length * MAX_PRESENCES_PAR_JOUR) * 100,
                NombreEmployes = nombreEmployes
            };
        }
    }

    public class PlanningStats
    {
        public int PresencesRequises { get; set; }
        public int PresencesMaxPossibles { get; set; }
        public double TauxOccupationTheorique { get; set; }
        public int NombreEmployes { get; set; }
    }
}