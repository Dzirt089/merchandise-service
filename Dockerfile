# Берет базовый образ, с которым начнёт работать докер
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Устанавливает внутри докер-контейнера базовую директорию, все команды после этой записи будут работать относительно этой директории. Если её нет - то она создатся
WORKDIR /src

# Копируем проект в докер, указывая какой проект необходим и куда
COPY ["src/OzonEdu.MerchandiseService/OzonEdu.MerchandiseService.csproj","src/OzonEdu.MerchandiseService/"]

# Обновляем зависимости в проекте
RUN dotnet restore "./src/OzonEdu.MerchandiseService/OzonEdu.MerchandiseService.csproj"

# Копирует всё из всё из указанной директории физического расположенного проекта, в директорию докера
COPY . .

# Обновляем текущую директорию, чтобы могли билдить наш проект в докере
WORKDIR "/src/src/OzonEdu.MerchandiseService"

# Запускаем комманду Ран для билдинга
RUN dotnet build "OzonEdu.MerchandiseService.csproj" -c Release -o /app/build

# Публикуем из образа билд в publish. Так сказать ренейминг. В новую папку publish
FROM build AS publish
RUN dotnet publish "OzonEdu.MerchandiseService.csproj" -c Release -o /app/publish
COPY "entrypoint.sh" "/app/publish/."

# Берем новый образ .Net для запуска в нём нашего приложения. 
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS runtime

# Создаём новую папку в новом контейнере
WORKDIR /app

# Какие порты будут выводить
EXPOSE 80
EXPOSE 443

# Делаем финальный образ на базе runtime
FROM runtime AS final

# Создаём новую папку в новом образе
WORKDIR /app

# Копируем из publish образа, из папки в нём /app/publish в текущий контекст ( . ) final образа
COPY --from=publish /app/publish .

# Точка, из которой контейнер будет исполняться
#ENTRYPOINT ["dotnet","OzonEdu.MerchandiseService.dll"]

# даём права
RUN chmod +x entrypoint.sh

# Запуск скрипта напрямую (JSON-массив для корректной обработки сигналов)
CMD ["/bin/bash", "entrypoint.sh"]