using System.Collections.Generic;
using static MQTT_NET_COMELIT.Utility.Utility;

namespace MQTT_NET_COMELIT.Comelit
{
    /// <summary>
    /// Tracks device state snapshots to detect changes and avoid redundant MQTT publishes.
    /// Implements bidirectional synchronization between Comelit and Home Assistant.
    /// </summary>
    public class DeviceStateManager
    {
        // State snapshots: DeviceID -> state snapshot
        private Dictionary<string, DeviceStateSnapshot> _stateSnapshots = new();

        /// <summary>
        /// Represents a device state snapshot for change detection
        /// </summary>
        public class DeviceStateSnapshot
        {
            public string Status { get; set; } // ON/OFF/IDLE
            public string Brightness { get; set; } // For dimmers
            public string Position { get; set; } // For blinds (0-100)
            public string Temperature { get; set; } // For clima
            public string Humidity { get; set; } // For clima
            public string PowerSt { get; set; } // For clima mode
            public string EstInv { get; set; } // For clima heat/cool
            public string AutoMan { get; set; } // For clima preset
            public DateTime LastUpdate { get; set; }

            /// <summary>
            /// Create snapshot from a Device
            /// </summary>
            public static DeviceStateSnapshot FromDevice(Device device)
            {
                var snapshot = new DeviceStateSnapshot
                {
                    Status = device.Status,
                    LastUpdate = DateTime.UtcNow
                };

                if (device is DimmerLight dimmer)
                {
                    snapshot.Brightness = dimmer.Bright;
                }
                else if (device is ElectricBlind blind)
                {
                    snapshot.Position = blind.OpenStatus;
                }
                else if (device is Clima clima)
                {
                    snapshot.Temperature = clima.Temperatura;
                    snapshot.Humidity = clima.Umidita;
                    snapshot.PowerSt = clima.PowerSt;
                    snapshot.EstInv = clima.EstInv;
                    snapshot.AutoMan = clima.AutoMan;
                }

                return snapshot;
            }

            /// <summary>
            /// Compare with current device state; return true if any property changed
            /// </summary>
            public bool IsDifferentFrom(Device device)
            {
                // Check status
                if (Status != device.Status)
                    return true;

                // Check device-specific properties
                if (device is DimmerLight dimmer)
                {
                    if (Brightness != dimmer.Bright)
                        return true;
                }
                else if (device is ElectricBlind blind)
                {
                    if (Position != blind.OpenStatus)
                        return true;
                }
                else if (device is Clima clima)
                {
                    // For temperature and humidity, use tolerance (±0.1) for floating point comparison
                    if (!AreTemperaturesEqual(Temperature, clima.Temperatura))
                        return true;
                    if (!AreHumiditiesEqual(Humidity, clima.Umidita))
                        return true;
                    if (PowerSt != clima.PowerSt)
                        return true;
                    if (EstInv != clima.EstInv)
                        return true;
                    if (AutoMan != clima.AutoMan)
                        return true;
                }

                return false;
            }

            private static bool AreTemperaturesEqual(string snap, string current)
            {
                if (string.IsNullOrEmpty(snap) && string.IsNullOrEmpty(current))
                    return true;
                if (string.IsNullOrEmpty(snap) || string.IsNullOrEmpty(current))
                    return false;
                if (double.TryParse(snap, out var snapTemp) && double.TryParse(current, out var currTemp))
                {
                    return Math.Abs(snapTemp - currTemp) < 0.1;
                }
                return snap == current;
            }

            private static bool AreHumiditiesEqual(string snap, string current)
            {
                return AreTemperaturesEqual(snap, current); // Same logic: tolerance for floats
            }
        }

