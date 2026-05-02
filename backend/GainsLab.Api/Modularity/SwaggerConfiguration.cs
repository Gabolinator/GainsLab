using System.Reflection;

namespace GainsLab.Api.Modularity;

public static class SwaggerConfiguration
{
    public static WebApplication ConfigureSwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "GainsLab API v1");
            options.RoutePrefix = "swagger";
        });
        
        return app;
    }
}