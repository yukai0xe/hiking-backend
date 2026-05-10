FROM mcr.microsoft.com/dotnet/sdk:9.0 AS restore

WORKDIR /src
COPY *.sln .
COPY hiking.WebApi/hiking.WebApi.csproj ./hiking.WebApi/
COPY hiking.Service/hiking.Service.csproj ./hiking.Service/
COPY hiking.Repository/hiking.Repository.csproj ./hiking.Repository/
RUN dotnet restore hiking-backend.sln --no-cache

FROM restore AS build
COPY . .
RUN dotnet publish ./hiking.WebApi/hiking.WebApi.csproj \
    -c Release \
    --no-restore \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "hiking.WebApi.dll"]