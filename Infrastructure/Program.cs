using Infrastructure.Extensions;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure(builder.Configuration);

builder.Configuration.Prepare();

var host = builder.Build();
host.Configure();
host.Run();
