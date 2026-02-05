using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

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

    var token = $"sclm-{Guid.NewGuid()}";
    issuedTokens[token] = request.User;

    return Results.Ok(new AuthResponse(
        User: request.User,
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
