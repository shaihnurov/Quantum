# Укажите базовый образ .NET SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Копируем CSPROJ-файлы и восстанавливаем зависимости
COPY *.sln ./
COPY Server/*.csproj ./Server/
RUN dotnet restore

# Копируем весь проект и выполняем сборку
COPY . ./
WORKDIR /app/Server
RUN dotnet publish -c Release -o /out

# Укажите базовый образ .NET Runtime для выполнения приложения
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /out .

# Укажите команду для запуска приложения
ENTRYPOINT ["dotnet", "Server.dll"]