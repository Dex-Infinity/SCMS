FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/SCMS.Web/SCMS.Web.csproj", "SCMS.Web/"]
RUN dotnet restore "SCMS.Web/SCMS.Web.csproj"

COPY src/ .
WORKDIR "/src/SCMS.Web"
RUN dotnet build "SCMS.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SCMS.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SCMS.Web.dll"]
