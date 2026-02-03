FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["AssetManagement.Web/AssetManagement.Web.csproj", "AssetManagement.Web/"]
RUN dotnet restore "AssetManagement.Web/AssetManagement.Web.csproj"

COPY . .
WORKDIR "/src/AssetManagement.Web"
RUN dotnet publish "AssetManagement.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "AssetManagement.Web.dll"]
