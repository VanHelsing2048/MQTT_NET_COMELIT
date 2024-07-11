using static MQTT_NET_COMELIT.Comelit.JsonParsing;
using MQTT_NET_COMELIT.Comelit.DevicesStructure;

namespace MQTT_NET_COMELIT.Comelit
{
    public partial class MQTTComelit
    {
        public Home HomeStructure { get; private set; }

        private void CreateStructure(Header root)
        {
            var casa = root.OutData[0].Elements[0].Data;
            HomeStructure = new Home()
            {
                ID = casa.ID,
                Description = casa.Description,
                Name = casa.Description,
                Type = casa.Type,
                SubType = casa.SubType,
                Areas = new List<Area>()
            };
            for (int i = 0; i <= casa.Elements.Count - 1; i++)
            {
                if (i < 2) // I primi due elementi sono i piani della casa con le stanze
                {
                    var floor = casa.Elements[i].Data;
                    for (int ii = 0; ii < floor.Elements.Count; ii++)
                    {
                        var element = floor.Elements[ii].Data;
                        HomeStructure.Areas.Add(new Area()
                        {
                            ID = element.ID,
                            Description = element.Description,
                            Name = element.Description,
                            Type = element.Type,
                            SubType = element.SubType,
                            Devices = new List<Device>()
                        });

                        for (int iii = 0; iii < element.Elements.Count; iii++)
                        {
                            var elData = element.Elements[iii].Data;
                            HomeStructure.Areas.Last().Devices.Add(CreateDeviceBinding(elData, element.Description));
                        }
                    }
                }
                else //I rimanenti sono i sensori Temp/Umid
                {
                    var clima = casa.Elements[i].Data;
                    Area area = HomeStructure.Areas.FirstOrDefault(x => x.Description == clima.Description);
                    if (area != null)
                    {
                        area.Devices.Add(CreateDeviceBinding(clima, clima.Description));
                    }
                    else
                    {
                        if (clima.Description == "Bagno Primo piano")
                        {
                            area = HomeStructure.Areas.FirstOrDefault(x => x.Description == "Bagno PP");
                            area.Devices.Add(CreateDeviceBinding(clima, clima.Description));
                        }
                        else { Utility.Utility.WriteLog($"Unable to match area name {clima.Description}"); }

                    }

                }
            }

            //Leggo ingressi
            for (int ing = 1; ing <= root.OutData[0].Elements.Count - 1; ing++)
            {
                var ingresso = root.OutData[0].Elements[ing];
                if (ingresso != null && ingresso.Data.Type == Enums.OBJECT_TYPE.INPUT)
                {
                    Area ingressi = HomeStructure.Areas.Find(a => a.Description == "Ingressi");
                    if (ingressi == null)
                    {
                        HomeStructure.Areas.Add(new Area()
                        {
                            ID = "DOM#IN",
                            Description = "Ingressi",
                            Name = "Ingressi",
                            Type = ingresso.Data.Type,
                            SubType = ingresso.Data.SubType,
                            Devices = new List<Device>()
                        });
                        ingressi = HomeStructure.Areas.Last();
                    }
                    ingressi.Devices.Add(CreateDeviceBinding(ingresso.Data, ingressi.Description));
                }
                else if (ingresso != null && ingresso.Data.Type == Enums.OBJECT_TYPE.RULE)
                {
                    if (ingresso.Data.RuleAtoms?.Count > 0 && ingresso.Data.RuleAtoms.First().ObjID.StartsWith("ALM"))
                    { //E' un elemento allarme
                        Area allarme = HomeStructure.Areas.Find(a => a.Description == "Allarme");
                        if (allarme == null)
                        {
                            HomeStructure.Areas.Add(new Area()
                            {
                                ID = "DOM#ALM",
                                Description = "Allarme",
                                Name = "Allarme",
                                Type = ingresso.Data.Type,
                                SubType = ingresso.Data.SubType,
                                Devices = new List<Device>()
                            });
                            allarme = HomeStructure.Areas.Last();
                        }
                        allarme.Devices.Add(CreateDeviceBinding(ingresso.Data, allarme.Description));
                    }
                }
            }
        }
    }
}
