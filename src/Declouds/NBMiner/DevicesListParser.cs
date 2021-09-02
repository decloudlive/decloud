using Newtonsoft.Json;
using DCL.Common.Device;
using System.Collections.Generic;
using System.Linq;

namespace NBDecloud
{
    internal class DevicesListParser
    {
        internal class Device
        {
            public int device_id { get; set; }
            public object memory { get; set; }
            public string name { get; set; }
            public int pci_bus_id { get; set; }
        }
        internal class NbDecloudDevices
        {
            public List<Device> devices { get; set; }
        }

        public static Dictionary<string, int> ParseNBDecloudOutput(string DecloudListDevicesJSON, IEnumerable<BaseDevice> DCLDevices)
        {
            var DecloudDevices = JsonConvert.DeserializeObject<NbDecloudDevices>(DecloudListDevicesJSON)?.devices ?? null;
            var mappedDevices = DCLDevices
                .Where(dev => dev is IGpuDevice)
                .Cast<IGpuDevice>()
                .Select(gpu => (gpu, DecloudDevice: DecloudDevices?.FirstOrDefault(dev => gpu.PCIeBusID == dev.pci_bus_id) ?? null))
                .Where(p => p.DecloudDevice != null)
                .ToDictionary(p => p.gpu.UUID, p => p.DecloudDevice.device_id);
            return mappedDevices;
        }
    }
}
