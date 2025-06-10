namespace PlanningPresenceBlazor.Services
{
    public class ToastService
    {
        public event Action<ToastMessage>? OnShowToast;

        public void ShowSuccess(string message, string title = "Succès")
        {
            OnShowToast?.Invoke(new ToastMessage
            {
                Message = message,
                Title = title,
                Type = ToastType.Success,
                Duration = 4000
            });
        }

        public void ShowError(string message, string title = "Erreur")
        {
            OnShowToast?.Invoke(new ToastMessage
            {
                Message = message,
                Title = title,
                Type = ToastType.Error,
                Duration = 6000
            });
        }

        public void ShowWarning(string message, string title = "Attention")
        {
            OnShowToast?.Invoke(new ToastMessage
            {
                Message = message,
                Title = title,
                Type = ToastType.Warning,
                Duration = 5000
            });
        }

        public void ShowInfo(string message, string title = "Information")
        {
            OnShowToast?.Invoke(new ToastMessage
            {
                Message = message,
                Title = title,
                Type = ToastType.Info,
                Duration = 4000
            });
        }
    }

    public class ToastMessage
    {
        public string Message { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public ToastType Type { get; set; }
        public int Duration { get; set; } = 4000;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public enum ToastType
    {
        Success,
        Error,
        Warning,
        Info
    }
}