FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Stash.App/Stash.App.fsproj", "Stash.App/"]
RUN dotnet restore "Stash.App/Stash.App.fsproj"
COPY . .
WORKDIR "/src/Stash.App"
RUN dotnet build "Stash.App.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Stash.App.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Stash.App.dll"]
