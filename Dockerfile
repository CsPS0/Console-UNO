FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY uno-game/uno-game/uno-game.csproj uno-game/uno-game/
RUN dotnet restore uno-game/uno-game/uno-game.csproj

COPY uno-game/ uno-game/
WORKDIR /src/uno-game/uno-game
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "uno-game.dll"]
