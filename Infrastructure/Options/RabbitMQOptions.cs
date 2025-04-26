using RabbitMQ.Client;

namespace Shared.Settings;

/// <summary>
/// Represents RabbitMQSettings section in appsettings.*.json
/// </summary>
public class RabbitMQOptions
{
    public string HostName { get; set; } = string.Empty;
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string QueueName { get; set; } = string.Empty;

    public ConnectionFactory Factory => new()
    {
        HostName = HostName,
        Port = Port,
        UserName = UserName,
        Password = Password,
    };
}
