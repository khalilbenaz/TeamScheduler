namespace PlanningPresenceBlazor.Data;


// Classes de configuration
public class EmailOptions
{
    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "Planning Présence";
}

public class SMSOptions
{
    public string ApiUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string FromNumber { get; set; } = string.Empty;
}

public class TeamsOptions
{
    public string WebhookUrl { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

public class PlanningOptions
{
    public string DefaultNotificationMethod { get; set; } = "Email";
    public bool EnableTestMode { get; set; } = true;
    public bool AutoSendNotifications { get; set; } = false;
    public int NotificationRetryAttempts { get; set; } = 3;
}