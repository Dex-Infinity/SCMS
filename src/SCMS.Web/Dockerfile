FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/SCMS.Web/SCMS.Web.csproj", "src/SCMS.Web/"]

RUN dotnet restore "src/SCMS.Web/SCMS.Web.csproj"

COPY . .

WORKDIR "/src/src/SCMS.Web"
RUN dotnet build "SCMS.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SCMS.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "SCMS.Web.dll"]