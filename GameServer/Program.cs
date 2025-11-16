var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapPost("/login", (LoginRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.UserId))
    {
        return Results.BadRequest("Invalid user");
    }
    var sessionId = Guid.NewGuid().ToString("N");
    return Results.Ok(new LoginResponse(req.UserId, sessionId));
});

app.MapPost("/move", (MoveRequest req) =>
{
    var newX = req.X + 1;
    var newY = req.Y + 1;
    return Results.Ok(new { x = newX, y = newY });
});

app.Run();

record LoginRequest(string UserId, string Password);
record LoginResponse(string UserId, string SessionId);
record MoveRequest(string SessionId, int X, int Y);
