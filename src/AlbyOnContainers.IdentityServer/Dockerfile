#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["src/AlbyOnContainers.IdentityServer/AlbyOnContainers.IdentityServer.csproj", "src/AlbyOnContainers.IdentityServer/"]
COPY ["src/AlbyOnContainers.Shared.Messages/AlbyOnContainers.Shared.Messages.csproj", "src/AlbyOnContainers.Shared.Messages/"]
RUN dotnet restore "src/AlbyOnContainers.IdentityServer/AlbyOnContainers.IdentityServer.csproj"
COPY . .
WORKDIR "/src/src/AlbyOnContainers.IdentityServer"
RUN dotnet build "AlbyOnContainers.IdentityServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AlbyOnContainers.IdentityServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AlbyOnContainers.IdentityServer.dll"]