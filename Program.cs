using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var usersFilePath = Path.Combine(AppContext.BaseDirectory, "data", "users.txt");
var contractsFilePath = Path.Combine(AppContext.BaseDirectory, "data", "contracts.txt");

if (!File.Exists(usersFilePath))
{
    usersFilePath = Path.Combine(Directory.GetCurrentDirectory(), "data", "users.txt");
}

if (!File.Exists(contractsFilePath))
{
    contractsFilePath = Path.Combine(Directory.GetCurrentDirectory(), "data", "contracts.txt");
}

var validUsers = LoadUsers(usersFilePath);
var validContracts = LoadContracts(contractsFilePath);
var issuedTokens = new ConcurrentDictionary<string, string>();
var radicadoCounter = 0;

app.MapPost("/SCLM-API/api/AuthSclmPlus", (AuthRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.User) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new ErrorResponse(
            StatusCode: 400,
            Descripcion: "Parámetros requeridos: user y password"
        ));
    }

    var user = request.User.Trim();
    var password = request.Password.Trim();

    if (!validUsers.TryGetValue(user, out var validPassword) || !string.Equals(validPassword, password, StringComparison.Ordinal))
    {
        return Results.Json(new ErrorResponse(
            StatusCode: 401,
            Descripcion: "1003 - Usuario no autenticado: Las credenciales del portal MET son inválidas."
        ), statusCode: 401);
    }

    var token = $"sclm-{Guid.NewGuid()}";
    issuedTokens[token] = user;

    return Results.Ok(new AuthResponse(
        User: user,
        TypeToken: "Bearer",
        StatusCode: 200,
        Descripcion: "Usuario autenticado: Se ha generado el token correctamente",
        Token: token
    ));
});

app.MapPost("/SCLM-API/api/enviarReporteMET", (HttpRequest httpRequest, ReporteRequest request) =>
{
    var authHeader = httpRequest.Headers.Authorization.ToString();

    if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
    {
        return Results.Json(new ErrorResponse(
            StatusCode: 401,
            Descripcion: "Token inválido o ausente"
        ), statusCode: 401);
    }

    var token = authHeader["Bearer ".Length..].Trim();

    if (string.IsNullOrWhiteSpace(token) || !issuedTokens.ContainsKey(token))
    {
        return Results.Json(new ErrorResponse(
            StatusCode: 401,
            Descripcion: "Token inválido o ausente"
        ), statusCode: 401);
    }

    if (string.IsNullOrWhiteSpace(request.UrlWebhook) || string.IsNullOrWhiteSpace(request.Contrato))
    {
        return Results.BadRequest(new ErrorResponse(
            StatusCode: 400,
            Descripcion: "Parámetros requeridos: urlWebhook y Contrato"
        ));
    }

    var contrato = request.Contrato.Trim();

    if (!validContracts.Contains(contrato))
    {
        return Results.Json(new ErrorResponse(
            StatusCode: 401,
            Descripcion: "1004 - Contrato no válido: El contrato enviado no existe o no está autorizado."
        ), statusCode: 401);
    }

    var radicado = Interlocked.Increment(ref radicadoCounter);

    return Results.Ok(new ReporteResponse(
        StatusCode: 200,
        Radicado: radicado,
        Descripcion: "Se ha recibido el reporte correctamente"
    ));
});

app.MapMethods("/{*path}", new[] { "GET", "PUT", "PATCH", "DELETE", "OPTIONS", "HEAD" }, () =>
    Results.Json(new ErrorResponse(
        StatusCode: 405,
        Descripcion: "Método no permitido. Use POST."
    ), statusCode: 405)
);

app.MapFallback(() => Results.Json(new ErrorResponse(
    StatusCode: 404,
    Descripcion: "Ruta no encontrada"
), statusCode: 404));

app.Run("http://0.0.0.0:8888");

static Dictionary<string, string> LoadUsers(string filePath)
{
    var users = new Dictionary<string, string>(StringComparer.Ordinal);

    if (!File.Exists(filePath))
    {
        return users;
    }

    foreach (var line in File.ReadAllLines(filePath))
    {
        var trimmed = line.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            continue;
        }

        var parts = trimmed.Split(';', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
        {
            continue;
        }

        users[parts[0]] = parts[1];
    }

    return users;
}

static HashSet<string> LoadContracts(string filePath)
{
    var contracts = new HashSet<string>(StringComparer.Ordinal);

    if (!File.Exists(filePath))
    {
        return contracts;
    }

    foreach (var line in File.ReadAllLines(filePath))
    {
        var trimmed = line.Trim();
        if (!string.IsNullOrWhiteSpace(trimmed))
        {
            contracts.Add(trimmed);
        }
    }

    return contracts;
}

public record AuthRequest(string User, string Password);
public record ReporteRequest(string UrlWebhook, string Contrato);

public record AuthResponse(
    string User,
    string TypeToken,
    int StatusCode,
    string Descripcion,
    string Token
);

public record ReporteResponse(
    int StatusCode,
    int Radicado,
    string Descripcion
);

public record ErrorResponse(
    int StatusCode,
    string Descripcion
);
