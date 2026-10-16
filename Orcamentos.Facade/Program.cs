var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.MapGet("/health", () => Results.Ok(new { facade = "ok", legado = "down", targetConfigurado = "legacy" }));
app.Run();

public partial class Program { }
