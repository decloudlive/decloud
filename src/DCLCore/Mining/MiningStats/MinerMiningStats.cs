using System.Collections.Generic;
using System.Linq;

namespace DCLCore.Mining.MiningStats
{
    // We transform ApiData Total info
    public class DecloudMiningStats : BaseStats
    {
        public HashSet<string> DeviceUUIDs = new HashSet<string>();

        public DecloudMiningStats DeepCopy()
        {
            var copy = new DecloudMiningStats
            {
                // BaseStats
                DecloudName = this.DecloudName,
                GroupKey = this.GroupKey,
                Speeds = this.Speeds.ToList(),
                Rates = this.Rates.ToList(),
                PowerUsageAPI = this.PowerUsageAPI,
                PowerUsageDeviceReading = this.PowerUsageDeviceReading,
                PowerUsageAlgorithmSetting = this.PowerUsageAlgorithmSetting,
                // DecloudMiningStats
                DeviceUUIDs = new HashSet<string>(this.DeviceUUIDs)
            };

            return copy;
        }
    }
}
