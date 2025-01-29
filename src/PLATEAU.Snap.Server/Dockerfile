# デバッグ コンテナーをカスタマイズする方法と、Visual Studio がこの Dockerfile を使用してより高速なデバッグのためにイメージをビルドする方法については、https://aka.ms/customizecontainer をご覧ください。

# このステージは、VS から高速モードで実行するときに使用されます (デバッグ構成の既定値)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# このステージは、サービス プロジェクトのビルドに使用されます
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PLATEAU.Snap.Server/PLATEAU.Snap.Server.csproj", "PLATEAU.Snap.Server/"]
COPY ["PLATEAU.Snap.Models/PLATEAU.Snap.Models.csproj", "PLATEAU.Snap.Models/"]
COPY ["PLATEAU.Snap.Server.Entities/PLATEAU.Snap.Server.Entities.csproj", "PLATEAU.Snap.Server.Entities/"]
COPY ["PLATEAU.Snap.Server.Repositories.PostgreSQL/PLATEAU.Snap.Server.Repositories.PostgreSQL.csproj", "PLATEAU.Snap.Server.Repositories.PostgreSQL/"]
COPY ["PLATEAU.Snap.Server.Repositories/PLATEAU.Snap.Server.Repositories.csproj", "PLATEAU.Snap.Server.Repositories/"]
COPY ["PLATEAU.Snap.Server.Repositories.S3/PLATEAU.Snap.Server.Repositories.S3.csproj", "PLATEAU.Snap.Server.Repositories.S3/"]
COPY ["PLATEAU.Snap.Server.Services.Impl/PLATEAU.Snap.Server.Services.Impl.csproj", "PLATEAU.Snap.Server.Services.Impl/"]
COPY ["PLATEAU.Snap.Server.Services/PLATEAU.Snap.Server.Services.csproj", "PLATEAU.Snap.Server.Services/"]
RUN dotnet restore "./PLATEAU.Snap.Server/PLATEAU.Snap.Server.csproj"
COPY . .
WORKDIR "/src/PLATEAU.Snap.Server"
RUN dotnet build "./PLATEAU.Snap.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

# このステージは、最終ステージにコピーするサービス プロジェクトを公開するために使用されます
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PLATEAU.Snap.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# このステージは、運用環境または VS から通常モードで実行している場合に使用されます (デバッグ構成を使用しない場合の既定)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PLATEAU.Snap.Server.dll"]