using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using SignFlow.Application.Options;

namespace SignFlow.Infrastructure.Email;

public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string plainTextContent, string? htmlContent = null, CancellationToken ct = default);
}

public class SendGridEmailSender : IEmailSender
{
    private readonly EmailOptions _options;
    public SendGridEmailSender(IOptions<EmailOptions> options) { _options = options.Value; }

    public async Task SendAsync(string toEmail, string subject, string plainTextContent, string? htmlContent = null, CancellationToken ct = default)
    {
        var client = new SendGridClient(_options.ApiKey);
        var from = new EmailAddress(_options.FromEmail, _options.FromName);
        var to = new EmailAddress(toEmail);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent ?? plainTextContent);
        await client.SendEmailAsync(msg, ct);
    }
}
