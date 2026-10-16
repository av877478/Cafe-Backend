
# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Cafe.Web/Cafe.Web.csproj", "Cafe.Web/"]
COPY ["Cafe.Data/Cafe.Data.csproj", "Cafe.Data/"]
COPY ["Cafe.Model/Cafe.Model.csproj", "Cafe.Model/"]
COPY ["Cafe.Repository/Cafe.Repository.csproj", "Cafe.Repository/"]
COPY ["Cafe.Service/Cafe.Service.csproj", "Cafe.Service/"]
RUN dotnet restore "./Cafe.Web/Cafe.Web.csproj"
COPY . .
WORKDIR "/src/Cafe.Web"
RUN dotnet build "./Cafe.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Cafe.Web.csproj" 
-c $BUILD_CONFIGURATION -o /app/publish
/p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cafe.Web.dll"]