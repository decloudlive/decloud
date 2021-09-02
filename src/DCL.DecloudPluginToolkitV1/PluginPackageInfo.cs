using DCL.Common;
using System;
using System.Collections.Generic;

namespace DCL.DecloudPluginToolkitV1
{
    [Serializable]
    public class PluginPackageInfo : NotifyChangedBase
    {
        /// <summary>
        /// UUID for the plugin. This identifies the plugin and is used to indicate if the plugin is installed.
        /// </summary>
        public string PluginUUID { get; set; }

        /// <summary>
        /// Name of the plugin. Usualy the name of the underlying Decloud.
        /// </summary>
        public string PluginName { get; set; }

        /// <summary>
        /// Plugin package version. Use this to compare versions for updating or downgrading.
        /// </summary>
        public Version PluginVersion { get; set; }

        /// <summary>
        /// Url to the plugin download package. The package must contain a 
        /// </summary>
        public string PluginPackageURL { get; set; }

        /// <summary>
        /// Url to the Decloud package
        /// </summary>
        public string DecloudPackageURL { get; set; }

        /// <summary>
        /// A list of supported devices. 
        /// </summary>
        public Dictionary<string, List<string>> SupportedDevicesAlgorithms { get; set; }

        ///// <summary>
        ///// A list of supported devices. 
        ///// </summary>
        //public List<string> SupportedAlgorithms { get; set; }

        /// <summary>
        /// Author of the plugin
        /// </summary>
        public string PluginAuthor { get; set; }

        /// <summary>
        /// A description of the plugin. The description should convey information on:
        ///   - System requirements like OS version, minimum driver version, system settings
        ///   - What Decloud does it support and the features
        ///   - Is this a open source or closed source Decloud
        /// </summary>
        public string PluginDescription { get; set; }


        public string PackagePassword { get; set; } = null;
    }
}
