#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0.3-buster-slim-arm32v7 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
WORKDIR /src
COPY ["src/AlbyOnContainers.Hermes/AlbyOnContainers.Hermes.csproj", "src/AlbyOnContainers.Hermes/"]
COPY ["src/AlbyOnContainers.Shared.Messages/AlbyOnContainers.Shared.Messages.csproj", "src/AlbyOnContainers.Shared.Messages/"]
RUN dotnet restore "src/AlbyOnContainers.Hermes/AlbyOnContainers.Hermes.csproj"
COPY . .
WORKDIR "/src/src/AlbyOnContainers.Hermes"
RUN dotnet build "AlbyOnContainers.Hermes.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AlbyOnContainers.Hermes.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AlbyOnContainers.Hermes.dll"]