using System.Linq;

namespace DCLCore.Mining.MiningStats
{
    // We transform ApiData PerDevice info
    public class DeviceMiningStats : BaseStats
    {
        public string DeviceUUID { get; set; } = "";

        public DeviceMiningStats DeepCopy()
        {
            var copy = new DeviceMiningStats
            {
                // BaseStats
                DecloudName = this.DecloudName,
                GroupKey = this.GroupKey,
                Speeds = this.Speeds.ToList(),
                Rates = this.Rates.ToList(),
                PowerUsageAPI = this.PowerUsageAPI,
                PowerUsageDeviceReading = this.PowerUsageDeviceReading,
                PowerUsageAlgorithmSetting = this.PowerUsageAlgorithmSetting,
                // DeviceMiningStats
                DeviceUUID = this.DeviceUUID
            };

            return copy;
        }
    }
}
