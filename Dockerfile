# ===========================
# BUILD
# ===========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY TuyenSinh.sln .

COPY TuyenSinh/TuyenSinh.csproj TuyenSinh/

RUN dotnet restore

COPY . .

RUN dotnet publish TuyenSinh/TuyenSinh.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ===========================
# RUNTIME
# ===========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "TuyenSinh.dll"]
