namespace NuvTools.Notification.Mail;

public class MailPart
{
    public required string MediaType { get; set; }
    public required string MediaExtension { get; set; }
    public required Stream Content { get; set; }
}