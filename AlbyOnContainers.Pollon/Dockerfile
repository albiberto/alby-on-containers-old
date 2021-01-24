#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["AlbyOnContainers.Pollon/AlbyOnContainers.Pollon.csproj", "AlbyOnContainers.Pollon/"]
RUN dotnet restore "AlbyOnContainers.Pollon/AlbyOnContainers.Pollon.csproj"
COPY . .
WORKDIR "/src/AlbyOnContainers.Pollon"
RUN dotnet build "AlbyOnContainers.Pollon.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AlbyOnContainers.Pollon.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AlbyOnContainers.Pollon.dll"]