using System.IO;

namespace NuvTools.Notification.Mail;

public class MailPart
{
    public string MediaType { get; set; }
    public string MediaExtension { get; set; }
    public Stream Content { get; set; }
}