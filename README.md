# MQTT_NET_COMELIT
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
=======

## Overview

MQTT_NET_COMELIT is a .NET application that exposes Comelit home automation
devices through MQTT so they can easily be consumed by Home Assistant. The
program connects to a Comelit HUB, retrieves the device structure and publishes
states and configuration topics to a configured MQTT broker.

## Prerequisites

- [.NET 7 SDK](https://dotnet.microsoft.com/download)

## Build

Restore the NuGet packages and build the solution:

```bash
dotnet restore
dotnet build
```

## Configuration

Application settings are stored in `data/options.json`. Edit this file with the
credentials for your Comelit HUB and the MQTT broker address. A typical
configuration looks like:

```json
{
  "comelit-username": "your-username",
  "comelit-password": "your-password",
  "comelit-HubIPAddress": "your-IP",
  "comelit-HubMACAddress": "your-hub-MAC",
  "comelit-ROOTelement": "GEN#17#13#1",
  "polling": 60,
  "mosquitto-username": "mosquitto-username",
  "mosquitto-password": "mosquitto-password",
  "mosquitto-IPAddress": "mosquitto-ip",
  "init-device-config": true
}
```

## Running

Once configured you can start the application with:

```bash
dotnet run
```

## Tests

Run the unit tests using the following command:

```bash
dotnet test
```

