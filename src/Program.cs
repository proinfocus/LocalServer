using System.Text;
using LocalServer;
using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddScoped<IService, Service>();

// var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

var app = builder.Build();

app.UseCors(o => o.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

var group = app.MapGroup("/api");

app.MapGet("/", () => "Welcome to your Local Server!");
group.MapGet("/{entity}", HandleGetAll);
group.MapGet("/{entity}/{id}", HandleGetById);
group.MapPost("/{entity}", HandleCreate);
group.MapPost("/generate/{entity}/{count}", HandleGenerate);
group.MapPut("/{entity}/{id}", HandleUpdateById);
group.MapDelete("/{entity}/{id}", HandleDeleteById);
group.MapDelete("/{entity}", HandleDeleteAll);

async Task<IResult> HandleGetAll(string entity, IService service, IDistributedCache cache, CancellationToken cancellationToken)
{
    var cacheData = cache.GetString(entity);
    if (string.IsNullOrEmpty(cacheData))
    {
        var data = await service.GetAll(entity, cancellationToken);
        if (data is null) return Results.NotFound();
        cache.SetString(entity, data);
        return Results.Content(data);
    }
    return Results.Content(cacheData);
}

async Task<IResult> HandleGetById(string entity, string id, IService service, IDistributedCache cache, CancellationToken cancellationToken)
{
    var cacheData = cache.GetString($"{entity}-{id}");
    if (string.IsNullOrEmpty(cacheData))
    {
        var data = await service.GetById(entity, id, cancellationToken);
        if (data is null) return Results.NotFound();
        cache.SetString($"{entity}-{id}", data);
        return Results.Content(data);
    }
    return Results.Content(cacheData);
}

async Task<IResult> HandleCreate(string entity, HttpRequest request, IService service, IDistributedCache cache, CancellationToken cancellationToken)
{
    cache.Remove(entity);
    using var ms = new MemoryStream();
    await request.Body.CopyToAsync(ms, cancellationToken);
    var newData = Encoding.UTF8.GetString(ms.ToArray());
    var data = await service.Create(entity, newData, cancellationToken);
    if (data is null) return Results.BadRequest();
    return Results.Content(data);
}

async Task<IResult> HandleGenerate(string entity, int count, HttpRequest request, IService service, IDistributedCache cache, CancellationToken cancellationToken)
{
    cache.Remove(entity);
    using var ms = new MemoryStream();
    await request.Body.CopyToAsync(ms, cancellationToken);
    var newData = Encoding.UTF8.GetString(ms.ToArray());    
    var result = await service.Generate(entity, count, newData, cancellationToken);
    if (result) return Results.Created();
    return Results.BadRequest();
}

async Task<IResult> HandleUpdateById(string entity, string id, HttpRequest request, IService service, IDistributedCache cache, CancellationToken cancellationToken)
{
    cache.Remove($"{entity}-id");
    using var ms = new MemoryStream();
    await request.Body.CopyToAsync(ms, cancellationToken);
    var newData = Encoding.UTF8.GetString(ms.ToArray());    
    var result = await service.Update(entity, id, newData, cancellationToken);
    if (result) return Results.NoContent();
    return Results.BadRequest();
}

async Task<IResult> HandleDeleteById(string entity, string id, IService service, IDistributedCache cache, CancellationToken cancellationToken)
{
    cache.Remove($"{entity}-id");
    var result = await service.Delete(entity, id, cancellationToken);
    if (result) return Results.NoContent();
    return Results.BadRequest();
}

async Task<IResult> HandleDeleteAll(string entity, IService service, IDistributedCache cache, CancellationToken cancellationToken)
{
    cache.Remove(entity);
    var result = await service.DeleteAll(entity, cancellationToken);
    if (result) return Results.NoContent();
    return Results.BadRequest();
}

app.Run();