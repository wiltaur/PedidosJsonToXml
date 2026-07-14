# Esta etapa se utiliza al ejecutar desde VS en modo rápido (predeterminado para la configuración de depuración)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Esta etapa se utiliza para construir el proyecto de servicio.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PedidosJsonToXml.Api/PedidosJsonToXml.Api.csproj", "PedidosJsonToXml.Api/"]
RUN dotnet restore "./PedidosJsonToXml.Api/PedidosJsonToXml.Api.csproj"
COPY . .
WORKDIR "/src/PedidosJsonToXml.Api"
RUN dotnet build "./PedidosJsonToXml.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta etapa se utiliza para publicar el proyecto de servicio que se copiará a la etapa final.
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PedidosJsonToXml.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Esta etapa se utiliza en producción o al ejecutar desde VS en modo normal (predeterminado cuando no se utiliza la configuración de depuración).
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PedidosJsonToXml.Api.dll"]