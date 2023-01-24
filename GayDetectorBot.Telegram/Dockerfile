#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["GayDetectorBot.Telegram/GayDetectorBot.Telegram.csproj", "GayDetectorBot.Telegram/"]
RUN dotnet restore "GayDetectorBot.Telegram/GayDetectorBot.Telegram.csproj"
COPY . .
WORKDIR "/src/GayDetectorBot.Telegram"
RUN dotnet build "GayDetectorBot.Telegram.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GayDetectorBot.Telegram.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GayDetectorBot.Telegram.dll"]