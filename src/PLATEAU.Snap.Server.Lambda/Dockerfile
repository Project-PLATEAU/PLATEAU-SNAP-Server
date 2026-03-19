FROM public.ecr.aws/lambda/dotnet:8 AS base
WORKDIR /var/task

FROM eclipse-temurin:11-jdk AS impexp
WORKDIR /app
ARG IMPORTER_EXPORTER_VERSION=5.5.0
ARG ADE_MANAGER_VERSION=2.3.0
ARG IUR_ADE_VERSION=2.1.0
ARG CITYDB_VERSION=4.4.1

RUN apt-get update && \
    apt-get install -y wget unzip && \
    rm -rf /var/lib/apt/lists/*
RUN wget https://github.com/3dcitydb/importer-exporter/releases/download/v${IMPORTER_EXPORTER_VERSION}/3DCityDB-Importer-Exporter-${IMPORTER_EXPORTER_VERSION}.zip && \
    wget https://github.com/3dcitydb/plugin-ade-manager/releases/download/v${ADE_MANAGER_VERSION}/plugin-ade-manager-${ADE_MANAGER_VERSION}.zip && \
    wget https://github.com/3dcitydb/iur-ade-citydb/releases/download/v${IUR_ADE_VERSION}/iur-ade-${IUR_ADE_VERSION}.zip && \
    wget https://github.com/3dcitydb/3dcitydb/releases/download/v${CITYDB_VERSION}/3dcitydb-${CITYDB_VERSION}.zip
RUN unzip 3DCityDB-Importer-Exporter-${IMPORTER_EXPORTER_VERSION}.zip && \
    unzip plugin-ade-manager-${ADE_MANAGER_VERSION}.zip && \
    unzip iur-ade-${IUR_ADE_VERSION}.zip && \
    unzip 3dcitydb-${CITYDB_VERSION}.zip && \
    mv 3DCityDB-Importer-Exporter-${IMPORTER_EXPORTER_VERSION} 3DCityDB-Importer-Exporter && \
    mv plugin-ade-manager-${ADE_MANAGER_VERSION} 3DCityDB-Importer-Exporter/plugins/plugin-ade-manager && \
    mv iur-ade-${IUR_ADE_VERSION} 3DCityDB-Importer-Exporter/ade-extensions/iur-ade && \
    mv 3dcitydb-${CITYDB_VERSION} 3DCityDB-Importer-Exporter/3dcitydb && \
    rm -f *.zip
RUN echo $JAVA_HOME

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PLATEAU.Snap.Server.Lambda/PLATEAU.Snap.Server.Lambda.csproj", "PLATEAU.Snap.Server.Lambda/"]
COPY ["PLATEAU.Snap.Models/PLATEAU.Snap.Models.csproj", "PLATEAU.Snap.Models/"]
COPY ["PLATEAU.Snap.Server.Entities/PLATEAU.Snap.Server.Entities.csproj", "PLATEAU.Snap.Server.Entities/"]
COPY ["PLATEAU.Snap.Server.Repositories.PostgreSQL/PLATEAU.Snap.Server.Repositories.PostgreSQL.csproj", "PLATEAU.Snap.Server.Repositories.PostgreSQL/"]
COPY ["PLATEAU.Snap.Server.Repositories/PLATEAU.Snap.Server.Repositories.csproj", "PLATEAU.Snap.Server.Repositories/"]
COPY ["PLATEAU.Snap.Server.Repositories.S3/PLATEAU.Snap.Server.Repositories.S3.csproj", "PLATEAU.Snap.Server.Repositories.S3/"]
COPY ["PLATEAU.Snap.Server.Services.Impl/PLATEAU.Snap.Server.Services.Impl.csproj", "PLATEAU.Snap.Server.Services.Impl/"]
COPY ["PLATEAU.Snap.Server.Services/PLATEAU.Snap.Server.Services.csproj", "PLATEAU.Snap.Server.Services/"]
RUN dotnet restore "./PLATEAU.Snap.Server.Lambda/PLATEAU.Snap.Server.Lambda.csproj"
COPY . .
RUN dotnet build "./PLATEAU.Snap.Server.Lambda/PLATEAU.Snap.Server.Lambda.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM base AS final
WORKDIR /var/task

RUN microdnf update -y && microdnf install -y findutils && microdnf clean all

ENV JAVA_HOME=/opt/java/openjdk
ENV PATH="$JAVA_HOME/bin:$PATH"
COPY --from=impexp $JAVA_HOME $JAVA_HOME
COPY --from=impexp /app/3DCityDB-Importer-Exporter /app/3DCityDB-Importer-Exporter
COPY --from=build /app/build .