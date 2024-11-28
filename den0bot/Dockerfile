#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:8.0-bookworm-slim AS base
WORKDIR /app

RUN apt-get update && apt-get -y install ffmpeg

FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build

WORKDIR /src
COPY . .
RUN dotnet restore "den0bot/den0bot.csproj"
WORKDIR "/src/den0bot"
RUN dotnet build "den0bot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "den0bot.csproj" -c Release -o /app/publish

FROM base AS final

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "den0bot.dll"]