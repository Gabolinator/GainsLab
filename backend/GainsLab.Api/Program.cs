using GainsLab.Api.Extensions;
using GainsLab.Infrastructure.Logging;
using GainsLab.Infrastructure.Utilities;


var logger = new GainsLabLogger("API");
logger.ToggleDecoration(false);
var clock = new Clock();

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureEnvironment(logger);
builder.ConfigureServices(logger, clock);

var app = builder.Build();
app.ConfigureRequestPipeline();
app.MapEndpoints();

await app.RunApplicationAsync(logger, clock);


/// <summary>
/// Partial Program class required for user secrets configuration.
/// </summary>
public partial class Program { }
