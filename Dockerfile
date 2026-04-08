FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/NOL.API/NOL.API.csproj", "NOL.API/"]
COPY ["src/NOL.Application/NOL.Application.csproj", "NOL.Application/"]
COPY ["src/NOL.Domain/NOL.Domain.csproj", "NOL.Domain/"]
COPY ["src/NOL.Infrastructure/NOL.Infrastructure.csproj", "NOL.Infrastructure/"]

RUN dotnet restore "NOL.API/NOL.API.csproj"

COPY src/ .

RUN dotnet build "NOL.API/NOL.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NOL.API/NOL.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NOL.API.dll"]
