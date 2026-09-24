FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["src/SwissTechTrainer.Domain/SwissTechTrainer.Domain.csproj", "src/SwissTechTrainer.Domain/"]
COPY ["src/SwissTechTrainer.Application/SwissTechTrainer.Application.csproj", "src/SwissTechTrainer.Application/"]
COPY ["src/SwissTechTrainer.Infrastructure/SwissTechTrainer.Infrastructure.csproj", "src/SwissTechTrainer.Infrastructure/"]
COPY ["src/SwissTechTrainer.Web/SwissTechTrainer.Web.csproj", "src/SwissTechTrainer.Web/"]

RUN dotnet restore "src/SwissTechTrainer.Web/SwissTechTrainer.Web.csproj"

# Copy full source and publish
COPY . .
WORKDIR "/src/src/SwissTechTrainer.Web"
RUN dotnet publish "SwissTechTrainer.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SwissTechTrainer.Web.dll"]
