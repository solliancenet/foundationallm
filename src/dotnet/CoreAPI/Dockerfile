#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Development

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY . .

RUN dotnet restore "Core/Core.csproj"
RUN dotnet build "Core/Core.csproj" -c Release -o /app/build

RUN dotnet restore "CoreAPI/CoreAPI.csproj"
RUN dotnet build "CoreAPI/CoreAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Core/Core.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet publish "CoreAPI/CoreAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FoundationaLLM.Core.API.dll"]