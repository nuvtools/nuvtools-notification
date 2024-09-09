namespace NuvTools.Notification.Mail;

public class MailMessage
{
    public required MailAddress From { get; set; }
    public required List<MailAddress> To { get; set; }
    public required string Subject { get; set; }
    public required string Body { get; set; }
    public List<MailPart>? Parts { get; set; }
}