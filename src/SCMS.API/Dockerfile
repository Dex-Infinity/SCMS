FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/SCMS.API/SCMS.API.csproj", "SCMS.API/"]
COPY ["src/SCMS.Domain/SCMS.Domain.csproj", "SCMS.Domain/"]
COPY ["src/SCMS.Infrastructure/SCMS.Infrastructure.csproj", "SCMS.Infrastructure/"]
RUN dotnet restore "SCMS.API/SCMS.API.csproj"

COPY src/ .
WORKDIR "/src/SCMS.API"
RUN dotnet build "SCMS.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SCMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SCMS.API.dll"]
