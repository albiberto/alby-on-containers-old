#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
WORKDIR /src
COPY ["src/AlbyOnContainers.Services.Catalog/AlbyOnContainers.Services.Catalog.csproj", "src/AlbyOnContainers.Services.Catalog/"]
RUN dotnet restore "src/AlbyOnContainers.Services.Catalog/AlbyOnContainers.Services.Catalog.csproj"
COPY . .
WORKDIR "/src/src/AlbyOnContainers.Services.Catalog"
RUN dotnet build "AlbyOnContainers.Services.Catalog.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AlbyOnContainers.Services.Catalog.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AlbyOnContainers.Services.Catalog.dll"]