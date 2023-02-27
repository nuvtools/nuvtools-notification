namespace NuvTools.Notification.Mail.Configuration;

/// <summary>
/// Contains the mail configuration that should be loaded from appsettings file.
/// <para>The default section name is "NuvTools.Notification.Mail"</para>
/// </summary>
public class MailConfigurationSection
{
    public string From { get; set; }
    public string Host { get; set; }
    public int Port { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string DisplayName { get; set; }
}