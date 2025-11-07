namespace SignFlow.Application.Options;

public class EmailOptions
{
    public const string SectionName = "Email";
    public string ApiKey { get; set; } = string.Empty; // SendGrid API key
    public string FromEmail { get; set; } = "no-reply@example.com";
    public string FromName { get; set; } = "SignFlow";
}
