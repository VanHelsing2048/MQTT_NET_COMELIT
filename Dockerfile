FROM mcr.microsoft.com/dotnet/runtime:8.0

ARG BUILD_VERSION
ARG BUILD_ARCH
ARG PUBLISH_DIR=publish/linux-x64

LABEL \
  io.hass.version="${BUILD_VERSION}" \
  io.hass.type="app" \
  io.hass.arch="${BUILD_ARCH}"

WORKDIR /app
COPY ${PUBLISH_DIR}/ .

ENTRYPOINT ["dotnet", "/app/MQTT_NET_COMELIT.dll"]
