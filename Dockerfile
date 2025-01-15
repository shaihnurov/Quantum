FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY Server/*.csproj ./Server/
RUN dotnet restore ./Server/Server.csproj

COPY . ./ 
WORKDIR /app/Server
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /out .

ENTRYPOINT ["dotnet", "Server.dll"]