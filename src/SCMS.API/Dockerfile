FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/SCMS.API/SCMS.API.csproj", "src/SCMS.API/"]
COPY ["src/SCMS.Domain/SCMS.Domain.csproj", "src/SCMS.Domain/"]
COPY ["src/SCMS.Infrastructure/SCMS.Infrastructure.csproj", "src/SCMS.Infrastructure/"]

RUN dotnet restore "src/SCMS.API/SCMS.API.csproj"

COPY . .

WORKDIR "/src/src/SCMS.API"
RUN dotnet build "SCMS.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SCMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "SCMS.API.dll"]