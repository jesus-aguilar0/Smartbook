using System.Net.Http;
using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Smartbook.LogicaDeNegocio.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
        _smtpHost = _configuration["Email:SmtpHost"] ?? throw new InvalidOperationException("Email SMTP Host not configured");
        _smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        _smtpUser = _configuration["Email:SmtpUser"] ?? throw new InvalidOperationException("Email SMTP User not configured");
        _smtpPassword = _configuration["Email:SmtpPassword"] ?? throw new InvalidOperationException("Email SMTP Password not configured");
        _fromEmail = _configuration["Email:FromEmail"] ?? throw new InvalidOperationException("Email From Email not configured");
        _fromName = _configuration["Email:FromName"] ?? "SmartBook - CDI CECAR";
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_fromName, _fromEmail));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = body
        };

        // El template HTML ya usa la URL directa del logo, no necesita incrustación
        // La URL https://credito.cecar.edu.co/assets/logo/logo.png es accesible públicamente
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_smtpUser, _smtpPassword);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public async Task SendEmailConfirmationAsync(string to, string token, string baseUrl)
    {
        var confirmUrl = $"{baseUrl}/api/usuarios/confirmar-email?token={Uri.EscapeDataString(token)}";
        var body = GetEmailTemplate(
            "Confirmación de Correo Electrónico",
            $"<p>Estimado usuario,</p><p>Gracias por registrarse en SmartBook - Centro de Idiomas CECAR.</p><p>Para completar el proceso de registro y activar su cuenta, por favor haga clic en el siguiente enlace:</p><p style='text-align: center; margin: 30px 0;'><a href='{confirmUrl}' style='background-color: #0066cc; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>Confirmar Correo Electrónico</a></p><p><strong>Este enlace expirará en 1 hora.</strong></p><p>Si no puede hacer clic en el enlace, copie y pegue la siguiente URL en su navegador:</p><p style='background-color: #f0f0f0; padding: 10px; border-radius: 5px; word-break: break-all; font-family: monospace; font-size: 12px;'>{confirmUrl}</p><p>Si no solicitó esta confirmación, puede ignorar este correo de forma segura.</p>"
        );
        await SendEmailAsync(to, "Confirmación de Correo Electrónico - SmartBook", body);
    }

    public async Task SendAccountCreatedConfirmationAsync(string to, string nombres)
    {
        var body = GetEmailTemplate(
            "Cuenta Creada Exitosamente",
            $"<p>Estimado/a {nombres},</p><p>¡Bienvenido/a a SmartBook!</p><p>Su correo electrónico ha sido confirmado exitosamente y su cuenta ha sido activada en el sistema SmartBook del Centro de Idiomas CECAR.</p><p><strong>Ya puede iniciar sesión y comenzar a utilizar el sistema.</strong></p><p>Si tiene alguna pregunta o necesita asistencia, no dude en contactarnos.</p><p>Gracias por formar parte de nuestra comunidad.</p><p style='margin-top: 30px;'><strong>Equipo SmartBook - CDI CECAR</strong></p>"
        );
        await SendEmailAsync(to, "Cuenta Creada Exitosamente - SmartBook", body);
    }

    public async Task SendPasswordResetAsync(string to, string token, string baseUrl)
    {
        // El link debe apuntar a un endpoint que permita restablecer la contraseña
        // Por ahora, usaremos el endpoint POST con instrucciones claras
        var resetUrl = $"{baseUrl}/api/usuarios/confirmar-reset-password";
        var body = GetEmailTemplate(
            "Restablecimiento de Contraseña",
            $"<p>Estimado usuario,</p><p>Ha solicitado restablecer su contraseña en el sistema SmartBook.</p><p><strong>Token de restablecimiento:</strong></p><p style='background-color: #f0f0f0; padding: 15px; border-radius: 5px; font-family: monospace; word-break: break-all;'>{token}</p><p>Para restablecer su contraseña, use el siguiente endpoint:</p><p><strong>POST</strong> {resetUrl}</p><p><strong>Body (JSON):</strong></p><pre style='background-color: #f0f0f0; padding: 10px; border-radius: 5px;'>{{\"token\": \"{token}\", \"nuevaContrasena\": \"SuNuevaContrasena123!\"}}</pre><p><strong>Este token expirará en 1 hora.</strong></p><p>Si no solicitó este restablecimiento, puede ignorar este correo de forma segura.</p>"
        );
        await SendEmailAsync(to, "Restablecimiento de Contraseña - SmartBook", body);
    }

    public async Task SendPasswordResetConfirmationAsync(string to, string nombres)
    {
        var body = GetEmailTemplate(
            "Contraseña Restablecida Exitosamente",
            $"<p>Estimado/a {nombres},</p><p><strong>Su contraseña ha sido restablecida exitosamente.</strong></p><p>Ya puede iniciar sesión en el sistema SmartBook con su nueva contraseña.</p><p><strong>Importante:</strong> Si usted no realizó esta acción, por favor contacte al administrador del sistema inmediatamente a través de: cdi@cecar.edu.co</p><p>Por su seguridad, le recomendamos:</p><ul><li>Usar una contraseña segura y única</li><li>No compartir su contraseña con nadie</li><li>Cambiar su contraseña periódicamente</li></ul>"
        );
        await SendEmailAsync(to, "Contraseña Restablecida - SmartBook", body);
    }

    public async Task SendVentaNotificationAsync(string to, string nombres, byte[] pdfContent, string numeroRecibo)
    {
        var body = GetEmailTemplate(
            "Confirmación de Compra - SmartBook",
            $"<p>Estimado/a {nombres},</p><p>Gracias por su compra. Adjunto encontrará el detalle de su transacción (Recibo N° {numeroRecibo}).</p><p>Si tiene alguna pregunta, no dude en contactarnos.</p>"
        );

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_fromName, _fromEmail));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = $"Confirmación de Compra - Recibo N° {numeroRecibo}";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = body
        };

        // Agregar logo de CECAR - Intentar incrustar, pero usar URL directa como fallback
        var logoUrl = _configuration["Email:LogoUrl"] ?? "https://credito.cecar.edu.co/assets/logo/logo.png";
        bool logoIncrustado = false;
        
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(15);
            var logoBytes = await httpClient.GetByteArrayAsync(logoUrl).ConfigureAwait(false);
            
            if (logoBytes != null && logoBytes.Length > 0)
            {
                // Intentar incrustar el logo como recurso vinculado
                var logo = bodyBuilder.LinkedResources.Add("logo.png", logoBytes, ContentType.Parse("image/png"));
                logo.ContentId = "cecar-logo";
                logo.ContentDisposition = new ContentDisposition(ContentDisposition.Inline);
                logoIncrustado = true;
            }
        }
        catch
        {
            // Si falla la descarga, continuar sin incrustar
        }
        
        // Si no se pudo incrustar, usar URL directa (más confiable para la mayoría de clientes)
        if (!logoIncrustado)
        {
            body = body.Replace("cid:cecar-logo", logoUrl);
            bodyBuilder.HtmlBody = body;
        }

        bodyBuilder.Attachments.Add($"Recibo_{numeroRecibo}.pdf", pdfContent, ContentType.Parse("application/pdf"));

        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_smtpUser, _smtpPassword);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    private string GetEmailTemplate(string title, string content)
    {
        // URL directa del logo de CECAR - accesible públicamente
        const string logoUrl = "https://credito.cecar.edu.co/assets/logo/logo.png";
        
        // Usar URL directa del logo (más confiable y simple)
        // La URL es accesible públicamente y funciona correctamente
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #0066cc; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 30px; border-radius: 0 0 5px 5px; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
        .logo {{ max-width: 200px; height: auto; margin-bottom: 20px; display: block; margin-left: auto; margin-right: auto; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <img src='{logoUrl}' alt='CECAR Logo' class='logo' style='max-width: 200px; height: auto; margin-bottom: 20px; display: block; margin: 0 auto 20px auto;' />
            <h1>{title}</h1>
        </div>
        <div class='content'>
            {content}
        </div>
        <div class='footer'>
            <p>Centro de Idiomas - Corporación Universitaria del Caribe (CECAR)</p>
            <p>SmartBook - Sistema de Gestión de Inventario</p>
        </div>
    </div>
</body>
</html>";
    }
}

