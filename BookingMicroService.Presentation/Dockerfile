# ---------- Base image ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["BookingMicroService.Presentation/BookingMicroService.Presentation.csproj", "BookingMicroService.Presentation/"]
COPY ["BookingMicroService.Application/BookingMicroService.Application.csproj", "BookingMicroService.Application/"]
COPY ["BookingMicroService.Domain/BookingMicroService.Domain.csproj", "BookingMicroService.Domain/"]
COPY ["Common/Common.csproj", "Common/"]
COPY ["BookingMicroService.Infrastructure/BookingMicroService.Infrastructure.csproj", "BookingMicroService.Infrastructure/"]

RUN dotnet restore "BookingMicroService.Presentation/BookingMicroService.Presentation.csproj"

COPY . .
WORKDIR "/src/BookingMicroService.Presentation"
RUN dotnet build "BookingMicroService.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ---------- Publish stage ----------
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BookingMicroService.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ---------- Runtime stage ----------
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookingMicroService.Presentation.dll"]
