# SCLM-API (C# / ASP.NET Core)

Implementación en **C#** de la API solicitada.

## Requisitos

- .NET 8 SDK

## Ejecutar

```bash
dotnet run
```

Servidor en:

- `http://localhost:8888`

## 1) POST AuthSclmPlus

Endpoint:

- `POST /SCLM-API/api/AuthSclmPlus`

Body:

```json
{
  "user": "Fernando",
  "password": "Admin1234"
}
```

Respuesta de ejemplo:

```json
{
  "user": "Fernando",
  "typeToken": "Bearer",
  "statusCode": 200,
  "descripcion": "Usuario autenticado: Se ha generado el token correctamente",
  "token": "sclm-xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
}
```

## 2) POST enviarReporteMET

Endpoint:

- `POST /SCLM-API/api/enviarReporteMET`

Header:

- `Authorization: Bearer <token>`

Body:

```json
{
  "urlWebhook": "http://entornowigos:9999/v1/ColjuegosResponse",
  "Contrato": "CXXXX"
}
```

Respuesta de ejemplo:

```json
{
  "statusCode": 200,
  "radicado": 1,
  "descripcion": "Se ha recibido el reporte correctamente"
}
```
