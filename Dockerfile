FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["StockServer.csproj", ""]
RUN dotnet restore "./StockServer.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "StockServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StockServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StockServer.dll"]