FROM mcr.microsoft.com/dotnet/runtime:8.0
RUN mkdir app
COPY /* /app/
ENTRYPOINT ["dotnet", "/app/MQTT_NET_COMELIT.dll"]