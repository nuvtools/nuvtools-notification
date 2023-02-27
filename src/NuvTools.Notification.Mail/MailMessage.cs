using System.Collections.Generic;

namespace NuvTools.Notification.Mail;

public class MailMessage
{
    public MailAddress From { get; set; }
    public List<MailAddress> To { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public List<MailPart> Parts { get; set; }
}