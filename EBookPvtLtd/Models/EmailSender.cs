using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Identity.UI.Services;

public class EmailSender : IEmailSender
{
    private readonly EmailSettings _emailSettings;

    public EmailSender(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
        _emailSettings.SmtpServer = "smtp.gmail.com";
        _emailSettings.FromEmail = "sandutharu40@gmail.com";
        _emailSettings.Username = "sandutharu40@gmail.com";
        _emailSettings.Password = "wtyo zcnu evdq xdvm";
        _emailSettings.FromName = "E-Book (Pvt) Ltd.";
        _emailSettings.Port = _emailSettings.Port > 0 ? _emailSettings.Port : 587;
    }

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var client = new SmtpClient(_emailSettings.SmtpServer)
        {
            Port = _emailSettings.Port,
            Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
            Subject = subject,
            Body = message,
            IsBodyHtml = true
        };
        mailMessage.To.Add(email);

        await client.SendMailAsync(mailMessage);
    }
}

public class EmailSettings
{
    public string SmtpServer { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string FromEmail { get; set; }
    public string FromName { get; set; }
}
