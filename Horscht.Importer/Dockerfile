#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Horscht.Importer/Horscht.Importer.csproj", "Horscht.Importer/"]
COPY ["Horscht.Contracts/Horscht.Contracts.csproj", "Horscht.Contracts/"]
COPY ["Horscht.Logic/Horscht.Logic.csproj", "Horscht.Logic/"]
RUN dotnet restore "./Horscht.Importer/Horscht.Importer.csproj"
COPY . .
WORKDIR "/src/Horscht.Importer"
RUN dotnet build "./Horscht.Importer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Horscht.Importer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Horscht.Importer.dll"]