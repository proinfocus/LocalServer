using System.Text;
using System.Text.Json;
using LocalServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IService, Service>();

// var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

var app = builder.Build();

app.MapGet("/", () => "Welcome to your Local Server!");

app.MapGet("/{entity}", HandleGetAll);
app.MapGet("/{entity}/{id}", HandleGetById);
app.MapPost("/{entity}", HandleCreate);
app.MapPost("/generate/{entity}/{count}", HandleGenerate);
app.MapPut("/{entity}/{id}", HandleUpdateById);
app.MapDelete("/{entity}/{id}", HandleDeleteById);
app.MapDelete("/{entity}", HandleDeleteAll);

async Task<IResult> HandleGetAll(string entity, IService service, CancellationToken cancellationToken)
{
    var data = await service.GetAll(entity, cancellationToken);
    if (data is null) return Results.NotFound();
    return Results.Content(data);
}

async Task<IResult> HandleGetById(string entity, string id, IService service, CancellationToken cancellationToken)
{
    var data = await service.GetById(entity, id, cancellationToken);
    if (data is null) return Results.NotFound();
    return Results.Content(data);
}

async Task<IResult> HandleCreate(string entity, HttpRequest request, IService service, CancellationToken cancellationToken)
{
    using var ms = new MemoryStream();
    await request.Body.CopyToAsync(ms, cancellationToken);
    var newData = Encoding.UTF8.GetString(ms.ToArray());
    var data = await service.Create(entity, newData, cancellationToken);
    if (data is null) return Results.BadRequest();
    return Results.Content(data);
}

async Task<IResult> HandleGenerate(string entity, int count, HttpRequest request, IService service, CancellationToken cancellationToken)
{
    using var ms = new MemoryStream();
    await request.Body.CopyToAsync(ms, cancellationToken);
    var newData = Encoding.UTF8.GetString(ms.ToArray());    
    var result = await service.Generate(entity, count, newData, cancellationToken);
    if (result) return Results.Created();
    return Results.BadRequest();
}

async Task<IResult> HandleUpdateById(string entity, string id, HttpRequest request, IService service, CancellationToken cancellationToken)
{
    using var ms = new MemoryStream();
    await request.Body.CopyToAsync(ms, cancellationToken);
    var newData = Encoding.UTF8.GetString(ms.ToArray());    
    var result = await service.Update(entity, id, newData, cancellationToken);
    if (result) return Results.NoContent();
    return Results.BadRequest();
}

async Task<IResult> HandleDeleteById(string entity, string id, IService service, CancellationToken cancellationToken)
{
    var result = await service.Delete(entity, id, cancellationToken);
    if (result) return Results.NoContent();
    return Results.BadRequest();
}

async Task<IResult> HandleDeleteAll(string entity, IService service, CancellationToken cancellationToken)
{
    var result = await service.DeleteAll(entity, cancellationToken);
    if (result) return Results.NoContent();
    return Results.BadRequest();
}

app.Run();