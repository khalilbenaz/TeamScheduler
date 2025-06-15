using CsvHelper;
using PlanningPresenceBlazor.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

namespace PlanningPresenceBlazor.Services
{
    public class PlanningService
    {
        private readonly PlanningDbContext _db;
        private static readonly string[] WeekDays = { "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi" };
        private static readonly string[] CriticalDays = { "Lundi", "Mardi", "Vendredi" };
        private static readonly string[] FlexibleDays = { "Mercredi", "Jeudi" };

        // Contraintes mises à jour
        private const int MIN_PRESENCES_PAR_PERSONNE = 3;
        private const int MAX_PRESENCES_PAR_PERSONNE = 3; // Maximum strict sauf si congés
        private const int PRESENCES_EXACTES_JOURS_CRITIQUES = 2;

        public PlanningService(PlanningDbContext db)
        {
            _db = db;
        }

        public async Task<List<Employe>> GetAllEmployesAsync()
        {
            return await _db.Employes.Where(e => e.EstActif).OrderBy(e => e.Nom).ToListAsync();
        }

        public async Task<List<Conge>> GetCongesForPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await _db.Conges
                .Where(c => c.DateDebut <= endDate && c.DateFin >= startDate)
                .OrderBy(c => c.DateDebut)
                .ToListAsync();
        }

