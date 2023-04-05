#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# build Console
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["zoka-test-suite.Console/zoka-test-suite.Console.csproj", "zoka-test-suite.Console/"]
RUN dotnet restore "zoka-test-suite.Console/zoka-test-suite.Console.csproj"
COPY . .
WORKDIR "/src/zoka-test-suite.Console"
RUN dotnet build "zoka-test-suite.Console.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "zoka-test-suite.Console.csproj" -c Release -o /app/publish /p:UseAppHost=false

# build AssertionActions plugin
WORKDIR "/src/Plugins/AssertionActions"
RUN dotnet publish "zoka-test-suite.AssertionActions.csproj" -c Release -o /app/publish/Plugins /p:UseAppHost=false
# build BasicTestActions plugins
WORKDIR "/src/Plugins/BasicTestActions"
RUN dotnet publish "zoka-test-suite.BasicTestActions.csproj" -c Release -o /app/publish/Plugins /p:UseAppHost=false
# build HttpTestActions plugins
WORKDIR "/src/Plugins/HttpTestActions"
RUN dotnet publish "zoka-test-suite.HttpTestActions.csproj" -c Release -o /app/publish/Plugins /p:UseAppHost=false
# build DataStorageActions plugins
WORKDIR "/src/Plugins/DataStorageActions"
RUN dotnet publish "zoka-test-suite.DataStorageActions.csproj" -c Release -o /app/publish/Plugins /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
