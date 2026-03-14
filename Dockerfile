FROM mcr.microsoft.com/dotnet/sdk:9.0 AS restore
WORKDIR /src

COPY ["src/OzonEdu.MerchandiseService/OzonEdu.MerchandiseService.csproj", "src/OzonEdu.MerchandiseService/"]
COPY ["tests/OzonEdu.MerchandiseService.E2ETests/OzonEdu.MerchandiseService.E2ETests.csproj", "tests/OzonEdu.MerchandiseService.E2ETests/"]

RUN dotnet restore "src/OzonEdu.MerchandiseService/OzonEdu.MerchandiseService.csproj"
RUN dotnet restore "tests/OzonEdu.MerchandiseService.E2ETests/OzonEdu.MerchandiseService.E2ETests.csproj"

COPY . .

FROM restore AS build
RUN dotnet build "src/OzonEdu.MerchandiseService/OzonEdu.MerchandiseService.csproj" -c Release --no-restore

FROM build AS publish
RUN dotnet publish "src/OzonEdu.MerchandiseService/OzonEdu.MerchandiseService.csproj" -c Release -o /app/publish --no-build

FROM restore AS test-runner
RUN apt-get update \
    && apt-get install -y --no-install-recommends bash curl \
    && rm -rf /var/lib/apt/lists/*
RUN dotnet build "tests/OzonEdu.MerchandiseService.E2ETests/OzonEdu.MerchandiseService.E2ETests.csproj" -c Release --no-restore
COPY entrypoint.e2e.sh /app/entrypoint.e2e.sh
RUN chmod +x /app/entrypoint.e2e.sh

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends bash curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .
COPY entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh

ENTRYPOINT ["/bin/bash", "/app/entrypoint.sh"]