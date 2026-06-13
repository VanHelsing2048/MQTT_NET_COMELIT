# Changelog

## 1.1.14

- Add a synthetic `Senza area` area for supported top-level Comelit devices that do not match an existing area.
- Expose generic Comelit `RULE` objects as MQTT binary sensors instead of discarding them when they are not alarm rules.
- Add dedicated Home Assistant MQTT sensors for climate current humidity and target humidity so both values can keep numeric history.
- Update README topic documentation for generic rules and climate humidity sensors.

## 1.1.13

- Re-publish the current known Home Assistant MQTT states after the Home Assistant MQTT client reconnects.
- Re-publish the current known Home Assistant MQTT states after the Comelit structure is rebuilt during login or reconnect.
- Keep device state topics non-retained while still refreshing rarely used devices after reconnects.
- Refresh the global Comelit availability topic when the state resync confirms that Comelit is logged in.
