using static MQTT_NET_COMELIT.Comelit.JsonParsing;
using static MQTT_NET_COMELIT.Utility.Utility;

namespace MQTT_NET_COMELIT.Comelit
{
    public partial class MQTTComelit
    {
        public Home HomeStructure { get; private set; }

        private static readonly Dictionary<string, string> AreaNameAliases = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Bagno Primo piano"] = "Bagno PP"
        };

        private void CreateStructure(Header root)
        {
            List<Element> rootElements = root?.OutData?.FirstOrDefault()?.Elements ?? [];
            ElementData casa = rootElements
                .Select(element => element.Data)
                .FirstOrDefault(data => data != null && IsZone(data));

            if (casa == null)
            {
                WriteLog("Unable to create Comelit structure: root home element not found", LogLevel.Error);
                return;
            }

            HomeStructure = new Home()
            {
                ID = casa.ID,
                Description = casa.Description,
                Name = casa.Description,
                Type = casa.Type,
                SubType = casa.SubType,
                Areas = []
            };

            Dictionary<string, Area> areaById = new(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, Area> areaByName = new(StringComparer.OrdinalIgnoreCase);

            foreach (Element element in casa.Elements ?? [])
            {
                AddStructureElement(element.Data, areaById, areaByName);
            }

            foreach (Element element in rootElements)
            {
                ElementData data = element.Data;
                if (data == null || string.Equals(data.ID, casa.ID, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                AddStructureElement(data, areaById, areaByName);
            }

            LogStructureDiagnostics();
        }

        private void AddStructureElement(ElementData data, Dictionary<string, Area> areaById, Dictionary<string, Area> areaByName)
        {
            if (data == null)
            {
                return;
            }

            if (IsZone(data))
            {
                AddAreasFromZone(data, areaById, areaByName);
            }
            else
            {
                AddTopLevelDevice(data, areaById, areaByName);
            }
        }

        private void AddAreasFromZone(ElementData zone, Dictionary<string, Area> areaById, Dictionary<string, Area> areaByName)
        {
            List<ElementData> children = (zone.Elements ?? [])
                .Where(element => element.Data != null)
                .Select(element => element.Data)
                .ToList();

            List<ElementData> childZones = children.Where(IsZone).ToList();
            List<ElementData> devices = children.Where(child => !IsZone(child)).ToList();

            if (devices.Count > 0)
            {
                Area area = GetOrCreateArea(zone, areaById, areaByName);
                foreach (ElementData device in devices)
                {
                    AddDeviceToArea(area, device);
                }
            }

            foreach (ElementData childZone in childZones)
            {
                AddAreasFromZone(childZone, areaById, areaByName);
            }
        }

        private void AddTopLevelDevice(ElementData device, Dictionary<string, Area> areaById, Dictionary<string, Area> areaByName)
        {
            Area area = FindAreaForDevice(device, areaById, areaByName);

            if (area == null)
            {
                if (device.Type == Enums.OBJECT_TYPE.INPUT)
                {
                    area = GetOrCreateSyntheticArea("DOM#IN", "Ingressi", device.Type, device.SubType, areaById, areaByName);
                }
                else if (IsAlarmRule(device))
                {
                    area = GetOrCreateSyntheticArea("DOM#ALM", "Allarme", device.Type, device.SubType, areaById, areaByName);
                }
                else if (IsSystemSensor(device))
                {
                    area = GetOrCreateSyntheticArea("DOM#SYS", "Sistema", device.Type, device.SubType, areaById, areaByName);
                }
            }

            if (area == null)
            {
                WriteUnsupportedDevice(device, "No matching area");
                return;
            }

            AddDeviceToArea(area, device);
        }

        private Area FindAreaForDevice(ElementData device, Dictionary<string, Area> areaById, Dictionary<string, Area> areaByName)
        {
            if (!string.IsNullOrWhiteSpace(device.PlaceID)
                && !string.Equals(device.PlaceID, HomeStructure.ID, StringComparison.OrdinalIgnoreCase)
                && areaById.TryGetValue(device.PlaceID, out Area areaByIdMatch))
            {
                return areaByIdMatch;
            }

            string description = device.Description ?? string.Empty;
            if (AreaNameAliases.TryGetValue(description, out string alias))
            {
                description = alias;
            }

            areaByName.TryGetValue(NormalizeAreaName(description), out Area areaByNameMatch);
            return areaByNameMatch;
        }

        private Area GetOrCreateArea(ElementData zone, Dictionary<string, Area> areaById, Dictionary<string, Area> areaByName)
        {
            if (!string.IsNullOrWhiteSpace(zone.ID) && areaById.TryGetValue(zone.ID, out Area existingById))
            {
                return existingById;
            }

            Area area = new()
            {
                ID = zone.ID,
                Description = zone.Description,
                Name = zone.Description,
                Type = zone.Type,
                SubType = zone.SubType,
                Devices = []
            };

            HomeStructure.Areas.Add(area);
            IndexArea(area, areaById, areaByName);
            return area;
        }

        private Area GetOrCreateSyntheticArea(
            string id,
            string description,
            Enums.OBJECT_TYPE type,
            Enums.OBJECT_SUBTYPE subType,
            Dictionary<string, Area> areaById,
            Dictionary<string, Area> areaByName)
        {
            if (areaById.TryGetValue(id, out Area existing))
            {
                return existing;
            }

            Area area = new()
            {
                ID = id,
                Description = description,
                Name = description,
                Type = type,
                SubType = subType,
                Devices = []
            };

            HomeStructure.Areas.Add(area);
            IndexArea(area, areaById, areaByName);
            return area;
        }

        private void AddDeviceToArea(Area area, ElementData deviceData)
        {
            Device device = CreateDeviceBinding(deviceData, area.Description);
            if (device == null)
            {
                WriteUnsupportedDevice(deviceData, $"Area '{area.Description}'");
                return;
            }

            area.Devices.Add(device);
        }

        private static void IndexArea(Area area, Dictionary<string, Area> areaById, Dictionary<string, Area> areaByName)
        {
            if (!string.IsNullOrWhiteSpace(area.ID))
            {
                areaById[area.ID] = area;
            }

            if (!string.IsNullOrWhiteSpace(area.Description))
            {
                areaByName[NormalizeAreaName(area.Description)] = area;
            }
        }

        private static bool IsZone(ElementData data)
        {
            return data.Type == Enums.OBJECT_TYPE.ZONE;
        }

        private static bool IsSystemSensor(ElementData data)
        {
            return data.Type == Enums.OBJECT_TYPE.POWER_SUPPLIER
                || data.Type == Enums.OBJECT_TYPE.OUTLET;
        }

        private static bool IsAlarmRule(ElementData data)
        {
            return data.Type == Enums.OBJECT_TYPE.RULE
                && data.RuleAtoms?.Any(atom => atom.ObjID?.StartsWith("ALM#ZN#", StringComparison.OrdinalIgnoreCase) == true) == true;
        }

        private static string NormalizeAreaName(string value)
        {
            return string.Join(' ', (value ?? string.Empty).Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries)).ToUpperInvariant();
        }

        private static void WriteUnsupportedDevice(ElementData device, string context)
        {
            WriteLog($"Unsupported Comelit device ({context}): id={device.ID}, type={(int)device.Type}/{device.Type}, sub_type={(int)device.SubType}/{device.SubType}, description='{device.Description}'", LogLevel.Warning);
        }

        private void LogStructureDiagnostics()
        {
            int deviceCount = HomeStructure.Areas.Sum(area => area.Devices?.Count ?? 0);
            WriteLog($"Comelit structure summary: {HomeStructure.Areas.Count} areas, {deviceCount} devices");

            foreach (Area area in HomeStructure.Areas.OrderBy(area => area.Description))
            {
                List<Device> devices = area.Devices ?? [];
                string details = string.Join(", ", devices
                    .GroupBy(device => $"{device.Type}/{device.SubType}")
                    .OrderBy(group => group.Key)
                    .Select(group => $"{group.Key}:{group.Count()}"));

                WriteLog($"Comelit area '{area.Description}' ({area.ID}): {devices.Count} devices" + (string.IsNullOrEmpty(details) ? string.Empty : $" [{details}]"), LogLevel.Debug);
            }
        }
    }
}
