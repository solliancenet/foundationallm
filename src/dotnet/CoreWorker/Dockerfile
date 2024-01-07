#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .

RUN dotnet restore "dotnet/Common/Common.csproj"
RUN dotnet build "dotnet/Common/Common.csproj" -c Release -o /app/build

RUN dotnet restore "dotnet/Core/Core.csproj"
RUN dotnet build "dotnet/Core/Core.csproj" -c Release -o /app/build

RUN dotnet restore "dotnet/CoreWorker/CoreWorker.csproj"
RUN dotnet build "dotnet/CoreWorker/CoreWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "dotnet/Common/Common.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet publish "dotnet/Core/Core.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet publish "dotnet/CoreWorker/CoreWorker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FoundationaLLM.Core.Worker.dll"]
LABEL org.opencontainers.image.source=https://github.com/solliancenet/foundationallm
