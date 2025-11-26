namespace Smartbook.LogicaDeNegocio.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendEmailConfirmationAsync(string to, string token, string baseUrl);
    Task SendAccountCreatedConfirmationAsync(string to, string nombres);
    Task SendPasswordResetAsync(string to, string token, string baseUrl);
    Task SendPasswordResetConfirmationAsync(string to, string nombres);
    Task SendVentaNotificationAsync(string to, string nombres, byte[] pdfContent, string numeroRecibo);
}

