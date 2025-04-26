using Infrastructure.Extensions;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure(builder.Configuration);

builder.Configuration.Prepare();

var host = builder.Build();
host.Run();
