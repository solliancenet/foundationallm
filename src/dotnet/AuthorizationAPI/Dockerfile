#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine3.21 AS base
ARG FOUNDATIONALLM_VERSION

RUN apk update
RUN apk add icu-libs icu-data-full

WORKDIR /app
EXPOSE 80

ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production
ENV FOUNDATIONALLM_VERSION=${FOUNDATIONALLM_VERSION:-0.0.0}

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine3.21 AS build
WORKDIR /src
COPY . .

RUN dotnet restore "dotnet/Common/Common.csproj"
RUN dotnet build "dotnet/Common/Common.csproj" -c Release -o /app/build

RUN dotnet restore "dotnet/Agent/Agent.csproj"
RUN dotnet build "dotnet/Agent/Agent.csproj" -c Release -o /app/build

RUN dotnet restore "dotnet/Prompt/Prompt.csproj"
RUN dotnet build "dotnet/Prompt/Prompt.csproj" -c Release -o /app/build

RUN dotnet restore "dotnet/Configuration/Configuration.csproj"
RUN dotnet build "dotnet/Configuration/Configuration.csproj" -c Release -o /app/build

RUN dotnet restore "dotnet/DataSource/DataSource.csproj"
RUN dotnet build "dotnet/DataSource/DataSource.csproj" -c Release -o /app/build

RUN dotnet restore "dotnet/Vectorization/Vectorization.csproj"
RUN dotnet build "dotnet/Vectorization/Vectorization.csproj" -c Release -o /app/build

RUN dotnet restore "dotnet/Authorization/Authorization.csproj"
RUN dotnet build "dotnet/Authorization/Authorization.csproj" -c Release -o /app/build

RUN dotnet restore "dotnet/AuthorizationAPI/AuthorizationAPI.csproj"
RUN dotnet build "dotnet/AuthorizationAPI/AuthorizationAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "dotnet/Common/Common.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet publish "dotnet/Agent/Agent.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet publish "dotnet/Prompt/Prompt.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet publish "dotnet/Configuration/Configuration.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet publish "dotnet/DataSource/DataSource.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet publish "dotnet/Vectorization/Vectorization.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet publish "dotnet/Authorization/Authorization.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet publish "dotnet/AuthorizationAPI/AuthorizationAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FoundationaLLM.Authorization.API.dll"]
LABEL org.opencontainers.image.source=https://github.com/solliancenet/foundationallm
LABEL ai.foundationallm.version=${FOUNDATIONALLM_VERSION:-0.0.0}