        public async Task<bool> SaveCongesToDbAsync(List<CongeCsv> congesCsv, DateTime weekStart, DateTime weekEnd)
        {
            try
            {
                var conges = new List<Conge>();

                foreach (var congeCsv in congesCsv)
                {
                    if (DateTime.TryParse(congeCsv.DateDebut, out DateTime dateDebut) &&
                        DateTime.TryParse(congeCsv.DateFin, out DateTime dateFin) &&
                        !string.IsNullOrWhiteSpace(congeCsv.Nom))
                    {
                        conges.Add(new Conge
                        {
                            Nom = congeCsv.Nom.Trim(),
                            DateDebut = dateDebut,
                            DateFin = dateFin,
                            Raison = congeCsv.Raison?.Trim(),
                            Type = ParseTypeConge(congeCsv.Type)
                        });
                    }
                }

                if (conges.Any())
                {
                    // Restreindre la suppression aux congés de la semaine affichée
                    var existingConges = await _db.Conges
                        .Where(c => c.DateDebut <= weekEnd && c.DateFin >= weekStart)
                        .ToListAsync();

                    _db.Conges.RemoveRange(existingConges);
                    await _db.Conges.AddRangeAsync(conges);
                    await _db.SaveChangesAsync();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<PlanningResult> GeneratePlanningAsync(DateTime weekStart)
        {
            var employes = await GetAllEmployesAsync();
            var allNames = employes.Select(e => e.Nom).ToList();
            var weekEnd = weekStart.AddDays(6);
            var conges = await GetCongesForPeriodAsync(weekStart, weekEnd);

            // Initialiser le planning
            var planning = allNames.ToDictionary(name => name,
                name => WeekDays.ToDictionary(day => day, _ => new PresenceInfo
                {
                    Status = PresenceStatus.Absent,
                    Note = ""
                }));

            var warnings = new List<string>();

            // Marquer les congés
            MarkVacations(planning, conges, weekStart);

            // Générer planning avec équité et rotation améliorée
            var success = GenerateOptimizedPlanning(planning, allNames, conges, weekStart, warnings);

            if (!success)
            {
                warnings.Add("⚠️ Impossible de satisfaire toutes les contraintes avec la configuration actuelle");
            }

            // Vérifications finales
            ValidateFinalPlanning(planning, warnings);

            return new PlanningResult
            {
                Planning = planning,
                Warnings = warnings,
                WeekStart = weekStart,
                WeekEnd = weekEnd,
                TotalEmployes = allNames.Count,
                EmployesEnConge = conges.Select(c => c.Nom).Distinct().Count()
            };
        }

        private void MarkVacations(
            Dictionary<string, Dictionary<string, PresenceInfo>> planning,
            List<Conge> conges,
            DateTime weekStart)
        {
            foreach (var day in WeekDays)
            {
                DateTime currentDate = GetDateFromDayName(weekStart, day);
                var unavailable = conges
                    .Where(c => currentDate >= c.DateDebut && currentDate <= c.DateFin)
                    .ToList();

                foreach (var conge in unavailable)
                {
                    if (planning.ContainsKey(conge.Nom))
                    {
                        planning[conge.Nom][day] = new PresenceInfo
                        {
                            Status = PresenceStatus.Conge,
                            Note = GetTypeDisplayName(conge.Type)
                        };
                    }
                }
            }
        }

        private bool GenerateOptimizedPlanning(
            Dictionary<string, Dictionary<string, PresenceInfo>> planning,
            List<string> allNames,
            List<Conge> conges,
            DateTime weekStart,
            List<string> warnings)
        {
            // Calculer les disponibilités par jour
            var availabilityByDay = CalculateAvailability(allNames, conges, weekStart);

            // Générer un seed basé sur la semaine pour la rotation
            var weekNumber = GetWeekNumber(weekStart);
            var random = new Random(weekNumber);

            // Compter les congés par personne cette semaine
            var congesByPerson = allNames.ToDictionary(name => name, name =>
                WeekDays.Count(day => planning[name][day].Status == PresenceStatus.Conge));

            // Phase 1: Assigner exactement 2 personnes aux jours critiques (Lu/Ma/Ve)
            if (!AssignCriticalDaysOptimized(planning, availabilityByDay, random, warnings, congesByPerson))
            {
                return false;
            }

            // Phase 2: Assigner les jours flexibles (Me/Je) avec gestion intelligente
            AssignFlexibleDaysIntelligent(planning, availabilityByDay, random, weekNumber, congesByPerson);

            // Phase 3: Équilibrer pour garantir exactement 3 présences par personne (sauf exceptions)
            BalancePresencesStrictly(planning, availabilityByDay, allNames, random, warnings, congesByPerson);

            return true;
        }

        private Dictionary<string, List<string>> CalculateAvailability(
            List<string> allNames,
            List<Conge> conges,
            DateTime weekStart)
        {
            var availability = new Dictionary<string, List<string>>();

            foreach (var day in WeekDays)
            {
                DateTime currentDate = GetDateFromDayName(weekStart, day);
                var unavailable = conges
                    .Where(c => currentDate >= c.DateDebut && currentDate <= c.DateFin)
                    .Select(c => c.Nom)
                    .ToHashSet();

                availability[day] = allNames.Where(name => !unavailable.Contains(name)).ToList();
            }

            return availability;
        }

        private bool AssignCriticalDaysOptimized(
            Dictionary<string, Dictionary<string, PresenceInfo>> planning,
            Dictionary<string, List<string>> availabilityByDay,
            Random random,
            List<string> warnings,
            Dictionary<string, int> congesByPerson)
        {
            foreach (var day in CriticalDays)
            {
                var available = availabilityByDay[day];

                if (available.Count < PRESENCES_EXACTES_JOURS_CRITIQUES)
                {
                    warnings.Add($"⚠️ {day}: Seulement {available.Count} personne(s) disponible(s), impossible d'assigner {PRESENCES_EXACTES_JOURS_CRITIQUES} personnes");
                    continue;
                }

                // Prioriser par : 1) moins de présences actuelles 2) plus de congés 3) rotation
                var prioritized = available
                    .OrderBy(person => GetCurrentPresences(planning, person))
                    .ThenByDescending(person => congesByPerson[person])
                    .ThenBy(x => random.Next())
                    .ToList();

                // Assigner exactement 2 personnes
                for (int i = 0; i < PRESENCES_EXACTES_JOURS_CRITIQUES && i < prioritized.Count; i++)
                {
                    planning[prioritized[i]][day] = new PresenceInfo
                    {
                        Status = PresenceStatus.Present,
                        Note = "Jour critique"
                    };
                }
            }

            return true;
        }

        private void AssignFlexibleDaysIntelligent(
            Dictionary<string, Dictionary<string, PresenceInfo>> planning,
            Dictionary<string, List<string>> availabilityByDay,
            Random random,
            int weekNumber,
            Dictionary<string, int> congesByPerson)
        {
            foreach (var day in FlexibleDays)
            {
                var available = availabilityByDay[day];
                if (available.Count == 0) continue;

                // Calculer qui a besoin de plus de présences pour atteindre 3
                var needingMore = available
                    .Where(person => GetCurrentPresences(planning, person) < MIN_PRESENCES_PAR_PERSONNE)
                    .OrderBy(person => GetCurrentPresences(planning, person))
                    .ThenByDescending(person => congesByPerson[person])
                    .ToList();

                if (needingMore.Any())
                {
                    // Assigner en priorité à ceux qui en ont besoin, mais pas plus de 2-3 personnes par jour flexible
                    var toAssign = needingMore.Take(Math.Min(3, available.Count)).ToList();

                    foreach (var person in toAssign)
                    {
                        planning[person][day] = new PresenceInfo
                        {
                            Status = PresenceStatus.Present,
                            Note = "Complément minimum"
                        };
                    }
                }
                else
                {
                    // Si tout le monde a déjà assez, rotation équitable avec 1-2 personnes maximum
                    var rotationOffset = weekNumber % available.Count;
                    var maxAssignments = Math.Min(2, available.Count);

                    var toAssign = available
                        .Skip(rotationOffset)
                        .Take(maxAssignments)
                        .Concat(available.Take(Math.Max(0, maxAssignments - (available.Count - rotationOffset))))
                        .Where(person => GetCurrentPresences(planning, person) < MAX_PRESENCES_PAR_PERSONNE)
                        .ToList();

                    foreach (var person in toAssign)
                    {
                        planning[person][day] = new PresenceInfo
                        {
                            Status = PresenceStatus.Present,
                            Note = "Rotation équitable"
                        };
                    }
                }
            }
        }

        private void BalancePresencesStrictly(
            Dictionary<string, Dictionary<string, PresenceInfo>> planning,
            Dictionary<string, List<string>> availabilityByDay,
            List<string> allNames,
            Random random,
            List<string> warnings,
            Dictionary<string, int> congesByPerson)
        {
            // Phase 1: Assurer que tout le monde a au moins 3 présences
            foreach (var person in allNames)
            {
                var currentPresences = GetCurrentPresences(planning, person);
                var personConges = congesByPerson[person];

                while (currentPresences < MIN_PRESENCES_PAR_PERSONNE)
                {
                    var bestDay = FindBestDayForAssignment(person, planning, availabilityByDay, random);

                    if (bestDay == null)
                    {
                        // Si la personne a beaucoup de congés, on peut accepter moins de 3 présences
                        if (personConges >= 3)
                        {
                            warnings.Add($"ℹ️ {person} a seulement {currentPresences} présence(s) à cause de {personConges} jour(s) de congé");
                        }
                        else
                        {
                            warnings.Add($"⚠️ Impossible d'assigner {MIN_PRESENCES_PAR_PERSONNE} présences à {person} (actuellement {currentPresences})");
                        }
                        break;
                    }

                    planning[person][bestDay] = new PresenceInfo
                    {
                        Status = PresenceStatus.Present,
                        Note = "Ajustement équité"
                    };

                    currentPresences++;
                }
            }

            // Phase 2: Limiter strictement à 3 présences maximum (sauf si congés compensatoires)
            foreach (var person in allNames)
            {
                var currentPresences = GetCurrentPresences(planning, person);
                var personConges = congesByPerson[person];

                // Maximum autorisé: 3 + (congés > 0 ? 1 : 0)
                var maxAllowed = MAX_PRESENCES_PAR_PERSONNE + (personConges > 0 ? 1 : 0);

                if (currentPresences > maxAllowed)
                {
                    // Retirer les présences excédentaires en commençant par les jours flexibles
                    var presenceDays = WeekDays
                        .Where(day => planning[person][day].Status == PresenceStatus.Present)
                        .OrderBy(day => FlexibleDays.Contains(day) ? 0 : 1) // Flexibles d'abord
                        .ToList();

                    int toRemove = currentPresences - maxAllowed;
                    for (int i = 0; i < toRemove && i < presenceDays.Count; i++)
                    {
                        var dayToRemove = presenceDays[i];
                        // Ne pas retirer si c'est un jour critique avec seulement 2 personnes
                        if (CriticalDays.Contains(dayToRemove))
                        {
                            var criticalDayPresences = planning.Values.Count(p => p[dayToRemove].Status == PresenceStatus.Present);
                            if (criticalDayPresences <= PRESENCES_EXACTES_JOURS_CRITIQUES)
                                continue;
                        }

                        planning[person][dayToRemove] = new PresenceInfo
                        {
                            Status = PresenceStatus.Absent,
                            Note = ""
                        };
                    }

                    warnings.Add($"ℹ️ {person}: présences réduites de {currentPresences} à {GetCurrentPresences(planning, person)} pour respecter l'équité");
                }
            }
        }

        private int GetCurrentPresences(Dictionary<string, Dictionary<string, PresenceInfo>> planning, string person)
        {
            return planning[person].Count(kvp => kvp.Value.Status == PresenceStatus.Present);
        }

        private string? FindBestDayForAssignment(
            string person,
            Dictionary<string, Dictionary<string, PresenceInfo>> planning,
            Dictionary<string, List<string>> availabilityByDay,
            Random random)
        {
            var candidateDays = WeekDays
                .Where(day => availabilityByDay[day].Contains(person) &&
                             planning[person][day].Status == PresenceStatus.Absent)
                .ToList();

            if (!candidateDays.Any()) return null;

            // Priorité aux jours flexibles pour ne pas perturber les jours critiques
            var flexibleCandidates = candidateDays.Where(day => FlexibleDays.Contains(day)).ToList();

            if (flexibleCandidates.Any())
            {
                return flexibleCandidates[random.Next(flexibleCandidates.Count)];
            }

            // Sinon, vérifier les jours critiques si possible sans violer la contrainte
            var criticalCandidates = candidateDays.Where(day => CriticalDays.Contains(day)).ToList();

            foreach (var day in criticalCandidates)
            {
                var currentAssigned = planning.Values.Count(p => p[day].Status == PresenceStatus.Present);
                if (currentAssigned < PRESENCES_EXACTES_JOURS_CRITIQUES)
                {
                    return day;
                }
            }

            return null;
        }

        private void ValidateFinalPlanning(
            Dictionary<string, Dictionary<string, PresenceInfo>> planning,
            List<string> warnings)
        {
            // Vérifier les présences par personne
            foreach (var person in planning.Keys)
            {
                var presences = planning[person].Count(kvp => kvp.Value.Status == PresenceStatus.Present);
                var conges = planning[person].Count(kvp => kvp.Value.Status == PresenceStatus.Conge);

                if (presences < MIN_PRESENCES_PAR_PERSONNE && conges < 3)
                {
                    warnings.Add($"⚠️ {person}: seulement {presences} présence(s) au lieu de {MIN_PRESENCES_PAR_PERSONNE} minimum");
                }
                else if (presences > MAX_PRESENCES_PAR_PERSONNE + (conges > 0 ? 1 : 0))
                {
                    warnings.Add($"⚠️ {person}: {presences} présence(s), dépassement de l'équité");
                }
            }

            // Vérifier les jours critiques
            foreach (var day in CriticalDays)
            {
                var presences = planning.Values.Count(p => p[day].Status == PresenceStatus.Present);
                if (presences != PRESENCES_EXACTES_JOURS_CRITIQUES)
                {
                    warnings.Add($"⚠️ {day}: {presences} présence(s) au lieu de {PRESENCES_EXACTES_JOURS_CRITIQUES} requis exactement");
                }
            }
        }

        private int GetWeekNumber(DateTime date)
        {
            var jan1 = new DateTime(date.Year, 1, 1);
            var daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;
            var firstThursday = jan1.AddDays(daysOffset);
            var cal = System.Globalization.CultureInfo.CurrentCulture.Calendar;
            var weekNum = cal.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return weekNum;
        }

        public async Task<byte[]> ExportPlanningToCsvAsync(Dictionary<string, Dictionary<string, PresenceInfo>> planning)
        {
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, Encoding.UTF8);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            // En-têtes
            csv.WriteField("Nom");
            foreach (var day in WeekDays)
                csv.WriteField(day);
            csv.WriteField("Total Présences");
            csv.WriteField("Objectif 3+ Atteint");
            csv.WriteField("Équitable (≤3)");
            csv.NextRecord();

            // Données des employés
            foreach (var kvp in planning.OrderBy(x => x.Key))
            {
                csv.WriteField(kvp.Key);
                int totalPresences = 0;

                foreach (var day in WeekDays)
                {
                    var presence = kvp.Value[day];
                    var statusText = presence.Status switch
                    {
                        PresenceStatus.Present => "Présent",
                        PresenceStatus.Conge => $"Congé ({presence.Note})",
                        _ => "Absent"
                    };

                    csv.WriteField(statusText);
                    if (presence.Status == PresenceStatus.Present) totalPresences++;
                }

                csv.WriteField(totalPresences.ToString());
                csv.WriteField(totalPresences >= MIN_PRESENCES_PAR_PERSONNE ? "✓" : "✗");
                csv.WriteField(totalPresences <= MAX_PRESENCES_PAR_PERSONNE ? "✓" : "Surcharge");
                csv.NextRecord();
            }

            await writer.FlushAsync();
            return stream.ToArray();
        }

        // NOUVELLE MÉTHODE: Export de rotation CSV
        public async Task<byte[]> ExportRotationToCsvAsync(List<PlanningResult> rotationResults)
        {
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, Encoding.UTF8);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            if (!rotationResults.Any()) return stream.ToArray();

            var allEmployees = rotationResults.First().Planning.Keys.OrderBy(x => x).ToList();

            // En-tête principal
            csv.WriteField("Employé");

            // En-têtes des semaines avec jours
            for (int weekIndex = 0; weekIndex < rotationResults.Count; weekIndex++)
            {
                var weekStart = rotationResults[weekIndex].WeekStart;
                foreach (var day in WeekDays)
                {
                    csv.WriteField($"S{weekIndex + 1}_{day.Substring(0, 2)}");
                }
                csv.WriteField($"S{weekIndex + 1}_Total");
            }

            csv.WriteField("Total Général");
            csv.WriteField("Moyenne/Semaine");
            csv.WriteField("Écart Type");
            csv.NextRecord();

            // Données par employé
            foreach (var employee in allEmployees)
            {
                csv.WriteField(employee);

                var weeklyTotals = new List<int>();

                // Pour chaque semaine
                foreach (var weekResult in rotationResults)
                {
                    int weekTotal = 0;

                    // Pour chaque jour de la semaine
                    foreach (var day in WeekDays)
                    {
                        var presence = weekResult.Planning[employee][day];
                        var statusCode = presence.Status switch
                        {
                            PresenceStatus.Present => "P",
                            PresenceStatus.Conge => "C",
                            _ => "-"
                        };

                        csv.WriteField(statusCode);
                        if (presence.Status == PresenceStatus.Present) weekTotal++;
                    }

                    csv.WriteField(weekTotal.ToString());
                    weeklyTotals.Add(weekTotal);
                }

                // Statistiques globales
                var total = weeklyTotals.Sum();
                var average = weeklyTotals.Average();
                var variance = weeklyTotals.Select(x => Math.Pow(x - average, 2)).Average();
                var stdDev = Math.Sqrt(variance);

                csv.WriteField(total.ToString());
                csv.WriteField(average.ToString("F1"));
                csv.WriteField(stdDev.ToString("F1"));
                csv.NextRecord();
            }

            // Ligne de totaux par jour
            csv.WriteField("TOTAL/JOUR");

            foreach (var weekResult in rotationResults)
            {
                foreach (var day in WeekDays)
                {
                    var dayTotal = weekResult.Planning.Values.Count(p => p[day].Status == PresenceStatus.Present);
                    csv.WriteField(dayTotal.ToString());
                }

                var weekTotal = weekResult.Planning.SelectMany(x => x.Value.Values).Count(x => x.Status == PresenceStatus.Present);
                csv.WriteField(weekTotal.ToString());
            }

            csv.WriteField(""); // Total général
            csv.WriteField(""); // Moyenne
            csv.WriteField(""); // Écart type
            csv.NextRecord();

            await writer.FlushAsync();
            return stream.ToArray();
        }

        // Méthode pour générer les notifications de présence
        public List<PresenceNotification> GeneratePresenceNotifications(Dictionary<string, Dictionary<string, PresenceInfo>> planning, DateTime weekStart)
        {
            var notifications = new List<PresenceNotification>();

            foreach (var employee in planning)
            {
                var presentDays = new List<string>();

                foreach (var day in WeekDays)
                {
                    if (employee.Value[day].Status == PresenceStatus.Present)
                    {
                        var dayDate = GetDateFromDayName(weekStart, day);
                        presentDays.Add($"{day} {dayDate:dd/MM}");
                    }
                }

                if (presentDays.Any())
                {
                    notifications.Add(new PresenceNotification
                    {
                        EmployeeName = employee.Key,
                        WeekStart = weekStart,
                        PresentDays = presentDays,
                        TotalPresences = presentDays.Count,
                        Message = GeneratePersonalizedMessage(employee.Key, presentDays, weekStart)
                    });
                }
            }

            return notifications;
        }

        private string GeneratePersonalizedMessage(string employeeName, List<string> presentDays, DateTime weekStart)
        {
            var weekEnd = weekStart.AddDays(4);
            var message = $"Bonjour {employeeName},\n\n";
            message += $"Voici votre planning de présence pour la semaine du {weekStart:dd/MM/yyyy} au {weekEnd:dd/MM/yyyy} :\n\n";

            foreach (var day in presentDays)
            {
                message += $"✅ {day}\n";
            }

            message += $"\nTotal : {presentDays.Count} jour(s) de présence\n";
            message += $"\nMerci de noter ces dates dans votre agenda.\n";
            message += $"En cas d'empêchement, merci de prévenir au plus tôt.\n\n";
            message += $"Bonne semaine !";

            return message;
        }

        private DateTime GetDateFromDayName(DateTime start, string day)
        {
            return day switch
            {
                "Lundi" => start,
                "Mardi" => start.AddDays(1),
                "Mercredi" => start.AddDays(2),
                "Jeudi" => start.AddDays(3),
                "Vendredi" => start.AddDays(4),
                _ => throw new ArgumentException($"Jour invalide: {day}")
            };
        }

        private static TypeConge ParseTypeConge(string? type)
        {
            return type?.ToLower() switch
            {
                "maladie" => TypeConge.CongeMaladie,
                "maternité" => TypeConge.CongeMaternite,
                "paternité" => TypeConge.CongePaternite,
                "personnel" => TypeConge.CongePersonnel,
                "formation" => TypeConge.Formation,
                _ => TypeConge.CongeAnnuel
            };
        }

        private string GetTypeDisplayName(TypeConge type)
        {
            return type switch
            {
                TypeConge.CongeAnnuel => "Congé annuel",
                TypeConge.CongeMaladie => "Congé maladie",
                TypeConge.CongeMaternite => "Congé maternité",
                TypeConge.CongePaternite => "Congé paternité",
                TypeConge.CongePersonnel => "Congé personnel",
                TypeConge.Formation => "Formation",
                _ => type.ToString()
            };
        }
    }

    // Classes de support
    public class CongeCsv
    {
        public string Nom { get; set; } = string.Empty;
        public string DateDebut { get; set; } = string.Empty;
        public string DateFin { get; set; } = string.Empty;
        public string? Raison { get; set; }
        public string? Type { get; set; }
    }

    public class PlanningResult
    {
        public Dictionary<string, Dictionary<string, PresenceInfo>> Planning { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public int TotalEmployes { get; set; }
        public int EmployesEnConge { get; set; }
    }

    public class PresenceInfo
    {
        public PresenceStatus Status { get; set; }
        public string Note { get; set; } = string.Empty;
    }

    public enum PresenceStatus
    {
        Present,
        Absent,
        Conge
    }

    public class PresenceNotification
    {
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime WeekStart { get; set; }
        public List<string> PresentDays { get; set; } = new();
        public int TotalPresences { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}