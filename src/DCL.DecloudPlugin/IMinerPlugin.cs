using DCL.Common.Algorithm;
using DCL.Common.Device;
using System;
using System.Collections.Generic;

namespace DCL.DecloudPlugin
{
    /// <summary>
    /// IDecloudPlugin is the base interface for registering a plugin in Decloud.
    /// This Interface should convey the name, version, grouping logic and most importantly should filter supported devices and algorithms.
    /// </summary>
    public interface IDecloudPlugin
    {
        /// <summary>
        /// Specifies the plugin version.
        /// </summary>
        Version Version { get; }

        /// <summary>
        /// Specifies the plugin name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Specifies the plugin author.
        /// </summary>
        string Author { get; }


        /// <summary>
        /// Checks supported devices for the plugin and returns devices and algorithms that can be mined with the plugin.
        /// </summary>
        /// <param name="devices"></param>
        Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices);

        /// <summary>
        /// Creates the plugin Decloud instance that is used for mining inside Decloud. 
        /// </summary>
        /// <returns>Returns the underlying IDecloud instance.</returns>
        IDecloud CreateDecloud();

        /// <summary>
        /// UUID for the plugin.
        /// </summary>
        string PluginUUID { get; }

        /// <summary>
        /// Checks if mining pairs a and b can be executed inside the same Decloud (IDecloud) instance.
        /// For example if we want to mine NeoScrypt on the two GPUs with ccDecloud we will create only one Decloud instance and run both on it.
        /// On certain Decloud like cpuDecloud if we would have dual socket CPUs and would mine the same algorithm we would run two instances.
        /// This is case by case and it depends on the Decloud.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        bool CanGroup(MiningPair a, MiningPair b);
    }
}
