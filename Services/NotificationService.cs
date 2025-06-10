using System.Net.Mail;
using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PlanningPresenceBlazor.Data;

namespace PlanningPresenceBlazor.Services
{
    public class NotificationService
    {
        private readonly PlanningDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(PlanningDbContext db, IConfiguration configuration, ILogger<NotificationService> logger)
        {
            _db = db;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<NotificationResult> SendPlanningNotificationsAsync(
            List<PresenceNotification> notifications,
            NotificationMethod method,
            bool testMode = false)
        {
            var result = new NotificationResult();
            var employees = await _db.Employes.Where(e => e.EstActif).ToListAsync();

            _logger.LogInformation("Début envoi de {Count} notifications via {Method} (Mode test: {TestMode})",
                notifications.Count, method, testMode);

            foreach (var notification in notifications)
            {
                var employee = employees.FirstOrDefault(e => e.Nom == notification.EmployeeName);
                if (employee == null)
                {
                    result.Failed.Add($"Employé {notification.EmployeeName} non trouvé");
                    continue;
                }

                // Vérifier les préférences de notification de l'employé
                bool canSend = method switch
                {
                    NotificationMethod.Email => employee.NotificationEmail && !string.IsNullOrEmpty(employee.Email),
                    NotificationMethod.SMS => employee.NotificationSMS && !string.IsNullOrEmpty(employee.Telephone),
                    NotificationMethod.Teams => employee.NotificationTeams && !string.IsNullOrEmpty(employee.TeamsId),
                    _ => false
                };

                if (!canSend)
                {
                    result.Failed.Add($"{employee.Nom} - {method} non configuré ou désactivé");
                    continue;
                }

                try
                {
                    bool success = method switch
                    {
                        NotificationMethod.Email => await SendEmailNotificationAsync(employee, notification, testMode),
                        NotificationMethod.SMS => await SendSMSNotificationAsync(employee, notification, testMode),
                        NotificationMethod.Teams => await SendTeamsNotificationAsync(employee, notification, testMode),
                        _ => false
                    };

                    if (success)
                    {
                        result.Successful.Add($"{employee.Nom} - {method}");
                        _logger.LogInformation("Notification {Method} envoyée avec succès à {Employee}", method, employee.Nom);
                    }
                    else
                    {
                        result.Failed.Add($"{employee.Nom} - Échec {method}");
                        _logger.LogWarning("Échec envoi {Method} à {Employee}", method, employee.Nom);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur envoi notification {Method} pour {Employee}", method, employee.Nom);
                    result.Failed.Add($"{employee.Nom} - Erreur: {ex.Message}");
                }
            }

            _logger.LogInformation("Fin envoi notifications: {Successful} succès, {Failed} échecs",
                result.TotalSent, result.TotalFailed);

            return result;
        }

        private async Task<bool> SendEmailNotificationAsync(Employe employee, PresenceNotification notification, bool testMode)
        {
            if (string.IsNullOrEmpty(employee.Email))
            {
                _logger.LogWarning("Pas d'email pour {Employee}", employee.Nom);
                return false;
            }

            if (testMode)
            {
                _logger.LogInformation("MODE TEST - Email à {Email}: Planning semaine {Week}",
                    employee.Email, notification.WeekStart.ToString("dd/MM"));
                await Task.Delay(500); // Simule le délai d'envoi
                return true;
            }

            try
            {
                var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUser = _configuration["Email:Username"] ?? "";
                var smtpPass = _configuration["Email:Password"] ?? "";
                var fromEmail = _configuration["Email:FromAddress"] ?? "planning@company.com";
                var fromName = _configuration["Email:FromName"] ?? "Planning Présence";

                if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
                {
                    _logger.LogWarning("Configuration email incomplète");
                    return false;
                }

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(smtpUser, smtpPass)
                };

                var message = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = $"🗓️ Planning semaine du {notification.WeekStart:dd/MM/yyyy}",
                    Body = GenerateEmailBody(notification),
                    IsBodyHtml = true
                };

                message.To.Add(employee.Email);

                await client.SendMailAsync(message);
                _logger.LogInformation("Email envoyé à {Employee} ({Email})", employee.Nom, employee.Email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur envoi email à {Employee}", employee.Nom);
                return false;
            }
        }

        private async Task<bool> SendSMSNotificationAsync(Employe employee, PresenceNotification notification, bool testMode)
        {
            if (string.IsNullOrEmpty(employee.Telephone))
            {
                _logger.LogWarning("Pas de téléphone pour {Employee}", employee.Nom);
                return false;
            }

            if (testMode)
            {
                _logger.LogInformation("MODE TEST - SMS à {Phone}: Planning {Week}",
                    employee.Telephone, notification.WeekStart.ToString("dd/MM"));
                await Task.Delay(300); // Simule le délai d'envoi
                return true;
            }

            try
            {
                var smsApiKey = _configuration["SMS:ApiKey"];
                var smsApiUrl = _configuration["SMS:ApiUrl"];
                var fromNumber = _configuration["SMS:FromNumber"];

                if (string.IsNullOrEmpty(smsApiKey) || string.IsNullOrEmpty(smsApiUrl))
                {
                    _logger.LogWarning("Configuration SMS manquante");
                    return false;
                }

                var message = GenerateSMSMessage(notification);

                using var httpClient = new HttpClient();
                var requestData = new
                {
                    to = employee.Telephone,
                    message = message,
                    from = fromNumber ?? "Planning"
                };

                var jsonContent = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {smsApiKey}");

                var response = await httpClient.PostAsync(smsApiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("SMS envoyé à {Employee} ({Phone})", employee.Nom, employee.Telephone);
                    return true;
                }
                else
                {
                    _logger.LogError("Échec envoi SMS à {Employee}: {Status} - {Content}",
                        employee.Nom, response.StatusCode, await response.Content.ReadAsStringAsync());
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur envoi SMS à {Employee}", employee.Nom);
                return false;
            }
        }

        private async Task<bool> SendTeamsNotificationAsync(Employe employee, PresenceNotification notification, bool testMode)
        {
            if (string.IsNullOrEmpty(employee.TeamsId))
            {
                _logger.LogWarning("Pas d'ID Teams pour {Employee}", employee.Nom);
                return false;
            }

            if (testMode)
            {
                _logger.LogInformation("MODE TEST - Teams à {TeamsId}: Planning {Week}",
                    employee.TeamsId, notification.WeekStart.ToString("dd/MM"));
                await Task.Delay(400); // Simule le délai d'envoi
                return true;
            }

            try
            {
                var teamsWebhook = _configuration["Teams:WebhookUrl"];
                var tenantId = _configuration["Teams:TenantId"];
                var clientId = _configuration["Teams:ClientId"];
                var clientSecret = _configuration["Teams:ClientSecret"];

                if (string.IsNullOrEmpty(teamsWebhook))
                {
                    _logger.LogWarning("Configuration Teams manquante");
                    return false;
                }

                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);

                var teamsMessage = GenerateTeamsMessage(notification);
                var jsonContent = JsonSerializer.Serialize(teamsMessage);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(teamsWebhook, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Message Teams envoyé à {Employee}", employee.Nom);
                    return true;
                }
                else
                {
                    _logger.LogError("Échec envoi Teams à {Employee}: {Status} - {Content}",
                        employee.Nom, response.StatusCode, await response.Content.ReadAsStringAsync());
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur envoi Teams à {Employee}", employee.Nom);
                return false;
            }
        }

        private string GenerateEmailBody(PresenceNotification notification)
        {
            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <style>
        body {{ 
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
            color: #333; 
            line-height: 1.6;
            margin: 0;
            padding: 0;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background: #ffffff;
        }}
        .header {{ 
            background: linear-gradient(135deg, #2563eb, #1e40af); 
            color: white; 
            padding: 30px 20px; 
            text-align: center; 
            border-radius: 10px 10px 0 0;
        }}
        .header h1 {{
            margin: 0;
            font-size: 24px;
            font-weight: 600;
        }}
        .header p {{
            margin: 10px 0 0 0;
            font-size: 16px;
            opacity: 0.9;
        }}
        .content {{ 
            padding: 30px 20px; 
        }}
        .greeting {{
            font-size: 18px;
            margin-bottom: 20px;
            color: #2563eb;
            font-weight: 600;
        }}
        .day-item {{ 
            background: #f8f9fa; 
            margin: 8px 0; 
            padding: 15px; 
            border-left: 4px solid #28a745; 
            border-radius: 5px;
            display: flex;
            align-items: center;
        }}
        .day-item .icon {{
            color: #28a745;
            margin-right: 10px;
            font-weight: bold;
        }}
        .summary {{ 
            background: linear-gradient(135deg, #e7f3ff, #cce7ff); 
            padding: 20px; 
            margin: 25px 0; 
            border-radius: 8px; 
            border: 1px solid #b3d9ff;
        }}
        .summary-title {{
            font-weight: 600;
            color: #1e40af;
            margin-bottom: 10px;
        }}
        .important {{ 
            background: #fff3cd; 
            padding: 20px; 
            margin: 20px 0; 
            border-radius: 8px; 
            border: 1px solid #ffeaa7;
        }}
        .important h4 {{
            color: #856404;
            margin: 0 0 15px 0;
        }}
        .important ul {{
            margin: 0;
            padding-left: 20px;
        }}
        .important li {{
            margin: 5px 0;
            color: #856404;
        }}
        .footer {{ 
            background: #f8f9fa; 
            padding: 20px; 
            text-align: center; 
            font-size: 14px; 
            color: #666; 
            border-radius: 0 0 10px 10px;
        }}
        .footer p {{
            margin: 5px 0;
        }}
        .btn {{
            display: inline-block;
            padding: 12px 24px;
            background: #2563eb;
            color: white;
            text-decoration: none;
            border-radius: 6px;
            font-weight: 600;
            margin: 10px 5px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>📅 Planning de Présence</h1>
            <p>Semaine du {notification.WeekStart:dd/MM/yyyy} au {notification.WeekStart.AddDays(4):dd/MM/yyyy}</p>
        </div>
        
        <div class='content'>
            <div class='greeting'>Bonjour {notification.EmployeeName} ! 👋</div>
            
            <p>Voici votre planning de présence pour la semaine :</p>
            
            <div class='summary'>
                <div class='summary-title'>📊 Résumé de votre semaine</div>
                <strong>{notification.TotalPresences} jour(s) de présence</strong> planifiés
            </div>";

            foreach (var day in notification.PresentDays)
            {
                html += $"<div class='day-item'><span class='icon'>✅</span> <strong>{day}</strong></div>";
            }

            html += $@"
            <div class='important'>
                <h4>📌 Important</h4>
                <ul>
                    <li>Merci de noter ces dates dans votre agenda personnel</li>
                    <li>En cas d'empêchement, prévenez votre responsable au plus tôt</li>
                    <li>Consultez l'application planning pour plus de détails</li>
                    <li>Ce planning peut être modifié selon les besoins du service</li>
                </ul>
            </div>
            
            <p style='text-align: center; margin: 30px 0;'>
                <a href='#' class='btn'>📱 Accéder au planning complet</a>
            </p>
            
            <p style='color: #666; font-style: italic;'>
                Nous vous souhaitons une excellente semaine ! 🌟
            </p>
        </div>
        
        <div class='footer'>
            <p><strong>📧 Message automatique - Planning Présence</strong></p>
            <p>Ne pas répondre à cet email | Généré le {DateTime.Now:dd/MM/yyyy à HH:mm}</p>
        </div>
    </div>
</body>
</html>";

            return html;
        }

        private string GenerateSMSMessage(PresenceNotification notification)
        {
            var message = $"🗓️ Planning {notification.WeekStart:dd/MM}: ";

            // Raccourcir les jours pour SMS
            var shortDays = notification.PresentDays
                .Select(d => d.Split(' ')[0].Substring(0, 2)) // Lu, Ma, Me, Je, Ve
                .ToList();

            message += string.Join(", ", shortDays);
            message += $" ({notification.TotalPresences}j)";
            message += " 📅 Notez dans votre agenda. Bonne semaine !";

            return message;
        }

        private object GenerateTeamsMessage(PresenceNotification notification)
        {
            return new
            {
                type = "message",
                attachments = new[]
                {
                    new
                    {
                        contentType = "application/vnd.microsoft.card.adaptive",
                        contentUrl = null as string,
                        content = new
                        {
                            type = "AdaptiveCard",
                            schema = "http://adaptivecards.io/schemas/adaptive-card.json",
                            version = "1.4",
                            body = new object[]
                            {
                                new
                                {
                                    type = "Container",
                                    style = "emphasis",
                                    items = new object[]
                                    {
                                        new
                                        {
                                            type = "ColumnSet",
                                            columns = new object[]
                                            {
                                                new
                                                {
                                                    type = "Column",
                                                    items = new object[]
                                                    {
                                                        new
                                                        {
                                                            type = "Image",
                                                            style = "person",
                                                            url = "https://cdn-icons-png.flaticon.com/512/3135/3135715.png",
                                                            size = "small"
                                                        }
                                                    },
                                                    width = "auto"
                                                },
                                                new
                                                {
                                                    type = "Column",
                                                    items = new object[]
                                                    {
                                                        new
                                                        {
                                                            type = "TextBlock",
                                                            text = "📅 Planning de Présence",
                                                            weight = "bolder",
                                                            size = "medium"
                                                        },
                                                        new
                                                        {
                                                            type = "TextBlock",
                                                            text = $"Semaine du {notification.WeekStart:dd/MM/yyyy}",
                                                            spacing = "none",
                                                            isSubtle = true
                                                        }
                                                    },
                                                    width = "stretch"
                                                }
                                            }
                                        }
                                    }
                                },
                                new
                                {
                                    type = "Container",
                                    items = new object[]
                                    {
                                        new
                                        {
                                            type = "TextBlock",
                                            text = $"Bonjour **{notification.EmployeeName}** ! 👋",
                                            wrap = true,
                                            size = "medium"
                                        },
                                        new
                                        {
                                            type = "TextBlock",
                                            text = "Voici votre planning de présence pour cette semaine :",
                                            wrap = true,
                                            spacing = "medium"
                                        }
                                    }
                                },
                                new
                                {
                                    type = "Container",
                                    style = "good",
                                    items = new object[]
                                    {
                                        new
                                        {
                                            type = "FactSet",
                                            facts = notification.PresentDays.Select(day => new
                                            {
                                                title = "✅ Présent",
                                                value = day
                                            }).ToArray()
                                        }
                                    }
                                },
                                new
                                {
                                    type = "Container",
                                    items = new object[]
                                    {
                                        new
                                        {
                                            type = "ColumnSet",
                                            columns = new object[]
                                            {
                                                new
                                                {
                                                    type = "Column",
                                                    items = new object[]
                                                    {
                                                        new
                                                        {
                                                            type = "TextBlock",
                                                            text = "📊 **Total**",
                                                            weight = "bolder"
                                                        }
                                                    },
                                                    width = "stretch"
                                                },
                                                new
                                                {
                                                    type = "Column",
                                                    items = new object[]
                                                    {
                                                        new
                                                        {
                                                            type = "TextBlock",
                                                            text = $"**{notification.TotalPresences} jour(s)**",
                                                            color = "good",
                                                            horizontalAlignment = "right"
                                                        }
                                                    },
                                                    width = "auto"
                                                }
                                            }
                                        }
                                    }
                                },
                                new
                                {
                                    type = "Container",
                                    style = "warning",
                                    items = new object[]
                                    {
                                        new
                                        {
                                            type = "TextBlock",
                                            text = "📌 **N'oubliez pas :**\n• Notez ces dates dans votre agenda\n• Prévenez en cas d'empêchement\n• Consultez l'app pour les détails",
                                            wrap = true,
                                            size = "small"
                                        }
                                    }
                                }
                            },
                            actions = new object[]
                            {
                                new
                                {
                                    type = "Action.OpenUrl",
                                    title = "📱 Voir le planning complet",
                                    url = "https://planning.company.com" // À adapter selon votre URL
                                }
                            }
                        }
                    }
                }
            };
        }

        public async Task<List<NotificationPreview>> GenerateNotificationPreviews(List<PresenceNotification> notifications)
        {
            var previews = new List<NotificationPreview>();
            var employees = await _db.Employes.Where(e => e.EstActif).ToListAsync();

            foreach (var notification in notifications)
            {
                var employee = employees.FirstOrDefault(e => e.Nom == notification.EmployeeName);
                if (employee != null)
                {
                    previews.Add(new NotificationPreview
                    {
                        EmployeeName = employee.Nom,
                        Email = employee.Email ?? "Non configuré",
                        Telephone = employee.Telephone ?? "Non configuré",
                        TeamsId = !string.IsNullOrEmpty(employee.TeamsId) ?
                            employee.TeamsId.Length > 20 ? employee.TeamsId.Substring(0, 20) + "..." : employee.TeamsId
                            : "Non configuré",
                        MessagePreview = notification.Message,
                        PresentDays = notification.PresentDays,
                        HasEmail = !string.IsNullOrEmpty(employee.Email) && employee.NotificationEmail,
                        HasPhone = !string.IsNullOrEmpty(employee.Telephone) && employee.NotificationSMS,
                        HasTeams = !string.IsNullOrEmpty(employee.TeamsId) && employee.NotificationTeams
                    });
                }
            }

            return previews;
        }

        // Méthode pour tester la configuration des notifications
        public async Task<NotificationTestResult> TestNotificationConfigurationAsync()
        {
            var result = new NotificationTestResult();

            // Test configuration email
            var emailConfig = new
            {
                Host = _configuration["Email:SmtpHost"],
                Port = _configuration["Email:SmtpPort"],
                Username = _configuration["Email:Username"],
                Password = !string.IsNullOrEmpty(_configuration["Email:Password"]) ? "***" : null,
                FromAddress = _configuration["Email:FromAddress"]
            };

            result.EmailConfigured = !string.IsNullOrEmpty(emailConfig.Host) &&
                                   !string.IsNullOrEmpty(emailConfig.Username) &&
                                   !string.IsNullOrEmpty(_configuration["Email:Password"]);

            // Test configuration SMS
            result.SMSConfigured = !string.IsNullOrEmpty(_configuration["SMS:ApiKey"]) &&
                                 !string.IsNullOrEmpty(_configuration["SMS:ApiUrl"]);

            // Test configuration Teams
            result.TeamsConfigured = !string.IsNullOrEmpty(_configuration["Teams:WebhookUrl"]);

            // Compter les employés avec chaque type de notification
            var employees = await _db.Employes.Where(e => e.EstActif).ToListAsync();

            result.EmployeesWithEmail = employees.Count(e => e.NotificationEmail && !string.IsNullOrEmpty(e.Email));
            result.EmployeesWithSMS = employees.Count(e => e.NotificationSMS && !string.IsNullOrEmpty(e.Telephone));
            result.EmployeesWithTeams = employees.Count(e => e.NotificationTeams && !string.IsNullOrEmpty(e.TeamsId));
            result.TotalEmployees = employees.Count;

            return result;
        }
    }

    public enum NotificationMethod
    {
        Email,
        SMS,
        Teams
    }

    public class NotificationResult
    {
        public List<string> Successful { get; set; } = new();
        public List<string> Failed { get; set; } = new();
        public int TotalSent => Successful.Count;
        public int TotalFailed => Failed.Count;
        public bool HasErrors => Failed.Any();
        public double SuccessRate => TotalSent + TotalFailed > 0 ? (double)TotalSent / (TotalSent + TotalFailed) * 100 : 0;
    }

    public class NotificationPreview
    {
        public string EmployeeName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telephone { get; set; } = string.Empty;
        public string TeamsId { get; set; } = string.Empty;
        public string MessagePreview { get; set; } = string.Empty;
        public List<string> PresentDays { get; set; } = new();
        public bool HasEmail { get; set; }
        public bool HasPhone { get; set; }
        public bool HasTeams { get; set; }
    }

    public class NotificationTestResult
    {
        public bool EmailConfigured { get; set; }
        public bool SMSConfigured { get; set; }
        public bool TeamsConfigured { get; set; }
        public int EmployeesWithEmail { get; set; }
        public int EmployeesWithSMS { get; set; }
        public int EmployeesWithTeams { get; set; }
        public int TotalEmployees { get; set; }
    }
}