        /// <summary>
        /// Check if device state has changed since last snapshot.
        /// If changed, updates the snapshot and returns true.
        /// If unchanged, returns false.
        /// Logs transitions for debugging.
        /// </summary>
        public bool HasChanged(Device device)
        {
            if (device == null)
                return false;

            string deviceId = device.ID;

            // If no previous snapshot, consider it a change
            if (!_stateSnapshots.TryGetValue(deviceId, out var prevSnapshot))
            {
                var newSnapshot = DeviceStateSnapshot.FromDevice(device);
                _stateSnapshots[deviceId] = newSnapshot;
                WriteLog($"[STATE] Device {deviceId} ({device.SubType}): Initial snapshot created - Status={newSnapshot.Status}, Bright={newSnapshot.Brightness}, Pos={newSnapshot.Position}, Temp={newSnapshot.Temperature}, Hum={newSnapshot.Humidity}", LogLevel.Debug);
                return true; // First time seeing this device
            }

            // Check if state is different and log transitions
            bool changed = prevSnapshot.IsDifferentFrom(device);

            if (changed)
            {
                var newSnapshot = DeviceStateSnapshot.FromDevice(device);

                // Log state transitions
                if (prevSnapshot.Status != newSnapshot.Status)
                    WriteLog($"[TRANSITION] Device {deviceId}: Status {prevSnapshot.Status} -> {newSnapshot.Status}", LogLevel.Debug);

                if (device is DimmerLight && prevSnapshot.Brightness != newSnapshot.Brightness)
                    WriteLog($"[TRANSITION] Device {deviceId}: Brightness {prevSnapshot.Brightness} -> {newSnapshot.Brightness}", LogLevel.Debug);

                if (device is ElectricBlind && prevSnapshot.Position != newSnapshot.Position)
                    WriteLog($"[TRANSITION] Device {deviceId}: Position {prevSnapshot.Position}% -> {newSnapshot.Position}%", LogLevel.Debug);

                if (device is Clima)
                {
                    if (prevSnapshot.Temperature != newSnapshot.Temperature)
                        WriteLog($"[TRANSITION] Device {deviceId}: Temperature {prevSnapshot.Temperature}°C -> {newSnapshot.Temperature}°C");
                    if (prevSnapshot.Humidity != newSnapshot.Humidity)
                        WriteLog($"[TRANSITION] Device {deviceId}: Humidity {prevSnapshot.Humidity}% -> {newSnapshot.Humidity}%", LogLevel.Debug);
                    if (prevSnapshot.PowerSt != newSnapshot.PowerSt)
                        WriteLog($"[TRANSITION] Device {deviceId}: PowerSt {prevSnapshot.PowerSt} -> {newSnapshot.PowerSt}", LogLevel.Debug);
                    if (prevSnapshot.EstInv != newSnapshot.EstInv)
                        WriteLog($"[TRANSITION] Device {deviceId}: EstInv {prevSnapshot.EstInv} -> {newSnapshot.EstInv}", LogLevel.Debug);
                    if (prevSnapshot.AutoMan != newSnapshot.AutoMan)
                        WriteLog($"[TRANSITION] Device {deviceId}: AutoMan {prevSnapshot.AutoMan} -> {newSnapshot.AutoMan}", LogLevel.Debug);
                }

                _stateSnapshots[deviceId] = newSnapshot;
            }

            return changed;
        }

        /// <summary>
        /// Get the current snapshot for a device (or null if not yet tracked)
        /// </summary>
        public DeviceStateSnapshot GetSnapshot(Device device)
        {
            _stateSnapshots.TryGetValue(device?.ID, out var snapshot);
            return snapshot;
        }

        /// <summary>
        /// Clear all snapshots (useful on reconnect or reset)
        /// </summary>
        public void ClearAllSnapshots()
        {
            _stateSnapshots.Clear();
            WriteLog("DeviceStateManager: All state snapshots cleared", LogLevel.Debug);
        }

        /// <summary>
        /// Get count of tracked devices (for diagnostics)
        /// </summary>
        public int TrackedDeviceCount => _stateSnapshots.Count;
    }
}
