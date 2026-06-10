FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
COPY . .
RUN dotnet publish MQTT_NET_COMELIT.csproj -c Release --self-contained false -o /app

FROM mcr.microsoft.com/dotnet/runtime:8.0

ARG BUILD_VERSION
ARG BUILD_ARCH

LABEL \
  io.hass.version="${BUILD_VERSION}" \
  io.hass.type="app" \
  io.hass.arch="${BUILD_ARCH}"

WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "/app/MQTT_NET_COMELIT.dll"]
