#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Development

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY . .

RUN dotnet restore "dotnet/Common/Common.csproj"
RUN dotnet build "dotnet/Common/Common.csproj" -c Release -o /app/build

RUN dotnet restore "dotnet/SemanticKernel/SemanticKernel.csproj"
RUN dotnet build "dotnet/SemanticKernel/SemanticKernel.csproj" -c Release -o /app/build

RUN dotnet restore "dotnet/SemanticKernelAPI/SemanticKernelAPI.csproj"
RUN dotnet build "dotnet/SemanticKernelAPI/SemanticKernelAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "dotnet/Common/Common.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet publish "dotnet/SemanticKernel/SemanticKernel.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet publish "dotnet/SemanticKernelAPI/SemanticKernelAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FoundationaLLM.SemanticKernel.API.dll"]