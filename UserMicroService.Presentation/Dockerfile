# ---------- Base image ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["UserMicroService.Presentation/UserMicroService.Presentation.csproj", "UserMicroService.Presentation/"]
COPY ["UserMicroService.Application/UserMicroService.Application.csproj", "UserMicroService.Application/"]
COPY ["UserMicroService.Domain/UserMicroService.Domain.csproj", "UserMicroService.Domain/"]
COPY ["Common/Common.csproj", "Common/"]
COPY ["UserMicroService.Infrastructure/UserMicroService.Infrastructure.csproj", "UserMicroService.Infrastructure/"]

RUN dotnet restore "UserMicroService.Presentation/UserMicroService.Presentation.csproj"

COPY . .
WORKDIR "/src/UserMicroService.Presentation"
RUN dotnet build "UserMicroService.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ---------- Publish stage ----------
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "UserMicroService.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ---------- Runtime stage ----------
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserMicroService.Presentation.dll"]