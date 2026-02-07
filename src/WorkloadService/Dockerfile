FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY . .

RUN rm -rf src/Database/appsettings.*

RUN dotnet restore src/WorkloadService/WorkloadService.csproj

RUN dotnet publish src/WorkloadService/WorkloadService.csproj \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WorkloadService.dll"]
