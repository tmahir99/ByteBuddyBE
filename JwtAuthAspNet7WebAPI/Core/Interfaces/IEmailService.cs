namespace JwtAuthAspNet7WebAPI.Core.Interfaces
{
    public interface IEmailService
    {
        Task SendActivationEmailAsync(string email, string userName, string activationToken);
        Task SendPasswordResetEmailAsync(string email, string userName, string resetToken);
        Task SendWelcomeEmailAsync(string email, string userName);
        Task SendNotificationEmailAsync(string email, string subject, string message);
    }
}
