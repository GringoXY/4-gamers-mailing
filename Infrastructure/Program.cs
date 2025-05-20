using System.Globalization;
using Infrastructure.Extensions;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure(builder.Configuration);

builder.Configuration.Prepare();

var cultureName = Environment.GetEnvironmentVariable("ASPNETCORE_CULTURE") ?? CultureInfo.CurrentCulture.Name;
var culture = new CultureInfo(cultureName);
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var host = builder.Build();
host.Configure();
host.Run();
