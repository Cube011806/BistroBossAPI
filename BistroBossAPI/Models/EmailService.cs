namespace BistroBossAPI
{
    //using MailKit.Net.Smtp;
    //using MimeKit;
    //using Microsoft.Extensions.Options;
    //public interface IEmailService
    //{
    //    void SendEmail(string toEmail, string subject, string message);
    //}
    //public class EmailService : IEmailService
    //{
    //    private readonly EmailSettings _settings;

    //    public EmailService(IOptions<EmailSettings> options)
    //    {
    //        _settings = options.Value;
    //    }

        
    //    public void SendEmail(string toEmail, string subject, string message)
    //    {
    //        var email = new MimeMessage();
    //        email.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
    //        email.To.Add(MailboxAddress.Parse(toEmail));
    //        email.Subject = subject;

    //        var builder = new BodyBuilder { HtmlBody = message };
    //        email.Body = builder.ToMessageBody();

    //        using var smtp = new SmtpClient();
    //        smtp.Connect(_settings.SmtpServer, _settings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
    //        smtp.Authenticate(_settings.Username, _settings.Password);
    //        smtp.Send(email);
    //        smtp.Disconnect(true);
    //    }
    //}
}
