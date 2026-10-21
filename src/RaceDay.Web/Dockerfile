# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["RaceDay.Web.csproj", "./"]
RUN dotnet restore "RaceDay.Web.csproj"
COPY . .
RUN dotnet build "RaceDay.Web.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "RaceDay.Web.csproj" -c Release -o /app/publish

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RaceDay.Web.dll"]