FROM mcr.microsoft.com/dotnet/runtime:7.0
RUN mkdir app
COPY /* /app/
ENTRYPOINT ["dotnet", "/app/MQTT_NET_COMELIT.dll"]