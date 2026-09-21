# ========================================================
# Dockerfile cho Backend ASP.NET Core (.NET 6)
# ========================================================

# Stage 1: Runtime Base
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 5000

# Stage 2: SDK Build
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy toàn bộ file .csproj để tận dụng Docker layer cache khi restore NuGet
COPY ["API/API.csproj", "API/"]
COPY ["Dms.Infrastructure/Dms.Infrastructure.csproj", "Dms.Infrastructure/"]
COPY ["Dms.Application/Dms.Application.csproj", "Dms.Application/"]
COPY ["Dms.Domain/Dms.Domain.csproj", "Dms.Domain/"]
COPY ["Dms.Application.Shared/Dms.Application.Shared.csproj", "Dms.Application.Shared/"]

# Restore packages
RUN dotnet restore "API/API.csproj"

# Copy toàn bộ mã nguồn
COPY . .

# Build dự án
WORKDIR "/src/API"
RUN dotnet build "API.csproj" -c Release -o /app/build

# Stage 3: Publish
FROM build AS publish
RUN dotnet publish "API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Final Image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Biến môi trường mặc định chạy local
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "API.dll"]
