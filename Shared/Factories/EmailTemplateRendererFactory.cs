using Contracts.Events;
using RazorLight;

namespace Shared.Factories;

public static class EmailTemplateRendererFactory
{
    private static readonly RazorLightEngine Engine = new RazorLightEngineBuilder()
        .UseFileSystemProject(Path.Combine(AppContext.BaseDirectory, "Templates", "Emails"))
        .UseMemoryCachingProvider()
        .Build();

    public static async Task<string> RenderTemplateAsync(this IEvent @event)
        => await Engine.CompileRenderAsync($"{@event.GetType().Name}.cshtml", @event);
}
