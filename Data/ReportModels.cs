using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PlanningPresenceBlazor.Data
{
    /// <summary>
    /// Critères de recherche pour générer un rapport
    /// </summary>
    public class ReportCriteria
    {
        [Required(ErrorMessage = "La date de début est requise")]
        public DateTime StartDate { get; set; } = DateTime.Today.AddMonths(-1);

        [Required(ErrorMessage = "La date de fin est requise")]
        public DateTime EndDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Le type de rapport est requis")]
        public ReportType ReportType { get; set; } = ReportType.Presence;

        public Granularity Granularity { get; set; } = Granularity.Weekly;

        public List<int>? EmployeeIds { get; set; }

        public bool IncludeInactiveEmployees { get; set; } = false;

        public string? GeneratedBy { get; set; }

        public ReportFormat ExportFormat { get; set; } = ReportFormat.PDF;

        /// <summary>
        /// Valide que les critères sont corrects
        /// </summary>
        public ValidationResult Validate()
        {
            if (EndDate < StartDate)
            {
                return new ValidationResult("La date de fin doit être après la date de début");
            }

            if ((EndDate - StartDate).TotalDays > 365)
            {
                return new ValidationResult("La période ne peut pas dépasser 365 jours");
            }

            return ValidationResult.Success!;
        }
    }

    /// <summary>
    /// Types de rapports disponibles
    /// </summary>
    public enum ReportType
    {
        [Display(Name = "Analyse des présences")]
        Presence,
        
        [Display(Name = "Analyse des congés")]
        Conges,
        
        [Display(Name = "Analyse d'équité")]
        Equity,
        
        [Display(Name = "Tendances temporelles")]
        Trends,
        
        [Display(Name = "Couverture et effectifs")]
        Coverage
    }

    /// <summary>
    /// Granularité temporelle pour les rapports
    /// </summary>
    public enum Granularity
    {
        [Display(Name = "Quotidienne")]
        Daily,
        
        [Display(Name = "Hebdomadaire")]
        Weekly,
        
        [Display(Name = "Mensuelle")]
        Monthly,
        
        [Display(Name = "Trimestrielle")]
        Quarterly,
        
        [Display(Name = "Annuelle")]
        Yearly
    }

    /// <summary>
    /// Formats d'export disponibles
    /// </summary>
    public enum ReportFormat
    {
        PDF,
        Excel,
        CSV,
        Word,
        HTML
    }

    /// <summary>
    /// Données principales d'un rapport
    /// </summary>
    public class ReportData
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string GeneratedBy { get; set; } = "";
        public ReportType ReportType { get; set; }
        
        // Données communes
        public List<Employe> Employees { get; set; } = new();
        public List<Conge> Conges { get; set; } = new();
        public int TotalEmployees { get; set; }
        public int WorkingDays { get; set; }
        
        // Analyses spécifiques
        public PresenceAnalysis? PresenceAnalysis { get; set; }
        public CongesAnalysis? CongesAnalysis { get; set; }
        public EquityAnalysis? EquityAnalysis { get; set; }
        public TrendsAnalysis? TrendsAnalysis { get; set; }
        public CoverageAnalysis? CoverageAnalysis { get; set; }
        
        // Métadonnées
        public Dictionary<string, object> Metadata { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        
        /// <summary>
        /// Obtient un résumé textuel du rapport
        /// </summary>
        public string GetSummary()
        {
            return ReportType switch
            {
                ReportType.Presence => $"Taux de présence moyen: {PresenceAnalysis?.AveragePresenceRate:F1}%",
                ReportType.Conges => $"Total congés: {CongesAnalysis?.ByEmployee?.Sum(e => e.TotalDays) ?? 0} jours",
                ReportType.Equity => $"Score d'équité: {EquityAnalysis?.EquityScore:F1}%",
                ReportType.Trends => $"Tendance: {TrendsAnalysis?.TrendDirection}",
                ReportType.Coverage => $"Couverture moyenne: {CoverageAnalysis?.AverageCoverage:F1}%",
                _ => "Rapport généré"
            };
        }
    }

    /// <summary>
    /// Analyse des présences
    /// </summary>
    public class PresenceAnalysis
    {
        public List<EmployeePresenceStats> EmployeeStats { get; set; } = new();
        public double AveragePresenceRate { get; set; }
        public int TotalPresences { get; set; }
        public int TotalAbsences { get; set; }
        public List<EmployeePresenceStats> TopPerformers { get; set; } = new();
        public List<EmployeePresenceStats> BottomPerformers { get; set; } = new();
        
        // Statistiques par jour de la semaine
        public Dictionary<DayOfWeek, DayStatistics> DayOfWeekStats { get; set; } = new();
        
        // Périodes d'absence élevée
        public List<HighAbsencePeriod> HighAbsencePeriods { get; set; } = new();
    }

    /// <summary>
    /// Statistiques de présence par employé
    /// </summary>
    public class EmployeePresenceStats
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = "";
        public int TotalPresenceDays { get; set; }
        public int TotalAbsenceDays { get; set; }
        public int TotalCongesDays { get; set; }
        public double PresenceRate { get; set; }
        public double AbsenceRate { get; set; }
        
        // Détail par mois
        public Dictionary<string, MonthlyStats> MonthlyBreakdown { get; set; } = new();
        
        // Patterns d'absence
        public List<string> AbsencePatterns { get; set; } = new();
        
        /// <summary>
        /// Calcule le score de fiabilité
        /// </summary>
        public double GetReliabilityScore()
        {
            if (TotalPresenceDays + TotalAbsenceDays + TotalCongesDays == 0) return 0;
            
            // Score basé sur le taux de présence et la régularité
            var baseScore = PresenceRate;
            var penaltyForAbsences = TotalAbsenceDays * 2; // Pénalité pour absences non justifiées
            
            return Math.Max(0, baseScore - penaltyForAbsences);
        }
    }

    /// <summary>
    /// Statistiques mensuelles
    /// </summary>
    public class MonthlyStats
    {
        public string Month { get; set; } = "";
        public int PresenceDays { get; set; }
        public int AbsenceDays { get; set; }
        public int CongesDays { get; set; }
        public double PresenceRate { get; set; }
    }

    /// <summary>
    /// Statistiques par jour de la semaine
    /// </summary>
    public class DayStatistics
    {
        public DayOfWeek DayOfWeek { get; set; }
        public int TotalOccurrences { get; set; }
        public int TotalPresences { get; set; }
        public int TotalAbsences { get; set; }
        public double AveragePresenceRate { get; set; }
    }

    /// <summary>
    /// Période avec taux d'absence élevé
    /// </summary>
    public class HighAbsencePeriod
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double AbsenceRate { get; set; }
        public List<string> AffectedEmployees { get; set; } = new();
        public string PossibleCause { get; set; } = "";
    }

    /// <summary>
    /// Analyse des congés
    /// </summary>
    public class CongesAnalysis
    {
        public Dictionary<string, CongeTypeStats> ByType { get; set; } = new();
        public List<EmployeeCongeStats> ByEmployee { get; set; } = new();
        public List<PeakPeriod> PeakPeriods { get; set; } = new();
        public double AverageCongeRate { get; set; }
        
        // Analyse saisonnière
        public Dictionary<string, SeasonalStats> SeasonalAnalysis { get; set; } = new();
        
        // Prévisions
        public List<CongeForecast> Forecasts { get; set; } = new();
    }

    /// <summary>
    /// Statistiques par type de congé
    /// </summary>
    public class CongeTypeStats
    {
        public string Type { get; set; } = "";
        public int Count { get; set; }
        public int TotalDays { get; set; }
        public int Employees { get; set; }
        public double AverageDuration { get; set; }
        
        /// <summary>
        /// Obtient la durée moyenne formatée
        /// </summary>
        public string GetFormattedAverageDuration()
        {
            return Count > 0 ? $"{TotalDays / (double)Count:F1} jours" : "0 jour";
        }
    }

    /// <summary>
    /// Statistiques de congé par employé
    /// </summary>
    public class EmployeeCongeStats
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = "";
        public int TotalConges { get; set; }
        public int TotalDays { get; set; }
        public Dictionary<string, int> Types { get; set; } = new();
        public double CongeRate { get; set; }
        
        // Solde de congés
        public CongeBalance Balance { get; set; } = new();
    }

    /// <summary>
    /// Solde de congés
    /// </summary>
    public class CongeBalance
    {
        public int TotalAllowed { get; set; }
        public int Used { get; set; }
        public int Remaining { get; set; }
        public int Planned { get; set; }
        
        public double UsagePercentage => TotalAllowed > 0 ? (Used / (double)TotalAllowed) * 100 : 0;
    }

    /// <summary>
    /// Période de pointe pour les congés
    /// </summary>
    public class PeakPeriod
    {
        public DateTime Date { get; set; }
        public int EmployeeCount { get; set; }
        public bool IsCritical { get; set; }
        public List<string> Employees { get; set; } = new();
        public string Recommendation { get; set; } = "";
    }

    /// <summary>
    /// Statistiques saisonnières
    /// </summary>
    public class SeasonalStats
    {
        public string Season { get; set; } = "";
        public int TotalConges { get; set; }
        public double AveragePerEmployee { get; set; }
        public string Trend { get; set; } = "";
    }

    /// <summary>
    /// Prévision de congés
    /// </summary>
    public class CongeForecast
    {
        public DateTime Period { get; set; }
        public int ExpectedConges { get; set; }
        public double ConfidenceLevel { get; set; }
        public string Recommendation { get; set; } = "";
    }

    /// <summary>
    /// Analyse d'équité
    /// </summary>
    public class EquityAnalysis
    {
        public double Mean { get; set; }
        public double StandardDeviation { get; set; }
        public double VariationCoefficient { get; set; }
        public double GiniIndex { get; set; }
        public double EquityScore { get; set; }
        
        public List<string> Imbalances { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        
        // Distribution détaillée
        public List<WorkloadDistribution> WorkloadDistributions { get; set; } = new();
        
        // Analyse comparative
        public List<EmployeeComparison> Comparisons { get; set; } = new();
    }

    /// <summary>
    /// Distribution de la charge de travail
    /// </summary>
    public class WorkloadDistribution
    {
        public string Range { get; set; } = "";
        public int EmployeeCount { get; set; }
        public double Percentage { get; set; }
        public List<string> Employees { get; set; } = new();
    }

    /// <summary>
    /// Comparaison entre employés
    /// </summary>
    public class EmployeeComparison
    {
        public string Employee1 { get; set; } = "";
        public string Employee2 { get; set; } = "";
        public double WorkloadDifference { get; set; }
        public string ComparisonResult { get; set; } = "";
    }

    /// <summary>
    /// Analyse des tendances
    /// </summary>
    public class TrendsAnalysis
    {
        public List<TrendData> Trends { get; set; } = new();
        public string TrendDirection { get; set; } = "";
        public double TrendPercentage { get; set; }
        
        // Prédictions
        public List<TrendPrediction> Predictions { get; set; } = new();
        
        // Points d'inflexion
        public List<InflectionPoint> InflectionPoints { get; set; } = new();
        
        // Corrélations
        public List<Correlation> Correlations { get; set; } = new();
    }

    /// <summary>
    /// Données de tendance pour une période
    /// </summary>
    public class TrendData
    {
        public string Period { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double PresenceRate { get; set; }
        public int CongesCount { get; set; }
        public int EmployeesOnConge { get; set; }
        
        // Métriques additionnelles
        public Dictionary<string, double> AdditionalMetrics { get; set; } = new();
    }

    /// <summary>
    /// Prédiction de tendance
    /// </summary>
    public class TrendPrediction
    {
        public DateTime Period { get; set; }
        public double PredictedValue { get; set; }
        public double ConfidenceInterval { get; set; }
        public string Method { get; set; } = "";
    }

    /// <summary>
    /// Point d'inflexion dans les tendances
    /// </summary>
    public class InflectionPoint
    {
        public DateTime Date { get; set; }
        public string Type { get; set; } = ""; // "Peak", "Valley", "Change"
        public double Value { get; set; }
        public string Description { get; set; } = "";
    }

    /// <summary>
    /// Corrélation entre variables
    /// </summary>
    public class Correlation
    {
        public string Variable1 { get; set; } = "";
        public string Variable2 { get; set; } = "";
        public double CorrelationCoefficient { get; set; }
        public string Strength { get; set; } = ""; // "Forte", "Moyenne", "Faible"
        public string Direction { get; set; } = ""; // "Positive", "Négative"
    }

    /// <summary>
    /// Analyse de couverture
    /// </summary>
    public class CoverageAnalysis
    {
        public List<DailyCoverage> DailyCoverage { get; set; } = new();
        public double AverageCoverage { get; set; }
        public double MinCoverage { get; set; }
        public double MaxCoverage { get; set; }
        public List<DailyCoverage> CriticalDays { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        
        // Analyse par poste/équipe
        public Dictionary<string, TeamCoverage> TeamCoverages { get; set; } = new();
        
        // Seuils de couverture
        public CoverageThresholds Thresholds { get; set; } = new();
    }

    /// <summary>
    /// Couverture quotidienne
    /// </summary>
    public class DailyCoverage
    {
        public DateTime Date { get; set; }
        public string DayOfWeek { get; set; } = "";
        public int EmployeesPresent { get; set; }
        public int EmployeesAbsent { get; set; }
        public double CoverageRate { get; set; }
        public bool IsCritical { get; set; }
        
        // Détails par équipe/poste
        public Dictionary<string, int> TeamPresence { get; set; } = new();
        
        /// <summary>
        /// Détermine le niveau de criticité
        /// </summary>
        public string GetCriticalityLevel()
        {
            if (CoverageRate >= 80) return "Normal";
            if (CoverageRate >= 60) return "Attention";
            if (CoverageRate >= 40) return "Critique";
            return "Urgence";
        }
    }

    /// <summary>
    /// Couverture par équipe
    /// </summary>
    public class TeamCoverage
    {
        public string TeamName { get; set; } = "";
        public int TotalMembers { get; set; }
        public double AverageCoverage { get; set; }
        public int DaysUnderThreshold { get; set; }
        public List<string> FrequentAbsentees { get; set; } = new();
    }

    /// <summary>
    /// Seuils de couverture
    /// </summary>
    public class CoverageThresholds
    {
        public double Minimum { get; set; } = 60;
        public double Optimal { get; set; } = 80;
        public double Critical { get; set; } = 40;
        
        public Dictionary<string, double> TeamSpecificThresholds { get; set; } = new();
    }

    /// <summary>
    /// Configuration pour la génération de rapports
    /// </summary>
    public class ReportConfiguration
    {
        public bool IncludeCharts { get; set; } = true;
        public bool IncludeDetailedTables { get; set; } = true;
        public bool IncludeRecommendations { get; set; } = true;
        public bool IncludeForecast { get; set; } = false;
        
        public ChartConfiguration ChartConfig { get; set; } = new();
        public ExportConfiguration ExportConfig { get; set; } = new();
    }

    /// <summary>
    /// Configuration des graphiques
    /// </summary>
    public class ChartConfiguration
    {
        public string ColorScheme { get; set; } = "default";
        public bool ShowDataLabels { get; set; } = true;
        public bool AnimateCharts { get; set; } = true;
        public Dictionary<string, ChartTypeConfig> ChartTypes { get; set; } = new();
    }

    /// <summary>
    /// Configuration d'un type de graphique
    /// </summary>
    public class ChartTypeConfig
    {
        public string Type { get; set; } = "";
        public bool Enabled { get; set; } = true;
        public Dictionary<string, object> Options { get; set; } = new();
    }

    /// <summary>
    /// Configuration d'export
    /// </summary>
    public class ExportConfiguration
    {
        public bool IncludeHeader { get; set; } = true;
        public bool IncludeFooter { get; set; } = true;
        public bool IncludePageNumbers { get; set; } = true;
        public string CompanyLogo { get; set; } = "";
        public Dictionary<string, string> CustomFields { get; set; } = new();
    }

    /// <summary>
    /// Résultat de l'export d'un rapport
    /// </summary>
    public class ReportExportResult
    {
        public bool Success { get; set; }
        public string FileName { get; set; } = "";
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = "";
        public long FileSize { get; set; }
        public string Error { get; set; } = "";
    }

    /// <summary>
    /// Historique des rapports générés
    /// </summary>
    public class ReportHistory
    {
        public int Id { get; set; }
        public string ReportId { get; set; } = "";
        public ReportType Type { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string GeneratedBy { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Summary { get; set; } = "";
        public bool IsArchived { get; set; }
        public string FilePath { get; set; } = "";
    }

    /// <summary>
    /// Métadonnées d'un rapport sauvegardé
    /// </summary>
    public class ReportMetadata
    {
        public string Version { get; set; } = "1.0";
        public DateTime CreatedAt { get; set; }
        public DateTime LastModified { get; set; }
        public string CreatedBy { get; set; } = "";
        public string ModifiedBy { get; set; } = "";
        public Dictionary<string, string> Tags { get; set; } = new();
        public List<string> SharedWith { get; set; } = new();
        public bool IsPublic { get; set; }
    }
}