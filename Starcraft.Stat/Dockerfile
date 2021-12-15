FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Starcraft.Stat/Starcraft.Stat.csproj", "Starcraft.Stat/"]
RUN dotnet restore "Starcraft.Stat/Starcraft.Stat.csproj"
COPY . .
WORKDIR "/src/Starcraft.Stat"
RUN dotnet build "Starcraft.Stat.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Starcraft.Stat.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Starcraft.Stat.dll"]
