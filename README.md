# MQTT NET Comelit

.NET bridge between a Comelit home automation system and Home Assistant over MQTT.

The application connects to the Comelit HUB, reads the home structure, publishes MQTT Discovery configuration and state topics for Home Assistant, and forwards commands from Home Assistant back to the Comelit system.

## Main Features

- MQTT connection to the Comelit HUB using IP address, MAC address, and Comelit credentials.
- Automatic Home Assistant entity creation through MQTT Discovery.
- State synchronization on startup and after MQTT/Comelit reconnects.
- MQTT command topic subscriptions to control Comelit devices from Home Assistant.
- Periodic polling and automatic reconnect logic for MQTT/Home Assistant.
- Configurable logging.

Supported devices:

- on/off lights;
- dimmers with 0-255 brightness;
- electric blinds/covers with open, close, stop, and position control;
- climate/thermostat devices with temperature, humidity, heat/cool/off mode, and automatic/manual controls;
- irrigation valves;
- switches and digital outputs;
- temporary buttons and automations;
- power/water sensors and input binary sensors;
- supported Comelit alarm rule sensors.

## Requirements

- .NET 8 SDK for local build and development.
- MQTT broker reachable by Home Assistant.
- Comelit HUB reachable on the local network.
- Valid Comelit credentials and HUB identifiers.

## Home Assistant Add-on

This repository includes the metadata required to run as a Home Assistant add-on:

- `repository.yaml`: add-on repository definition;
- `config.yaml`: add-on name, version, supported architectures, Docker image, and options schema;
- `Dockerfile`: .NET 8 runtime image used to run the published application.

When running as an add-on, Home Assistant provides the configuration at `/data/options.json`. When running locally, the application falls back to `data/options.json`.

## Configuration

Complete example:

```json
{
  "comelit-username": "your-comelit-username",
  "comelit-password": "your-comelit-password",
  "comelit-HubIPAddress": "192.168.1.100",
  "comelit-HubMACAddress": "001122334455",
  "comelit-ROOTelement": "GEN#17#13#1",
  "polling": 60,
  "mosquitto-username": "your-mqtt-username",
  "mosquitto-password": "your-mqtt-password",
  "mosquitto-IPAddress": "192.168.1.10",
  "init-device-config": true,
  "log-level": "info"
}
```

Options:

| Field | Description |
| --- | --- |
| `comelit-username` | Username used to authenticate with the Comelit HUB. |
| `comelit-password` | Comelit password. |
| `comelit-HubIPAddress` | Comelit HUB IP address. |
| `comelit-HubMACAddress` | HUB MAC address, without separators. |
| `comelit-ROOTelement` | Root Comelit element used to read the home structure. |
| `polling` | Polling interval in seconds. |
| `mosquitto-username` | MQTT broker username used by Home Assistant. |
| `mosquitto-password` | MQTT broker password. |
| `mosquitto-IPAddress` | MQTT broker IP address. |
| `init-device-config` | When `true`, publishes MQTT Discovery payloads and is then reset to `false`. |
| `log-level` | Log level: `debug`, `info`, `warning`, `error`. |

To create or refresh entities in Home Assistant, set `init-device-config` to `true` and restart the app/add-on. After the discovery payloads are published, the value is saved back as `false`.

## MQTT Topics

Application topics use the `home` prefix. MQTT Discovery topics use `homeassistant`.

Examples:

| Type | Command | State | Discovery |
| --- | --- | --- | --- |
| Light | `home/lights/<id>/set` | `home/lights/<id>/state` | `homeassistant/light/<id>/config` |
| Dimmer | `home/lights/<id>/brightness/set` | `home/lights/<id>/brightness/state` | `homeassistant/light/<id>/config` |
| Cover | `home/cover/<id>/set` | `home/cover/<id>/state` | `homeassistant/cover/<id>/config` |
| Cover position | `home/cover/<id>/position/set` | `home/cover/<id>/position/state` | `homeassistant/cover/<id>/config` |
| Climate | `home/climate/<id>/mode/set` | `home/climate/<id>/mode/state` | `homeassistant/climate/<id>/config` |
| Sensor | - | `home/sensor/<id>/state` | `homeassistant/sensor/<id>/config` |
| Input | - | `home/inputs/<id>/state` | `homeassistant/binary_sensor/<id>/config` |

Comelit IDs are normalized before being used in MQTT topics.

## Local Run

Edit `data/options.json`, then run:

```bash
dotnet restore
dotnet build
dotnet run
```

Startup flow:

1. load configuration;
2. connect to the MQTT broker/Home Assistant;
3. connect to the Comelit HUB;
4. publish discovery payloads and current device states;
5. re-publish known states after MQTT/Comelit reconnects;
6. keep listening for MQTT commands and Comelit state updates.

## Tests

```bash
dotnet test
```

The test suite covers utilities, device configuration, Home Assistant discovery, and Comelit MQTT logic.

## Development

Main project structure:

- `Program.cs`: bootstrap, configuration loading, client startup, and initial publishing.
- `Comelit/`: connection, login, parsing, device models, and commands sent to Comelit.
- `HomeAssistant/`: MQTT client for Home Assistant, subscriptions, and command routing.
- `Static/ConfigData.cs`: JSON options mapping.
- `tests/`: MSTest test project.
- `tools/Comelit.JsonProbe/`: helper tool for inspecting Comelit JSON payloads.

## Quick Troubleshooting

- Entities do not appear in Home Assistant: set `init-device-config` to `true`, restart, and verify that MQTT Discovery is enabled.
- Commands are not received: check MQTT broker IP/credentials, then inspect logs for `/set` topic subscriptions.
- HUB cannot be reached: verify IP address, MAC address, and network connectivity from the host/container running the app.
- States are not updating: check `polling`, Comelit login logs, MQTT broker availability, and reconnect logs.

## License

Distributed under the MIT License. See [LICENSE](LICENSE).
