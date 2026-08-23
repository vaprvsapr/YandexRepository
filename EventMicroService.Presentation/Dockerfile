# ---------- Base image ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["EventMicroService.Presentation/EventMicroService.Presentation.csproj", "EventMicroService.Presentation/"]
COPY ["EventMicroService.Application/EventMicroService.Application.csproj", "EventMicroService.Application/"]
COPY ["EventMicroService.Domain/EventMicroService.Domain.csproj", "EventMicroService.Domain/"]
COPY ["Common/Common.csproj", "Common/"]
COPY ["EventMicroService.Infrastructure/EventMicroService.Infrastructure.csproj", "EventMicroService.Infrastructure/"]

RUN dotnet restore "EventMicroService.Presentation/EventMicroService.Presentation.csproj"

COPY . .
WORKDIR "/src/EventMicroService.Presentation"
RUN dotnet build "EventMicroService.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ---------- Publish stage ----------
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EventMicroService.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ---------- Runtime stage ----------
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventMicroService.Presentation.dll"]