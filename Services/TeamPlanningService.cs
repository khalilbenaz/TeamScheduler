using Microsoft.EntityFrameworkCore;
using PlanningPresenceBlazor.Data;
using System.Text.Json;

namespace PlanningPresenceBlazor.Services
{
    public class TeamPlanningService
    {
        private readonly PlanningDbContext _db;
        private readonly ILogger<TeamPlanningService> _logger;
        private static readonly string[] WeekDays = { "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi" };

        public TeamPlanningService(PlanningDbContext db, ILogger<TeamPlanningService> logger)
        {
            _db = db;
            _logger = logger;
        }

        #region Gestion des équipes

        public async Task<List<Equipe>> GetAllEquipesAsync(bool includeMembers = false)
        {
            var query = _db.Equipes.Where(e => e.EstActive);

            if (includeMembers)
            {
                query = query.Include(e => e.Membres)
                             .Include(e => e.ChefEquipe);
            }

            return await query.OrderBy(e => e.Nom).ToListAsync();
        }

        public async Task<Equipe?> GetEquipeByIdAsync(int id, bool includeDetails = false)
        {
            var query = _db.Equipes.Where(e => e.Id == id);

            if (includeDetails)
            {
                query = query.Include(e => e.Membres)
                             .ThenInclude(m => m.Competences)
                             .ThenInclude(c => c.Competence)
                             .Include(e => e.ChefEquipe)
                             .Include(e => e.Affectations)
                             .ThenInclude(a => a.Client);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<EquipeStats> GetEquipeStatsAsync(int equipeId)
        {
            var equipe = await GetEquipeByIdAsync(equipeId, true);
            if (equipe == null) return new EquipeStats();

            var stats = new EquipeStats
            {
                NombreTotal = equipe.Membres.Count,
                NombreActifs = equipe.Membres.Count(m => m.EstActif),
                NombreEnConge = await _db.Conges
                    .Where(c => c.Employe != null && c.Employe.EquipeId == equipeId && c.EstActif)
                    .Select(c => c.EmployeId)
                    .Distinct()
                    .CountAsync(),
                NombreClients = equipe.Affectations.Count(a => a.EstActive),
                CompetencesUniques = equipe.Membres
                    .SelectMany(m => m.Competences)
                    .Select(c => c.CompetenceId)
                    .Distinct()
                    .Count()
            };

            return stats;
        }

        #endregion

        #region Gestion des clients

        public async Task<List<Client>> GetAllClientsAsync(bool includeAffectations = false)
        {
            var query = _db.Clients.Where(c => c.EstActif);

            if (includeAffectations)
            {
                query = query.Include(c => c.Affectations)
                             .ThenInclude(a => a.Equipe);
            }

            return await query.OrderBy(c => c.Nom).ToListAsync();
        }

        public async Task<ClientDetails?> GetClientDetailsAsync(int clientId)
        {
            var client = await _db.Clients
                .Include(c => c.Affectations)
                .ThenInclude(a => a.Equipe)
                .ThenInclude(e => e.Membres)
                .FirstOrDefaultAsync(c => c.Id == clientId);

            if (client == null) return null;

            var competencesRequises = await _db.ClientCompetencesRequises
                .Where(cr => cr.ClientId == clientId)
                .Include(cr => cr.Competence)
                .ToListAsync();

            return new ClientDetails
            {
                Client = client,
                CompetencesRequises = competencesRequises,
                EquipesAffectees = client.Affectations.Where(a => a.EstActive).Select(a => a.Equipe).ToList(),
                NombreEmployesTotal = client.Affectations
                    .Where(a => a.EstActive)
                    .SelectMany(a => a.Equipe.Membres)
                    .Count(m => m.EstActif)
            };
        }

        #endregion

        #region Génération de planning par équipe

        public async Task<TeamPlanningResult> GenerateTeamPlanningAsync(int equipeId, DateTime weekStart)
        {
            var equipe = await GetEquipeByIdAsync(equipeId, true);
            if (equipe == null)
            {
                throw new ArgumentException($"Équipe {equipeId} introuvable");
            }

            var result = new TeamPlanningResult
            {
                EquipeId = equipeId,
                EquipeNom = equipe.Nom,
                WeekStart = GetMondayOfWeek(weekStart),
                WeekEnd = GetMondayOfWeek(weekStart).AddDays(4)
            };

            // Récupérer la configuration
            var config = await GetActiveConfigurationAsync() ?? throw new InvalidOperationException("Aucune configuration active");
            
            // Appliquer les contraintes de l'équipe si définies
            var constraints = new PlanningConstraints
            {
                MinPresencesParPersonne = equipe.PresencesMinParPersonne > 0 ? equipe.PresencesMinParPersonne : config.PresencesMinParPersonne,
                MaxPresencesParPersonne = equipe.PresencesMaxParPersonne > 0 ? equipe.PresencesMaxParPersonne : config.PresencesMaxParPersonne,
                MinPresencesParJour = equipe.PresencesMinParJour > 0 ? equipe.PresencesMinParJour : config.PresencesMinParJour,
                MaxPresencesParJour = equipe.PresencesMaxParJour > 0 ? equipe.PresencesMaxParJour : config.PresencesMaxParJour,
                JoursCritiques = ParseJsonArray(equipe.JoursCritiques ?? config.JoursCritiques),
                MinPresencesJoursCritiques = equipe.PresencesMinJoursCritiques > 0 ? equipe.PresencesMinJoursCritiques : config.PresencesMinJoursCritiques
            };

            // Récupérer les membres actifs
            var membres = equipe.Membres.Where(m => m.EstActif).ToList();
            result.NombreMembres = membres.Count;

            // Récupérer les congés de la semaine
            var conges = await GetCongesForWeekAsync(weekStart, membres.Select(m => m.Id).ToList());
            result.NombreEnConge = conges.Select(c => c.EmployeId).Distinct().Count();

            // Récupérer les affectations clients actives
            var affectations = await GetActiveAffectationsAsync(equipeId, weekStart);

            // Initialiser le planning
            var planning = InitializePlanning(membres);

            // Marquer les congés
            MarkVacations(planning, conges, weekStart);

            // Générer le planning en tenant compte des contraintes et des affectations clients
            var warnings = new List<string>();
            var success = await GenerateOptimizedTeamPlanningAsync(
                planning, 
                membres, 
                affectations, 
                constraints, 
                weekStart, 
                warnings
            );

            if (!success)
            {
                warnings.Add("⚠️ Impossible de satisfaire toutes les contraintes");
            }

            // Valider le planning final
            ValidateTeamPlanning(planning, constraints, warnings);

            // Créer les affectations par client
            result.PlanningParClient = await CreateClientAssignmentsAsync(planning, affectations, weekStart);
            result.Planning = planning;
            result.Warnings = warnings;

            return result;
        }

        private async Task<bool> GenerateOptimizedTeamPlanningAsync(
            Dictionary<string, Dictionary<string, PresenceInfo>> planning,
            List<Employe> membres,
            List<AffectationEquipeClient> affectations,
            PlanningConstraints constraints,
            DateTime weekStart,
            List<string> warnings)
        {
            var random = new Random(GetWeekNumber(weekStart));

            // Phase 1: Affecter les jours critiques
            foreach (var day in constraints.JoursCritiques)
            {
                var disponibles = GetAvailableMembers(planning, membres, day);
                
                if (disponibles.Count < constraints.MinPresencesJoursCritiques)
                {
                    warnings.Add($"⚠️ {day}: Seulement {disponibles.Count} personne(s) disponible(s) sur {constraints.MinPresencesJoursCritiques} requises");
                    continue;
                }

                // Prioriser par compétences requises et rotation
                var prioritized = await PrioritizeMembersAsync(disponibles, affectations, weekStart, random);
                
                // Assigner le minimum requis
                for (int i = 0; i < Math.Min(constraints.MinPresencesJoursCritiques, prioritized.Count); i++)
                {
                    planning[prioritized[i].Nom][day] = new PresenceInfo
                    {
                        Status = PresenceStatus.Present,
                        Note = "Jour critique"
                    };
                }
            }

            // Phase 2: Compléter les présences pour atteindre le minimum par personne
            foreach (var membre in membres)
            {
                var currentPresences = GetCurrentPresences(planning, membre.Nom);
                var targetPresences = membre.PresencesMinSemaine > 0 ? membre.PresencesMinSemaine : constraints.MinPresencesParPersonne;

                while (currentPresences < targetPresences)
                {
                    var bestDay = FindBestDayForMember(planning, membre, constraints, affectations);
                    
                    if (bestDay == null)
                    {
                        warnings.Add($"ℹ️ {membre.Nom}: seulement {currentPresences} présence(s) sur {targetPresences} requises");
                        break;
                    }

                    planning[membre.Nom][bestDay] = new PresenceInfo
                    {
                        Status = PresenceStatus.Present,
                        Note = "Complément"
                    };
                    currentPresences++;
                }
            }

            // Phase 3: Optimiser pour les affectations clients
            await OptimizeForClientAssignmentsAsync(planning, membres, affectations, constraints);

            return true;
        }

        private async Task<List<Employe>> PrioritizeMembersAsync(
            List<Employe> membres,
            List<AffectationEquipeClient> affectations,
            DateTime weekStart,
            Random random)
        {
            var scoredMembers = new List<(Employe membre, int score)>();

            foreach (var membre in membres)
            {
                int score = 0;

                // Bonus pour les compétences requises par les clients
                foreach (var affectation in affectations)
                {
                    var competencesRequises = await _db.ClientCompetencesRequises
                        .Where(cr => cr.ClientId == affectation.ClientId)
                        .ToListAsync();

                    var competencesMembre = membre.Competences.Select(c => c.CompetenceId).ToHashSet();
                    score += competencesRequises.Count(cr => competencesMembre.Contains(cr.CompetenceId)) * 10;
                }

                // Bonus pour niveau de compétence
                score += membre.NiveauCompetence * 5;

                // Ajouter un facteur aléatoire pour la rotation
                score += random.Next(0, 20);

                scoredMembers.Add((membre, score));
            }

            return scoredMembers.OrderByDescending(x => x.score).Select(x => x.membre).ToList();
        }

        #endregion

        #region Planning multi-équipes/clients

        public async Task<GlobalPlanningResult> GenerateGlobalPlanningAsync(DateTime weekStart)
        {
            var result = new GlobalPlanningResult
            {
                WeekStart = GetMondayOfWeek(weekStart),
                WeekEnd = GetMondayOfWeek(weekStart).AddDays(4)
            };

            // Récupérer toutes les équipes actives
            var equipes = await GetAllEquipesAsync();

            // Générer le planning pour chaque équipe
            foreach (var equipe in equipes)
            {
                try
                {
                    var teamPlanning = await GenerateTeamPlanningAsync(equipe.Id, weekStart);
                    result.TeamPlannings.Add(teamPlanning);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur génération planning équipe {EquipeId}", equipe.Id);
                    result.Errors.Add($"Erreur équipe {equipe.Nom}: {ex.Message}");
                }
            }

            // Calculer les statistiques globales
            result.TotalEmployes = result.TeamPlannings.Sum(tp => tp.NombreMembres);
            result.TotalPresences = result.TeamPlannings.Sum(tp => 
                tp.Planning.Values.Sum(days => days.Count(d => d.Value.Status == PresenceStatus.Present)));
            result.TotalEnConge = result.TeamPlannings.Sum(tp => tp.NombreEnConge);

            return result;
        }

        public async Task<List<ClientPlanningView>> GetClientPlanningViewAsync(DateTime weekStart)
        {
            var views = new List<ClientPlanningView>();
            var monday = GetMondayOfWeek(weekStart);

            // Récupérer tous les clients avec affectations actives
            var clients = await _db.Clients
                .Include(c => c.Affectations)
                .ThenInclude(a => a.Equipe)
                .ThenInclude(e => e.Membres)
                .Where(c => c.EstActif && c.Affectations.Any(a => a.EstActive))
                .ToListAsync();

            foreach (var client in clients)
            {
                var view = new ClientPlanningView
                {
                    ClientId = client.Id,
                    ClientNom = client.Nom,
                    WeekStart = monday
                };

                // Pour chaque jour de la semaine
                foreach (var day in WeekDays)
                {
                    var dayDate = GetDateFromDayName(monday, day);
                    var employesDuJour = new List<EmployePresence>();

                    // Pour chaque affectation active du client
                    foreach (var affectation in client.Affectations.Where(a => a.EstActive))
                    {
                        // Récupérer le planning de l'équipe pour cette semaine
                        var planning = await _db.PlanningsEquipeClient
                            .FirstOrDefaultAsync(p => 
                                p.AffectationId == affectation.Id && 
                                p.DatePlanning == dayDate);

                        if (planning != null && !string.IsNullOrEmpty(planning.EmployesAssignes))
                        {
                            var employeIds = JsonSerializer.Deserialize<List<int>>(planning.EmployesAssignes) ?? new List<int>();
                            var employes = await _db.Employes
                                .Where(e => employeIds.Contains(e.Id))
                                .Select(e => new EmployePresence
                                {
                                    Nom = e.Nom,
                                    Equipe = e.Equipe!.Nom,
                                    Competences = e.Competences.Select(c => c.Competence.Nom).ToList()
                                })
                                .ToListAsync();

                            employesDuJour.AddRange(employes);
                        }
                    }

                    view.PresencesParJour[day] = employesDuJour;
                }

                views.Add(view);
            }

            return views;
        }

        #endregion

        #region Configuration

        public async Task<ConfigurationPlanning?> GetActiveConfigurationAsync()
        {
            return await _db.ConfigurationsPlanning
                .FirstOrDefaultAsync(c => c.EstActive);
        }

        public async Task<bool> UpdateConfigurationAsync(ConfigurationPlanning config)
        {
            try
            {
                config.DateModification = DateTime.Now;
                _db.ConfigurationsPlanning.Update(config);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur mise à jour configuration");
                return false;
            }
        }

        #endregion

        #region Méthodes utilitaires

        private Dictionary<string, Dictionary<string, PresenceInfo>> InitializePlanning(List<Employe> membres)
        {
            var planning = new Dictionary<string, Dictionary<string, PresenceInfo>>();

            foreach (var membre in membres)
            {
                planning[membre.Nom] = WeekDays.ToDictionary(
                    day => day,
                    _ => new PresenceInfo { Status = PresenceStatus.Absent, Note = "" }
                );
            }

            return planning;
        }

        private void MarkVacations(
            Dictionary<string, Dictionary<string, PresenceInfo>> planning,
            List<Conge> conges,
            DateTime weekStart)
        {
            foreach (var day in WeekDays)
            {
                var currentDate = GetDateFromDayName(weekStart, day);
                var congesDuJour = conges.Where(c => currentDate >= c.DateDebut && currentDate <= c.DateFin);

                foreach (var conge in congesDuJour)
                {
                    if (conge.Employe != null && planning.ContainsKey(conge.Employe.Nom))
                    {
                        planning[conge.Employe.Nom][day] = new PresenceInfo
                        {
                            Status = PresenceStatus.Conge,
                            Note = GetTypeCongeDisplayName(conge.Type)
                        };
                    }
                }
            }
        }

        private List<Employe> GetAvailableMembers(
            Dictionary<string, Dictionary<string, PresenceInfo>> planning,
            List<Employe> membres,
            string day)
        {
            return membres.Where(m => 
                planning.ContainsKey(m.Nom) && 
                planning[m.Nom][day].Status == PresenceStatus.Absent
            ).ToList();
        }

        private int GetCurrentPresences(Dictionary<string, Dictionary<string, PresenceInfo>> planning, string membre)
        {
            return planning.ContainsKey(membre) 
                ? planning[membre].Count(kvp => kvp.Value.Status == PresenceStatus.Present)
                : 0;
        }

        private string? FindBestDayForMember(
            Dictionary<string, Dictionary<string, PresenceInfo>> planning,
            Employe membre,
            PlanningConstraints constraints,
            List<AffectationEquipeClient> affectations)
        {
            var candidateDays = WeekDays
                .Where(day => planning[membre.Nom][day].Status == PresenceStatus.Absent)
                .ToList();

            if (!candidateDays.Any()) return null;

            // Prioriser les jours où le membre est nécessaire pour un client
            var scoredDays = new List<(string day, int score)>();

            foreach (var day in candidateDays)
            {
                int score = 0;

                // Vérifier si ce jour est dans les affectations clients
                foreach (var affectation in affectations)
                {
                    var joursPresence = ParseJsonArray(affectation.JoursPresence ?? "[]");
                    if (joursPresence.Contains(day))
                    {
                        score += 10;
                    }
                }

                // Vérifier les contraintes de présences par jour
                var currentDayPresences = planning.Values.Count(p => p[day].Status == PresenceStatus.Present);
                if (currentDayPresences < constraints.MinPresencesParJour)
                {
                    score += 5;
                }

                scoredDays.Add((day, score));
            }

            return scoredDays.OrderByDescending(x => x.score).FirstOrDefault().day;
        }

        private async Task<List<AffectationEquipeClient>> GetActiveAffectationsAsync(int equipeId, DateTime date)
        {
            return await _db.AffectationsEquipeClient
                .Include(a => a.Client)
                .Where(a => a.EquipeId == equipeId && 
                           a.EstActive && 
                           a.DateDebut <= date && 
                           (a.DateFin == null || a.DateFin >= date))
                .ToListAsync();
        }

        private async Task<List<Conge>> GetCongesForWeekAsync(DateTime weekStart, List<int> employeIds)
        {
            var weekEnd = weekStart.AddDays(4);
            
            return await _db.Conges
                .Include(c => c.Employe)
                .Where(c => c.EmployeId != null && 
                           employeIds.Contains(c.EmployeId.Value) &&
                           c.Status == StatusConge.Approuve &&
                           c.DateDebut <= weekEnd && 
                           c.DateFin >= weekStart)
                .ToListAsync();
        }

        private async Task OptimizeForClientAssignmentsAsync(
            Dictionary<string, Dictionary<string, PresenceInfo>> planning,
            List<Employe> membres,
            List<AffectationEquipeClient> affectations,
            PlanningConstraints constraints)
        {
            foreach (var affectation in affectations)
            {
                var joursClient = ParseJsonArray(affectation.JoursPresence ?? "[]");
                
                foreach (var jour in joursClient)
                {
                    var presentsJour = membres.Where(m => 
                        planning.ContainsKey(m.Nom) && 
                        planning[m.Nom][jour].Status == PresenceStatus.Present
                    ).Count();

                    // S'assurer du minimum requis pour le client
                    while (presentsJour < affectation.NombreMinPersonnes)
                    {
                        var candidat = membres
                            .Where(m => planning[m.Nom][jour].Status == PresenceStatus.Absent)
                            .OrderBy(m => GetCurrentPresences(planning, m.Nom))
                            .FirstOrDefault();

                        if (candidat == null) break;

                        planning[candidat.Nom][jour] = new PresenceInfo
                        {
                            Status = PresenceStatus.Present,
                            Note = $"Client: {affectation.Client.Nom}"
                        };
                        presentsJour++;
                    }
                }
            }
        }

        private async Task<Dictionary<string, List<EmployeAffectation>>> CreateClientAssignmentsAsync(
            Dictionary<string, Dictionary<string, PresenceInfo>> planning,
            List<AffectationEquipeClient> affectations,
            DateTime weekStart)
        {
            var result = new Dictionary<string, List<EmployeAffectation>>();

            foreach (var affectation in affectations)
            {
                var assignments = new List<EmployeAffectation>();

                foreach (var day in WeekDays)
                {
                    var dayDate = GetDateFromDayName(weekStart, day);
                    var employesPresents = planning
                        .Where(p => p.Value[day].Status == PresenceStatus.Present)
                        .Select(p => p.Key)
                        .ToList();

                    if (employesPresents.Any())
                    {
                        // Créer ou mettre à jour le planning dans la base
                        var planningDb = await _db.PlanningsEquipeClient
                            .FirstOrDefaultAsync(p => 
                                p.AffectationId == affectation.Id && 
                                p.DatePlanning == dayDate);

                        if (planningDb == null)
                        {
                            planningDb = new PlanningEquipeClient
                            {
                                AffectationId = affectation.Id,
                                DatePlanning = dayDate,
                                SemaineNumero = GetWeekNumber(weekStart),
                                Annee = weekStart.Year
                            };
                            _db.PlanningsEquipeClient.Add(planningDb);
                        }

                        var employeIds = await _db.Employes
                            .Where(e => employesPresents.Contains(e.Nom))
                            .Select(e => e.Id)
                            .ToListAsync();

                        planningDb.EmployesAssignes = JsonSerializer.Serialize(employeIds);
                        planningDb.NombrePresencesPrevues = employeIds.Count;
                        planningDb.Status = StatusPlanning.EnCours;

                        assignments.Add(new EmployeAffectation
                        {
                            Jour = day,
                            Employes = employesPresents,
                            NombreTotal = employesPresents.Count
                        });
                    }
                }

                result[affectation.Client.Nom] = assignments;
            }

            await _db.SaveChangesAsync();
            return result;
        }

        private void ValidateTeamPlanning(
            Dictionary<string, Dictionary<string, PresenceInfo>> planning,
            PlanningConstraints constraints,
            List<string> warnings)
        {
            // Vérifier les présences par personne
            foreach (var membre in planning.Keys)
            {
                var presences = planning[membre].Count(kvp => kvp.Value.Status == PresenceStatus.Present);
                var conges = planning[membre].Count(kvp => kvp.Value.Status == PresenceStatus.Conge);

                if (presences < constraints.MinPresencesParPersonne && conges < 3)
                {
                    warnings.Add($"⚠️ {membre}: {presences} présence(s) < {constraints.MinPresencesParPersonne} minimum");
                }
                else if (presences > constraints.MaxPresencesParPersonne)
                {
                    warnings.Add($"⚠️ {membre}: {presences} présence(s) > {constraints.MaxPresencesParPersonne} maximum");
                }
            }

            // Vérifier les jours critiques
            foreach (var jour in constraints.JoursCritiques)
            {
                var presences = planning.Values.Count(p => p[jour].Status == PresenceStatus.Present);
                if (presences < constraints.MinPresencesJoursCritiques)
                {
                    warnings.Add($"⚠️ {jour}: {presences} présence(s) < {constraints.MinPresencesJoursCritiques} minimum requis");
                }
            }
        }

        private List<string> ParseJsonArray(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private string GetTypeCongeDisplayName(TypeConge type)
        {
            return type switch
            {
                TypeConge.CongeAnnuel => "Congé annuel",
                TypeConge.CongeMaladie => "Congé maladie",
                TypeConge.CongeMaternite => "Congé maternité",
                TypeConge.CongePaternite => "Congé paternité",
                TypeConge.CongePersonnel => "Congé personnel",
                TypeConge.Formation => "Formation",
                TypeConge.RTT => "RTT",
                TypeConge.CongeExceptionnel => "Congé exceptionnel",
                _ => type.ToString()
            };
        }

        private DateTime GetMondayOfWeek(DateTime date)
        {
            int daysFromMonday = ((int)date.DayOfWeek - 1 + 7) % 7;
            return date.AddDays(-daysFromMonday);
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

        private int GetWeekNumber(DateTime date)
        {
            var jan1 = new DateTime(date.Year, 1, 1);
            var cal = System.Globalization.CultureInfo.CurrentCulture.Calendar;
            return cal.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        #endregion
    }

    #region DTOs et modèles de résultat

    public class TeamPlanningResult
    {
        public int EquipeId { get; set; }
        public string EquipeNom { get; set; } = string.Empty;
        public Dictionary<string, Dictionary<string, PresenceInfo>> Planning { get; set; } = new();
        public Dictionary<string, List<EmployeAffectation>> PlanningParClient { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public int NombreMembres { get; set; }
        public int NombreEnConge { get; set; }
    }

    public class GlobalPlanningResult
    {
        public List<TeamPlanningResult> TeamPlannings { get; set; } = new();
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public int TotalEmployes { get; set; }
        public int TotalPresences { get; set; }
        public int TotalEnConge { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class ClientPlanningView
    {
        public int ClientId { get; set; }
        public string ClientNom { get; set; } = string.Empty;
        public DateTime WeekStart { get; set; }
        public Dictionary<string, List<EmployePresence>> PresencesParJour { get; set; } = new();
    }

    public class EmployePresence
    {
        public string Nom { get; set; } = string.Empty;
        public string Equipe { get; set; } = string.Empty;
        public List<string> Competences { get; set; } = new();
    }

    public class EmployeAffectation
    {
        public string Jour { get; set; } = string.Empty;
        public List<string> Employes { get; set; } = new();
        public int NombreTotal { get; set; }
    }

    public class EquipeStats
    {
        public int NombreTotal { get; set; }
        public int NombreActifs { get; set; }
        public int NombreEnConge { get; set; }
        public int NombreClients { get; set; }
        public int CompetencesUniques { get; set; }
    }

    public class ClientDetails
    {
        public Client Client { get; set; } = null!;
        public List<ClientCompetenceRequise> CompetencesRequises { get; set; } = new();
        public List<Equipe> EquipesAffectees { get; set; } = new();
        public int NombreEmployesTotal { get; set; }
    }

    public class PlanningConstraints
    {
        public int MinPresencesParPersonne { get; set; }
        public int MaxPresencesParPersonne { get; set; }
        public int MinPresencesParJour { get; set; }
        public int MaxPresencesParJour { get; set; }
        public List<string> JoursCritiques { get; set; } = new();
        public int MinPresencesJoursCritiques { get; set; }
    }

    #endregion
}