using JwtAuthAspNet7WebAPI.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace JwtAuthAspNet7WebAPI.Core.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly string _baseUrl;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Load email configuration from appsettings.json
            _smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            _smtpUsername = _configuration["EmailSettings:SmtpUsername"] ?? "";
            _smtpPassword = _configuration["EmailSettings:SmtpPassword"] ?? "";
            _fromEmail = _configuration["EmailSettings:FromEmail"] ?? "";
            _fromName = _configuration["EmailSettings:FromName"] ?? "ByteBuddy";
            _baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:7000";
        }

        public async Task SendActivationEmailAsync(string email, string userName, string activationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    _logger.LogWarning("SendActivationEmailAsync called with empty email");
                    throw new ArgumentException("Email is required", nameof(email));
                }

                if (string.IsNullOrWhiteSpace(activationToken))
                {
                    _logger.LogWarning("SendActivationEmailAsync called with empty activation token");
                    throw new ArgumentException("Activation token is required", nameof(activationToken));
                }

                _logger.LogInformation("Sending activation email to: {Email}", email);

                var activationUrl = $"{_baseUrl}/api/auth/activate?token={activationToken}";
                var subject = "Aktiviraj svoj ByteBuddy nalog";
                var body = GetActivationEmailTemplate(userName, activationUrl);

                await SendEmailAsync(email, subject, body);
                _logger.LogInformation("Activation email sent successfully to: {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending activation email to: {Email}", email);
                throw;
            }
        }

        public async Task SendPasswordResetEmailAsync(string email, string userName, string resetToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    _logger.LogWarning("SendPasswordResetEmailAsync called with empty email");
                    throw new ArgumentException("Email is required", nameof(email));
                }

                if (string.IsNullOrWhiteSpace(resetToken))
                {
                    _logger.LogWarning("SendPasswordResetEmailAsync called with empty reset token");
                    throw new ArgumentException("Reset token is required", nameof(resetToken));
                }

                _logger.LogInformation("Sending password reset email to: {Email}", email);

                var resetUrl = $"{_baseUrl}/api/auth/reset-password?token={resetToken}";
                var subject = "Resetuj svoju ByteBuddy lozinku";
                var body = GetPasswordResetEmailTemplate(userName, resetUrl);

                await SendEmailAsync(email, subject, body);
                _logger.LogInformation("Password reset email sent successfully to: {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset email to: {Email}", email);
                throw;
            }
        }

        public async Task SendWelcomeEmailAsync(string email, string userName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    _logger.LogWarning("SendWelcomeEmailAsync called with empty email");
                    throw new ArgumentException("Email is required", nameof(email));
                }

                _logger.LogInformation("Sending welcome email to: {Email}", email);

                var subject = "Dobrodošli u ByteBuddy zajednicu!";
                var body = GetWelcomeEmailTemplate(userName);

                await SendEmailAsync(email, subject, body);
                _logger.LogInformation("Welcome email sent successfully to: {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending welcome email to: {Email}", email);
                throw;
            }
        }

        public async Task SendNotificationEmailAsync(string email, string subject, string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    _logger.LogWarning("SendNotificationEmailAsync called with empty email");
                    throw new ArgumentException("Email is required", nameof(email));
                }

                _logger.LogInformation("Sending notification email to: {Email}", email);

                var body = GetNotificationEmailTemplate(message);
                await SendEmailAsync(email, subject, body);
                _logger.LogInformation("Notification email sent successfully to: {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification email to: {Email}", email);
                throw;
            }
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                using var client = new SmtpClient(_smtpHost, _smtpPort);
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);

                using var message = new MailMessage();
                message.From = new MailAddress(_fromEmail, _fromName);
                message.To.Add(toEmail);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;
                message.BodyEncoding = Encoding.UTF8;
                message.SubjectEncoding = Encoding.UTF8;

                await client.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to: {Email}", toEmail);
                throw;
            }
        }

        private string GetActivationEmailTemplate(string userName, string activationUrl)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Aktivacija naloga</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #28a745; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>ByteBuddy</h1>
        </div>
        <div class='content'>
            <h2>Pozdrav {userName}!</h2>
            <p>Hvala vam što ste se registrovali na ByteBuddy platformu za programere!</p>
            <p>Da biste aktivirali svoj nalog, molimo vas kliknite na dugme ispod:</p>
            <a href='{activationUrl}' class='button'>Aktiviraj nalog</a>
            <p>Ili kopirajte i zalepite sledeći link u vaš browser:</p>
            <p><a href='{activationUrl}'>{activationUrl}</a></p>
            <p>Ovaj link će biti važeći narednih 24 sata.</p>
            <p>Ako niste kreirali nalog na ByteBuddy, molimo vas ignorišite ovaj email.</p>
        </div>
        <div class='footer'>
            <p>&copy; 2024 ByteBuddy. Sva prava zadržana.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetPasswordResetEmailTemplate(string userName, string resetUrl)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Resetovanje lozinke</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #dc3545; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #dc3545; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>ByteBuddy</h1>
        </div>
        <div class='content'>
            <h2>Pozdrav {userName}!</h2>
            <p>Primili smo zahtev za resetovanje vaše lozinke na ByteBuddy platformi.</p>
            <p>Da biste resetovali svoju lozinku, kliknite na dugme ispod:</p>
            <a href='{resetUrl}' class='button'>Resetuj lozinku</a>
            <p>Ili kopirajte i zalepite sledeći link u vaš browser:</p>
            <p><a href='{resetUrl}'>{resetUrl}</a></p>
            <p>Ovaj link će biti važeći narednih 1 sat.</p>
            <p>Ako niste zatražili resetovanje lozinke, molimo vas ignorišite ovaj email.</p>
        </div>
        <div class='footer'>
            <p>&copy; 2024 ByteBuddy. Sva prava zadržana.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetWelcomeEmailTemplate(string userName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Dobrodošli u ByteBuddy</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Dobrodošli u ByteBuddy!</h1>
        </div>
        <div class='content'>
            <h2>Pozdrav {userName}!</h2>
            <p>Vaš nalog je uspešno aktiviran! Dobrodošli u ByteBuddy zajednicu programera.</p>
            <p>Sada možete:</p>
            <ul>
                <li>Deliti svoje isečke koda sa zajednicom</li>
                <li>Povezivati se sa drugim programerima</li>
                <li>Komentarisati i lajkovati kodove</li>
                <li>Kreirati stranice i projekte</li>
                <li>Slati direktne poruke</li>
            </ul>
            <p>Počnite svoju ByteBuddy avanturu već danas!</p>
        </div>
        <div class='footer'>
            <p>&copy; 2024 ByteBuddy. Sva prava zadržana.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetNotificationEmailTemplate(string message)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>ByteBuddy obaveštenje</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #17a2b8; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>ByteBuddy</h1>
        </div>
        <div class='content'>
            {message}
        </div>
        <div class='footer'>
            <p>&copy; 2024 ByteBuddy. Sva prava zadržana.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
