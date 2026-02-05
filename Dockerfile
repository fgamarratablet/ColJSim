# Imagen base para build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar csproj y restaurar
COPY *.csproj ./
RUN dotnet restore

# Copiar todo y compilar
COPY . .
RUN dotnet publish -c Release -o out

# Imagen final más ligera
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Render usa la variable PORT
ENV ASPNETCORE_URLS=http://+:$PORT

EXPOSE 8080
ENTRYPOINT ["dotnet", "ColJSim.dll"]
