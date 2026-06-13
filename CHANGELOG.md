# Changelog

## 1.1.12

- Re-publish the current known Home Assistant MQTT states after the Home Assistant MQTT client reconnects.
- Re-publish the current known Home Assistant MQTT states after the Comelit structure is rebuilt during login or reconnect.
- Keep device state topics non-retained while still refreshing rarely used devices after reconnects.
- Refresh the global Comelit availability topic when the state resync confirms that Comelit is logged in.
