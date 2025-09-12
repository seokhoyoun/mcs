# 디버그 컨테이너를 사용자 지정하는 방법과 Visual Studio 이 Dockerfile을 사용하여 더 빠른 디버깅을 위해 이미지를 빌드하는 방법을 알아보려면 https://aka.ms/customizecontainer를 참조하세요.

# 이 스테이지는 VS에서 빠른 모드로 실행할 때 사용됩니다(디버그 구성의 기본값).
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# 이 스테이지는 서비스 프로젝트를 빌드하는 데 사용됩니다.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.build.props", "."]
COPY ["src/apps/Nexus.Portal/Nexus.Portal.csproj", "src/apps/Nexus.Portal/"]
COPY ["src/libs/Nexus.Core/Nexus.Core.csproj", "src/libs/Nexus.Core/"]
COPY ["src/libs/Nexus.Infrastructure/Nexus.Infrastructure.csproj", "src/libs/Nexus.Infrastructure/"]
RUN dotnet restore "./src/apps/Nexus.Portal/Nexus.Portal.csproj"
COPY . .
WORKDIR "/src/src/apps/Nexus.Portal"
RUN dotnet build "./Nexus.Portal.csproj" -c $BUILD_CONFIGURATION -o /app/build

# 이 스테이지는 최종 스테이지에 복사할 서비스 프로젝트를 게시하는 데 사용됩니다.
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Nexus.Portal.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 이 스테이지는 프로덕션에서 사용되거나 VS에서 일반 모드로 실행할 때 사용됩니다(디버그 구성을 사용하지 않는 경우 기본값).
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Nexus.Portal.dll"]