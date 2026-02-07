FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY . .

RUN rm -rf src/Database/appsettings.*

RUN dotnet restore src/OperationalService/OperationalService.csproj

RUN dotnet publish src/OperationalService/OperationalService.csproj \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "OperationalService.dll"]
