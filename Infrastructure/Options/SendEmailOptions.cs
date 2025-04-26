namespace Infrastructure.Options;

public class SendEmailOptions
{
    public static readonly string SectionName = "BackgroundServices:SendEmail";

    public const string IntervalFormat = "d'.'hh':'mm':'ss";

    /// <summary>
    /// Default cleanup interval - every hour
    /// </summary>
    public const string DefaultInterval = "0.00:01:00";

    /// <summary>
    /// Expected format "dd.HH:mm:ss" i.e "0.00:01:00" runs every minute
    /// </summary>
    public string Interval { get; set; } = DefaultInterval;

    public SmtpOptions Smtp { get; set; }
}
