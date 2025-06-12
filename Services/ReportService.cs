using Microsoft.EntityFrameworkCore;
using PlanningPresenceBlazor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlanningPresenceBlazor.Services
{
    public class ReportService
    {
        private readonly PlanningDbContext _context;
        private readonly PlanningService _planningService;

        public ReportService(PlanningDbContext context, PlanningService planningService)
        {
            _context = context;
            _planningService = planningService;
        }

        /// <summary>
        /// Génère un rapport complet selon les critères spécifiés
        /// </summary>
        public async Task<ReportData> GenerateReportAsync(ReportCriteria criteria)
        {
            var report = new ReportData
            {
                StartDate = criteria.StartDate,
                EndDate = criteria.EndDate,
                ReportType = criteria.ReportType,
                GeneratedAt = DateTime.Now,
                GeneratedBy = criteria.GeneratedBy ?? "Système"
            };

            // Charger les données de base
            await LoadBaseDataAsync(report, criteria);

            // Générer le contenu spécifique selon le type de rapport
            switch (criteria.ReportType)
            {
                case ReportType.Presence:
                    await GeneratePresenceReportAsync(report, criteria);
                    break;
                case ReportType.Conges:
                    await GenerateCongesReportAsync(report, criteria);
                    break;
                case ReportType.Equity:
                    await GenerateEquityReportAsync(report, criteria);
                    break;
                case ReportType.Trends:
                    await GenerateTrendsReportAsync(report, criteria);
                    break;
                case ReportType.Coverage:
                    await GenerateCoverageReportAsync(report, criteria);
                    break;
            }

            return report;
        }

        /// <summary>
        /// Charge les données de base communes à tous les rapports
        /// </summary>
        private async Task LoadBaseDataAsync(ReportData report, ReportCriteria criteria)
        {
            // Charger les employés
            var employeesQuery = _context.Employes.AsQueryable();
            
            if (criteria.IncludeInactiveEmployees == false)
            {
                employeesQuery = employeesQuery.Where(e => e.EstActif);
            }

            if (criteria.EmployeeIds?.Any() == true)
            {
                employeesQuery = employeesQuery.Where(e => criteria.EmployeeIds.Contains(e.Id));
            }

            report.Employees = await employeesQuery.ToListAsync();
            report.TotalEmployees = report.Employees.Count;

            // Charger les congés de la période
            report.Conges = await _context.Conges
                .Where(c => c.DateDebut <= criteria.EndDate && c.DateFin >= criteria.StartDate)
                .ToListAsync();

            // Calculer les jours ouvrables
            report.WorkingDays = CalculateWorkingDays(criteria.StartDate, criteria.EndDate);
        }

        /// <summary>
        /// Génère un rapport d'analyse des présences
        /// </summary>
        private async Task GeneratePresenceReportAsync(ReportData report, ReportCriteria criteria)
        {
            report.PresenceAnalysis = new PresenceAnalysis();

            foreach (var employee in report.Employees)
            {
                var stats = new EmployeePresenceStats
                {
                    EmployeeId = employee.Id,
                    EmployeeName = employee.Nom
                };

                // Calculer les jours de congé
                var employeeConges = report.Conges.Where(c => c.Nom == employee.Nom).ToList();
                stats.TotalCongesDays = CalculateCongesDays(employeeConges, criteria.StartDate, criteria.EndDate);

                // Simuler les présences (en production, cela viendrait du planning)
                stats.TotalPresenceDays = report.WorkingDays - stats.TotalCongesDays - Random.Shared.Next(0, 3);
                stats.TotalAbsenceDays = report.WorkingDays - stats.TotalPresenceDays - stats.TotalCongesDays;
                
                stats.PresenceRate = report.WorkingDays > 0 
                    ? (double)stats.TotalPresenceDays / report.WorkingDays * 100 
                    : 0;

                report.PresenceAnalysis.EmployeeStats.Add(stats);
            }

            // Calculer les statistiques globales
            report.PresenceAnalysis.AveragePresenceRate = report.PresenceAnalysis.EmployeeStats.Any() 
                ? report.PresenceAnalysis.EmployeeStats.Average(e => e.PresenceRate) 
                : 0;

            report.PresenceAnalysis.TotalPresences = report.PresenceAnalysis.EmployeeStats.Sum(e => e.TotalPresenceDays);
            report.PresenceAnalysis.TotalAbsences = report.PresenceAnalysis.EmployeeStats.Sum(e => e.TotalAbsenceDays);

            // Identifier les employés avec le meilleur/pire taux
            report.PresenceAnalysis.TopPerformers = report.PresenceAnalysis.EmployeeStats
                .OrderByDescending(e => e.PresenceRate)
                .Take(5)
                .ToList();

            report.PresenceAnalysis.BottomPerformers = report.PresenceAnalysis.EmployeeStats
                .OrderBy(e => e.PresenceRate)
                .Take(5)
                .ToList();
        }

        /// <summary>
        /// Génère un rapport d'analyse des congés
        /// </summary>
        private async Task GenerateCongesReportAsync(ReportData report, ReportCriteria criteria)
        {
            report.CongesAnalysis = new CongesAnalysis();

            // Analyser par type de congé
            report.CongesAnalysis.ByType = report.Conges
                .GroupBy(c => c.Type)
                .ToDictionary(
                    g => g.Key.ToString(), 
                    g => new CongeTypeStats
                    {
                        Count = g.Count(),
                        TotalDays = g.Sum(c => CalculateCongesDays(new[] { c }, criteria.StartDate, criteria.EndDate)),
                        Employees = g.Select(c => c.Nom).Distinct().Count()
                    }
                );

            // Analyser par employé
            report.CongesAnalysis.ByEmployee = report.Conges
                .GroupBy(c => c.Nom)
                .Select(g => new EmployeeCongeStats
                {
                    EmployeeName = g.Key,
                    TotalConges = g.Count(),
                    TotalDays = g.Sum(c => CalculateCongesDays(new[] { c }, criteria.StartDate, criteria.EndDate)),
                    Types = g.GroupBy(c => c.Type).ToDictionary(t => t.Key.ToString(), t => t.Count())
                })
                .OrderByDescending(e => e.TotalDays)
                .ToList();

            // Identifier les périodes de pointe
            report.CongesAnalysis.PeakPeriods = IdentifyPeakPeriods(report.Conges, criteria.StartDate, criteria.EndDate);

            // Calculer le taux de congé moyen
            var totalPossibleDays = report.WorkingDays * report.TotalEmployees;
            var totalCongesDays = report.CongesAnalysis.ByEmployee.Sum(e => e.TotalDays);
            report.CongesAnalysis.AverageCongeRate = totalPossibleDays > 0 
                ? (double)totalCongesDays / totalPossibleDays * 100 
                : 0;
        }

        /// <summary>
        /// Génère un rapport d'analyse d'équité
        /// </summary>
        private async Task GenerateEquityReportAsync(ReportData report, ReportCriteria criteria)
        {
            report.EquityAnalysis = new EquityAnalysis();

            // Calculer la distribution des charges de travail
            var workloads = new List<double>();
            foreach (var employee in report.Employees)
            {
                var employeeConges = report.Conges.Where(c => c.Nom == employee.Nom).ToList();
                var congesDays = CalculateCongesDays(employeeConges, criteria.StartDate, criteria.EndDate);
                var workDays = report.WorkingDays - congesDays;
                workloads.Add(workDays);
            }

            // Calculer les indicateurs statistiques
            if (workloads.Any())
            {
                report.EquityAnalysis.Mean = workloads.Average();
                report.EquityAnalysis.StandardDeviation = CalculateStandardDeviation(workloads);
                report.EquityAnalysis.VariationCoefficient = report.EquityAnalysis.Mean > 0 
                    ? (report.EquityAnalysis.StandardDeviation / report.EquityAnalysis.Mean) * 100 
                    : 0;
                report.EquityAnalysis.GiniIndex = CalculateGiniIndex(workloads);
                
                // Score d'équité (100% = parfait, 0% = très inéquitable)
                report.EquityAnalysis.EquityScore = Math.Max(0, 100 - report.EquityAnalysis.VariationCoefficient);
            }

            // Identifier les déséquilibres
            report.EquityAnalysis.Imbalances = new List<string>();
            if (report.EquityAnalysis.VariationCoefficient > 20)
            {
                report.EquityAnalysis.Imbalances.Add("Forte variation dans la distribution des charges de travail");
            }
            if (report.EquityAnalysis.GiniIndex > 0.3)
            {
                report.EquityAnalysis.Imbalances.Add("Inégalité significative détectée (Gini > 0.3)");
            }

            // Recommandations
            report.EquityAnalysis.Recommendations = GenerateEquityRecommendations(report.EquityAnalysis);
        }

        /// <summary>
        /// Génère un rapport d'analyse des tendances
        /// </summary>
        private async Task GenerateTrendsReportAsync(ReportData report, ReportCriteria criteria)
        {
            report.TrendsAnalysis = new TrendsAnalysis();

            // Diviser la période en intervalles selon la granularité
            var intervals = GenerateTimeIntervals(criteria.StartDate, criteria.EndDate, criteria.Granularity);

            foreach (var interval in intervals)
            {
                var trend = new TrendData
                {
                    Period = interval.Label,
                    StartDate = interval.Start,
                    EndDate = interval.End
                };

                // Analyser les congés pour cette période
                var periodConges = report.Conges
                    .Where(c => c.DateDebut <= interval.End && c.DateFin >= interval.Start)
                    .ToList();

                trend.CongesCount = periodConges.Count;
                trend.EmployeesOnConge = periodConges.Select(c => c.Nom).Distinct().Count();
                
                // Simuler les présences
                var workingDays = CalculateWorkingDays(interval.Start, interval.End);
                var totalPossible = workingDays * report.TotalEmployees;
                var totalCongesDays = periodConges.Sum(c => CalculateCongesDays(new[] { c }, interval.Start, interval.End));
                
                trend.PresenceRate = totalPossible > 0 
                    ? (double)(totalPossible - totalCongesDays) / totalPossible * 100 
                    : 0;

                report.TrendsAnalysis.Trends.Add(trend);
            }

            // Identifier les tendances
            if (report.TrendsAnalysis.Trends.Count > 1)
            {
                var firstHalf = report.TrendsAnalysis.Trends.Take(report.TrendsAnalysis.Trends.Count / 2).Average(t => t.PresenceRate);
                var secondHalf = report.TrendsAnalysis.Trends.Skip(report.TrendsAnalysis.Trends.Count / 2).Average(t => t.PresenceRate);
                
                report.TrendsAnalysis.TrendDirection = secondHalf > firstHalf ? "Amélioration" : "Dégradation";
                report.TrendsAnalysis.TrendPercentage = Math.Abs(secondHalf - firstHalf);
            }
        }

        /// <summary>
        /// Génère un rapport d'analyse de couverture
        /// </summary>
        private async Task GenerateCoverageReportAsync(ReportData report, ReportCriteria criteria)
        {
            report.CoverageAnalysis = new CoverageAnalysis();

            // Analyser la couverture jour par jour
            var currentDate = criteria.StartDate;
            while (currentDate <= criteria.EndDate)
            {
                if (IsWorkingDay(currentDate))
                {
                    var coverage = new DailyCoverage
                    {
                        Date = currentDate,
                        DayOfWeek = currentDate.DayOfWeek.ToString()
                    };

                    // Compter les employés en congé ce jour
                    var employeesOnConge = report.Conges
                        .Where(c => c.DateDebut <= currentDate && c.DateFin >= currentDate)
                        .Select(c => c.Nom)
                        .Distinct()
                        .Count();

                    coverage.EmployeesPresent = report.TotalEmployees - employeesOnConge;
                    coverage.EmployeesAbsent = employeesOnConge;
                    coverage.CoverageRate = report.TotalEmployees > 0 
                        ? (double)coverage.EmployeesPresent / report.TotalEmployees * 100 
                        : 0;

                    report.CoverageAnalysis.DailyCoverage.Add(coverage);
                }

                currentDate = currentDate.AddDays(1);
            }

            // Calculer les statistiques
            if (report.CoverageAnalysis.DailyCoverage.Any())
            {
                report.CoverageAnalysis.AverageCoverage = report.CoverageAnalysis.DailyCoverage.Average(d => d.CoverageRate);
                report.CoverageAnalysis.MinCoverage = report.CoverageAnalysis.DailyCoverage.Min(d => d.CoverageRate);
                report.CoverageAnalysis.MaxCoverage = report.CoverageAnalysis.DailyCoverage.Max(d => d.CoverageRate);
                
                // Identifier les jours critiques (couverture < 70%)
                report.CoverageAnalysis.CriticalDays = report.CoverageAnalysis.DailyCoverage
                    .Where(d => d.CoverageRate < 70)
                    .OrderBy(d => d.CoverageRate)
                    .ToList();
            }

            // Recommandations
            report.CoverageAnalysis.Recommendations = GenerateCoverageRecommendations(report.CoverageAnalysis);
        }

        /// <summary>
        /// Calcule le nombre de jours ouvrables entre deux dates
        /// </summary>
        private int CalculateWorkingDays(DateTime start, DateTime end)
        {
            int days = 0;
            var current = start;
            
            while (current <= end)
            {
                if (IsWorkingDay(current))
                {
                    days++;
                }
                current = current.AddDays(1);
            }
            
            return days;
        }

        /// <summary>
        /// Détermine si une date est un jour ouvrable
        /// </summary>
        private bool IsWorkingDay(DateTime date)
        {
            return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
        }

        /// <summary>
        /// Calcule le nombre de jours de congé dans une période donnée
        /// </summary>
        private int CalculateCongesDays(IEnumerable<Conge> conges, DateTime periodStart, DateTime periodEnd)
        {
            int totalDays = 0;
            
            foreach (var conge in conges)
            {
                var start = conge.DateDebut < periodStart ? periodStart : conge.DateDebut;
                var end = conge.DateFin > periodEnd ? periodEnd : conge.DateFin;
                
                if (start <= end)
                {
                    totalDays += CalculateWorkingDays(start, end);
                }
            }
            
            return totalDays;
        }

        /// <summary>
        /// Calcule l'écart-type d'une série de valeurs
        /// </summary>
        private double CalculateStandardDeviation(List<double> values)
        {
            if (!values.Any()) return 0;
            
            double mean = values.Average();
            double variance = values.Select(v => Math.Pow(v - mean, 2)).Average();
            return Math.Sqrt(variance);
        }

        /// <summary>
        /// Calcule l'indice de Gini pour mesurer l'inégalité
        /// </summary>
        private double CalculateGiniIndex(List<double> values)
        {
            if (!values.Any()) return 0;
            
            var sortedValues = values.OrderBy(v => v).ToList();
            int n = sortedValues.Count;
            double sumOfProducts = 0;
            
            for (int i = 0; i < n; i++)
            {
                sumOfProducts += (2.0 * (i + 1) - n - 1) * sortedValues[i];
            }
            
            double sum = sortedValues.Sum();
            return sum > 0 ? sumOfProducts / (n * sum) : 0;
        }

        /// <summary>
        /// Identifie les périodes de pointe pour les congés
        /// </summary>
        private List<PeakPeriod> IdentifyPeakPeriods(List<Conge> conges, DateTime start, DateTime end)
        {
            var peakPeriods = new List<PeakPeriod>();
            var dailyCounts = new Dictionary<DateTime, int>();
            
            // Compter les congés par jour
            var current = start;
            while (current <= end)
            {
                if (IsWorkingDay(current))
                {
                    var count = conges.Count(c => c.DateDebut <= current && c.DateFin >= current);
                    dailyCounts[current] = count;
                }
                current = current.AddDays(1);
            }
            
            // Identifier les pics (jours avec plus de 3 congés simultanés)
            var threshold = 3;
            foreach (var kvp in dailyCounts.Where(d => d.Value > threshold).OrderByDescending(d => d.Value))
            {
                peakPeriods.Add(new PeakPeriod
                {
                    Date = kvp.Key,
                    EmployeeCount = kvp.Value,
                    IsCritical = kvp.Value > 5
                });
            }
            
            return peakPeriods.Take(10).ToList(); // Top 10 peak days
        }

        /// <summary>
        /// Génère des intervalles de temps selon la granularité
        /// </summary>
        private List<TimeInterval> GenerateTimeIntervals(DateTime start, DateTime end, Granularity granularity)
        {
            var intervals = new List<TimeInterval>();
            var current = start;
            
            while (current <= end)
            {
                var interval = new TimeInterval { Start = current };
                
                switch (granularity)
                {
                    case Granularity.Weekly:
                        interval.End = current.AddDays(6);
                        interval.Label = $"Semaine du {current:dd/MM}";
                        break;
                    case Granularity.Monthly:
                        interval.End = current.AddMonths(1).AddDays(-1);
                        interval.Label = current.ToString("MMMM yyyy", new System.Globalization.CultureInfo("fr-FR"));
                        break;
                    case Granularity.Quarterly:
                        interval.End = current.AddMonths(3).AddDays(-1);
                        interval.Label = $"T{(current.Month - 1) / 3 + 1} {current.Year}";
                        break;
                    default:
                        interval.End = current;
                        interval.Label = current.ToString("dd/MM/yyyy");
                        break;
                }
                
                if (interval.End > end) interval.End = end;
                intervals.Add(interval);
                
                current = interval.End.AddDays(1);
            }
            
            return intervals;
        }

        /// <summary>
        /// Génère des recommandations d'équité
        /// </summary>
        private List<string> GenerateEquityRecommendations(EquityAnalysis analysis)
        {
            var recommendations = new List<string>();
            
            if (analysis.VariationCoefficient > 20)
            {
                recommendations.Add("Envisager une rotation des employés pour équilibrer la charge de travail");
            }
            
            if (analysis.GiniIndex > 0.3)
            {
                recommendations.Add("Revoir la politique d'attribution des congés pour plus d'équité");
            }
            
            if (analysis.EquityScore < 70)
            {
                recommendations.Add("Mettre en place un système de compensation pour les employés les plus sollicités");
            }
            
            return recommendations;
        }

        /// <summary>
        /// Génère des recommandations de couverture
        /// </summary>
        private List<string> GenerateCoverageRecommendations(CoverageAnalysis analysis)
        {
            var recommendations = new List<string>();
            
            if (analysis.MinCoverage < 60)
            {
                recommendations.Add("Établir un minimum d'effectif requis pour éviter les sous-effectifs critiques");
            }
            
            if (analysis.CriticalDays.Count > 5)
            {
                recommendations.Add($"Attention: {analysis.CriticalDays.Count} jours avec une couverture insuffisante détectés");
            }
            
            if (analysis.AverageCoverage < 75)
            {
                recommendations.Add("Envisager le recrutement de personnel supplémentaire ou l'ajustement du calendrier des congés");
            }
            
            return recommendations;
        }

        /// <summary>
        /// Exporte un rapport au format PDF
        /// </summary>
        public async Task<byte[]> ExportToPdfAsync(ReportData report)
        {
            // Simulation de génération PDF
            // En production, utiliser une bibliothèque comme iTextSharp ou QuestPDF
            await Task.Delay(100);
            
            // Retourner des données fictives pour la démo
            return System.Text.Encoding.UTF8.GetBytes($"PDF Report - {report.ReportType} - Generated at {report.GeneratedAt}");
        }

        /// <summary>
        /// Exporte un rapport au format Excel
        /// </summary>
        public async Task<byte[]> ExportToExcelAsync(ReportData report)
        {
            // Simulation de génération Excel
            // En production, utiliser une bibliothèque comme EPPlus ou ClosedXML
            await Task.Delay(100);
            
            // Retourner des données fictives pour la démo
            return System.Text.Encoding.UTF8.GetBytes($"Excel Report - {report.ReportType} - Generated at {report.GeneratedAt}");
        }

        /// <summary>
        /// Sauvegarde un rapport pour consultation ultérieure
        /// </summary>
        public async Task<int> SaveReportAsync(ReportData report)
        {
            // En production, sauvegarder en base de données
            await Task.Delay(100);
            
            // Retourner un ID fictif
            return Random.Shared.Next(1000, 9999);
        }

        /// <summary>
        /// Récupère les rapports sauvegardés
        /// </summary>
        public async Task<List<SavedReport>> GetSavedReportsAsync(int? employeeId = null)
        {
            // En production, récupérer depuis la base de données
            await Task.Delay(100);
            
            // Retourner des données fictives pour la démo
            return new List<SavedReport>
            {
                new() { Id = 1, Name = "Rapport présences Janvier", CreatedAt = DateTime.Now.AddDays(-5), Type = ReportType.Presence },
                new() { Id = 2, Name = "Analyse équité Q1", CreatedAt = DateTime.Now.AddDays(-10), Type = ReportType.Equity },
                new() { Id = 3, Name = "Tendances annuelles", CreatedAt = DateTime.Now.AddDays(-15), Type = ReportType.Trends }
            };
        }
    }

    // Classes d'aide
    public class TimeInterval
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Label { get; set; } = "";
    }

    public class SavedReport
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public ReportType Type { get; set; }
    }
}