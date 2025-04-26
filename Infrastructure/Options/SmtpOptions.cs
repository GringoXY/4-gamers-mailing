using System.Net;
using System.Net.Mail;

namespace Infrastructure.Options;

public class SmtpOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = false;

    public SmtpClient Client => new()
    {
        Host = Host,
        Port = Port,
        Credentials = new NetworkCredential(Username, Password),
        EnableSsl = EnableSsl,
    };
}
