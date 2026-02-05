# SCLM-API (C# / ASP.NET Core)

Implementación en **C#** de la API solicitada.

## Requisitos

- .NET 8 SDK

## Configuración por archivo TXT

La API lee los datos válidos desde la carpeta `data/`:

- `data/users.txt` para usuarios y contraseñas, formato `usuario;password`
- `data/contracts.txt` para contratos válidos, un contrato por línea

Archivos incluidos por defecto:

- `users.txt`
  - `Fernando;Admin1234`
  - `Test;Test`
- `contracts.txt`
  - `C0000`
  - `C1234`

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

Si credenciales correctas (según `users.txt`), responde `statusCode: 200` con token.

Si credenciales incorrectas, responde `statusCode: 401` con:

```json
{
  "statusCode": 401,
  "descripcion": "1003 - Usuario no autenticado: Las credenciales del portal MET son inválidas."
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
  "Contrato": "C1234"
}
```

Si contrato válido (según `contracts.txt`), responde `statusCode: 200` con `radicado`.

Si contrato inválido, responde `statusCode: 401` con descripción del error.
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
