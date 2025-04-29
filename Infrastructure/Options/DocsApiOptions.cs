namespace Infrastructure.Options;

public class DocsApiOptions
{
    public static readonly string SectionName = "DocsApi";

    public string Scheme { get; set; } = "http";
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 5287;

    public Uri Uri => new UriBuilder(Scheme, Host, Port).Uri;
}
