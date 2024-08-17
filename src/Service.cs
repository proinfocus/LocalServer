using System.Text;
using System.Text.Json;
using Bogus;

namespace LocalServer;

public sealed class Service : IService
{
    private readonly string dataFolder;
    private JsonSerializerOptions jso = new() {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public Service()
    {
        dataFolder = Path.Combine(Environment.CurrentDirectory, "data");
        if (!Directory.Exists(dataFolder)) Directory.CreateDirectory(dataFolder);
    }

    public async Task<string?> Create(string entity, string newData, CancellationToken cancellationToken)
    {
        var file = Path.Combine(dataFolder, entity + ".json");
        if (!File.Exists(file)) File.WriteAllText(file, "[]");
        try
        {
            int newId = GetLastID(entity) + 1;
            newData = Format(newData, newId);
            var currentData = File.ReadAllText(file);
            currentData = currentData == "[]" ? $"[ {newData} ]" : currentData[0..^1] + $",{newData.TrimEnd(',')} ]";
            await File.WriteAllTextAsync(file, currentData, cancellationToken);
            UpdateLastID(entity, newId);
            return newData;
        }
        catch
        {
            return null;
        }
    }

    public Task<bool> DeleteAll(string entity, CancellationToken cancellationToken)
    {
        var file = Path.Combine(dataFolder, entity + ".json");
        if (!File.Exists(file)) return Task.FromResult(false);

        File.Delete(file);
        DeleteEntity(entity);
        return Task.FromResult(true);
    }

    public async Task<bool> Delete(string entity, string id, CancellationToken cancellationToken)
    {
        bool isGuid = Guid.TryParse(id, out Guid _);
        var file = Path.Combine(dataFolder, entity + ".json");
        if (!File.Exists(file)) return false;

        var fileData = await File.ReadAllTextAsync(file, cancellationToken);
        var data = JsonSerializer.Deserialize<dynamic[]>(fileData, jso);
        if (data is null) return false;
        var sb = new StringBuilder();
        if (isGuid) id = $"\"{id}\"";
        for (int i = 0; i < data.Length; i++)
        {
            if (i > 0) sb.Append(',');
            if (data[i].ToString().Contains($"\"id\": {id}") || data[i].ToString().Contains($"\"id\":{id}")) continue;
            sb.Append(data[i]);
        }
        await File.WriteAllTextAsync(file, $"[{sb.ToString().TrimStart(',').TrimEnd(',')}]", cancellationToken);
        return true;
    }

    public async Task<string?> GetAll(string entity, CancellationToken cancellationToken)
    {
        var file = Path.Combine(dataFolder, entity + ".json");
        if (!File.Exists(file)) return null;
        return await File.ReadAllTextAsync(file, cancellationToken);
    }

    public async Task<string?> GetById(string entity, string id, CancellationToken cancellationToken)
    {
        bool isGuid = Guid.TryParse(id, out Guid _);
        var file = Path.Combine(dataFolder, entity + ".json");
        if (!File.Exists(file)) return null;

        var fileData = await File.ReadAllTextAsync(file, cancellationToken);
        var data = JsonSerializer.Deserialize<dynamic[]>(fileData, jso);
        if (data is null) return null;
        if (isGuid) id = $"\"{id}\"";
        for (int i = 0; i < data.Length; i++)
            if (data[i].ToString().Contains($"\"id\": {id}"))
                return data[i].ToString();
        return null;
    }

    public async Task<bool> Update(string entity, string id, string newData, CancellationToken cancellationToken)
    {
        bool isGuid = Guid.TryParse(id, out Guid _);
        var file = Path.Combine(dataFolder, entity + ".json");
        if (!File.Exists(file)) return false;

        var fileData = await File.ReadAllTextAsync(file, cancellationToken);
        var data = JsonSerializer.Deserialize<dynamic[]>(fileData, jso);
        if (data is null) return false;
        var sb = new StringBuilder();
        if (isGuid) id = $"\"{id}\"";
        for (int i = 0; i < data.Length; i++)
        {
            if (i > 0) sb.Append(',');
            if (data[i].ToString().Contains($"\"id\": {id}") || data[i].ToString().Contains($"\"id\":{id}")) sb.Append(Format(newData));
            else sb.Append(data[i]);
        }
        await File.WriteAllTextAsync(file, $"[{sb.ToString().TrimStart(',').TrimEnd(',')}]", cancellationToken);
        return true;
    }

    public async Task<bool> Generate(string entity, int count, string template, CancellationToken cancellationToken)
    {
        var file = Path.Combine(dataFolder, entity + ".json");
        if (!File.Exists(file)) File.WriteAllText(file, "[]");
        try
        {
            var currentData = File.ReadAllText(file);
            var sb = new StringBuilder();
            sb.Append('[');
            sb.Append(currentData[1..^1]);
            int lastId = GetLastID(entity);
            for (int i = 0; i < count; i++)
            {
                lastId++;
                var toAdd = Format(template, lastId);
                sb.Append((currentData == "[]" && i == 0) ? toAdd : $",{toAdd}");
            }
            sb.Append(']');
            await File.WriteAllTextAsync(file, $"[{sb.ToString().TrimStart(',').TrimEnd(',')}]", cancellationToken);
            UpdateLastID(entity, lastId);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string Format(string value, int? i = null)
    {
        int year = Random.Shared.Next(1900, DateTime.Now.Year);
        int month = Random.Shared.Next(1, 13);
        int day = Random.Shared.Next(1, 29);
        int hour = Random.Shared.Next(0, 24);
        int minute = Random.Shared.Next(0, 60);
        int second = Random.Shared.Next(0, 60);

        var fake = new Faker().Person;
        if (value.Contains("$LOREM$")) value = value.Replace("$LOREM$", new Faker().Lorem.Paragraph(1));
        if (value.Contains("$USERNAME$")) value = value.Replace("$USERNAME$", fake.UserName);
        if (value.Contains("$COMPANY$")) value = value.Replace("$COMPANY$", fake.Company.Name);
        if (value.Contains("$FIRSTNAME$")) value = value.Replace("$FIRSTNAME$", fake.FirstName);
        if (value.Contains("$LASTNAME$")) value = value.Replace("$LASTNAME$", fake.LastName);
        if (value.Contains("$FULLNAME$")) value = value.Replace("$FULLNAME$", fake.FullName);
        if (value.Contains("$PHONE$")) value = value.Replace("$PHONE$", fake.Phone);
        if (value.Contains("$WEBSITE$")) value = value.Replace("$WEBSITE$", fake.Website);
        if (value.Contains("$EMAIL$")) value = value.Replace("$EMAIL$", fake.Email.ToLower());
        if (value.Contains("$ADDRESS$")) value = value.Replace("$ADDRESS$", $"{fake.Address.Suite}, {fake.Address.Street}, {fake.Address.City}, {fake.Address.State}.");
        if (value.Contains("$GENDER$")) value = value.Replace("$GENDER$", fake.Gender.ToString());
        if (value.Contains("$ROLES$")) value = value.Replace("$ROLES$", GetRandomRoles());
        if (value.Contains("$PROFILE$")) value = value.Replace("$PROFILE$", GetRandomProfileImage(fake.Gender.ToString() == "Male" ? "men" : "women"));

        value = value.Replace("$GUID$", Guid.NewGuid().ToString())
                     .Replace("\"$SHORT$\"", Random.Shared.Next(0, 100).ToString())
                     .Replace("\"$PERCENT$\"", (Random.Shared.NextDouble() * 100).ToString("0.00"))
                     .Replace("\"$BIGINT$\"", Random.Shared.Next(10_000, 1_000_000).ToString())
                     .Replace("\"$INT$\"", Random.Shared.Next(100, 10_000).ToString())
                     .Replace("$DATETIME$", new DateTime(year, month, day, hour, minute, second).ToString("yyyy-MM-dd HH:mm:ss"))
                     .Replace("$DATE$", new DateTime(year, month, day, 0, 0, 0).ToString("yyyy-MM-dd HH:mm:ss"))
                     .Replace("$TIME$", new DateTime(1900, 1, 1, hour, minute, second).ToString("yyyy-MM-dd HH:mm:ss"))
                     .Replace("\"$BOOL$\"", Random.Shared.Next(0, 2) == 0 ? "true" : "false")
                     .Replace("\"$DOUBLE$\"", (Random.Shared.NextDouble() * 10000).ToString("0.00"));

        if (i is not null)
            return value.Replace("$I$", i.Value.ToString())
                        .Replace("\"$ID$\"", i.Value.ToString());
        return value;
    }

    private static string GetRandomProfileImage(string gender)
        => $"https://randomuser.me/api/portraits/{gender}/{Random.Shared.Next(1, 201)}.jpg";

    private void UpdateLastID(string entity, int id)
    {
        var file = Path.Combine(dataFolder, "local-server-settings");
        string fileData = "[]";
        if (File.Exists(file)) fileData = File.ReadAllText(file);
        List<Counter> entities = JsonSerializer.Deserialize<List<Counter>>(fileData, jso) ?? [];
        var found = entities?.Find(a => a.Entity == entity);
        if (found is not null) found.Id = id;
        else entities!.Add(new Counter(entity) { Id = id });
        File.WriteAllText(file, JsonSerializer.Serialize(entities, jso));
    }

    private void DeleteEntity(string entity)
    {
        var file = Path.Combine(dataFolder, "local-server-settings");
        string fileData = "[]";
        if (File.Exists(file)) fileData = File.ReadAllText(file);
        List<Counter> entities = JsonSerializer.Deserialize<List<Counter>>(fileData, jso) ?? [];
        var found = entities?.Find(a => a.Entity == entity)!;
        entities?.Remove(found);
        File.WriteAllText(file, JsonSerializer.Serialize(entities,jso));
    }

    private int GetLastID(string entity)
    {
        var file = Path.Combine(dataFolder, "local-server-settings");
        if (!File.Exists(file)) return 0;

        string fileData = File.ReadAllText(file);
        List<Counter> entities = JsonSerializer.Deserialize<List<Counter>>(fileData, jso)!;
        var found = entities?.Find(a => a.Entity == entity);
        return found is null ? 0 : found.Id;
    }

    private static string GetRandomRoles()
    {
        var roles = new[] { "SuperAdmin", "Admin", "Manager", "Supervisor", "User", "Tester", "Guest" };
        return roles[Random.Shared.Next(0, roles.Length)];
    }

    record Counter(string Entity)
    {
        public int Id { get; set; }
    };
}
