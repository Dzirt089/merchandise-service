FROM mcr.microsoft.com/dotnet/sdk:9.0 AS restore
WORKDIR /src

COPY ["src/OzonEdu.MerchandiseService/OzonEdu.MerchandiseService.csproj", "src/OzonEdu.MerchandiseService/"]
COPY ["src/OzonEdu.Infrastructure/OzonEdu.MerchandiseService.Infrastructure.csproj", "src/OzonEdu.Infrastructure/"]
COPY ["src/OzonEdu.MerchandiseService.Application/OzonEdu.MerchandiseService.Application.csproj", "src/OzonEdu.MerchandiseService.Application/"]
COPY ["src/OzonEdu.MerchandiseService.DataAccess.EntityFramework/OzonEdu.MerchandiseService.DataAccess.EntityFramework.csproj", "src/OzonEdu.MerchandiseService.DataAccess.EntityFramework/"]
COPY ["src/OzonEdu.MerchandiseService.Domain/OzonEdu.MerchandiseService.Domain.csproj", "src/OzonEdu.MerchandiseService.Domain/"]
COPY ["src/OzonEdu.MerchandiseService.Grpc/OzonEdu.MerchandiseService.Grpc.csproj", "src/OzonEdu.MerchandiseService.Grpc/"]
COPY ["src/OzonEdu.MerchandiseService.Migrator/OzonEdu.MerchandiseService.Migrator.csproj", "src/OzonEdu.MerchandiseService.Migrator/"]
COPY ["tests/OzonEdu.MerchandiseService.E2ETests/OzonEdu.MerchandiseService.E2ETests.csproj", "tests/OzonEdu.MerchandiseService.E2ETests/"]

RUN dotnet restore "src/OzonEdu.MerchandiseService/OzonEdu.MerchandiseService.csproj"
RUN dotnet restore "tests/OzonEdu.MerchandiseService.E2ETests/OzonEdu.MerchandiseService.E2ETests.csproj"

COPY . .

FROM restore AS build
RUN dotnet build "src/OzonEdu.MerchandiseService/OzonEdu.MerchandiseService.csproj" -c Release --no-restore

FROM build AS publish
RUN dotnet publish "src/OzonEdu.MerchandiseService/OzonEdu.MerchandiseService.csproj" -c Release -o /app/publish --no-build

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends bash curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .
COPY entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh

ENTRYPOINT ["/bin/bash", "/app/entrypoint.sh"]
