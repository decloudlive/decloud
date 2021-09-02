using DCL.Common;
using DCL.DecloudPlugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DCL.DecloudPluginLoader
{
    // TODO implement unloadable plugins
    public static class DecloudPluginHost
    {
        public static Dictionary<string, IDecloudPlugin> DecloudPlugin { get; } = new Dictionary<string, IDecloudPlugin>();
        private static Type _pluginType = typeof(IDecloudPlugin);

        private static HashSet<string> _tempDirPrefixes = new HashSet<string> { "installing", "backup", "temp" };

        private static bool IsValidPluginDirectory(string dirPath)
        {
            return _tempDirPrefixes.Any(prefix => dirPath.Contains(prefix)) == false;
        }

        // mutates DecloudPlugin
        public static IEnumerable<string> LoadPlugins(string pluginsRootDirPath)
        {
            if (!Directory.Exists(pluginsRootDirPath))
            {
                Logger.Info("DecloudPluginHost", $"Plugins root directory doesn't exist: {pluginsRootDirPath}");
                return Enumerable.Empty<string>();
            }
            try
            {
                var loadedPlugins = Directory.GetDirectories(pluginsRootDirPath)
                .Where(IsValidPluginDirectory)
                .SelectMany(LoadPlugin)
                .ToList();
                Logger.Info("DecloudPluginHost", $"Plugins successfully loaded");
                return loadedPlugins;
            }
            catch (Exception e)
            {
                Logger.Error("DecloudPluginHost", $"LoadPlugins error: {e}");
                return Enumerable.Empty<string>();
            }
        }

        // mutates DecloudPlugin
        public static IEnumerable<string> LoadPlugin(string pluginDirPath)
        {
            if (!Directory.Exists(pluginDirPath))
            {
                Logger.Info("DecloudPluginHost", $"Plugins path doesn't exist: {pluginDirPath}");
                return Enumerable.Empty<string>();
            }
            try
            {
                var loadedPlugins = Directory.GetFiles(pluginDirPath, "*.dll")
                    .SelectMany(LoadPluginsFromDllFile)
                    .Select(plugin =>
                    {
                        try
                        {
                            if (!pluginDirPath.Contains(plugin.PluginUUID)) throw new Exception($"Plugin UUID ({plugin.PluginUUID}) not within correct path {pluginDirPath}.");
                            if (DecloudPlugin.ContainsKey(plugin.PluginUUID))
                            {
                                var existingPlugin = DecloudPlugin[plugin.PluginUUID];
                                Logger.Info("DecloudPluginHost", $"Already existing plugin {plugin.PluginUUID}");
                                Logger.Info("DecloudPluginHost", $"Old name {existingPlugin.Name} and version {existingPlugin.Version}\r\n new name {plugin.Name} and version {plugin.Version}");
                            }
                            DecloudPlugin[plugin.PluginUUID] = plugin;
                            return plugin.PluginUUID;
                        }
                        catch (Exception e)
                        {
                            Logger.Error("DecloudPluginHost", $"Error occured while loading plugin: {e}");
                            return null;
                        }
                    })
                    .Where(id => !string.IsNullOrEmpty(id) && !string.IsNullOrWhiteSpace(id))
                    .ToList();
                return loadedPlugins;
            }
            catch (Exception e)
            {
                Logger.Error("DecloudPluginHost", $"LoadPlugin Error: {e.Message}");
            }
            return Enumerable.Empty<string>();
        }

        private static Assembly LoadAssemblyWithoutFileLocking(string dllFilePath)
        {
            try
            {
                // read raw assembly and load that, this will not lock the file
                return Assembly.Load(File.ReadAllBytes(dllFilePath));
            }
            catch (Exception e)
            {
                Logger.Error("DecloudPluginHost", $"Error occured while loading dll '{dllFilePath}' files: {e.Message}");
                return null;
            }
        }

        private static IEnumerable<Type> GetAssemblyConcreteTypes(Assembly assembly)
        {
            if (assembly == null) return Enumerable.Empty<Type>();
            try
            {
                var concreteTypes = assembly.GetTypes().Where(type => !type.IsInterface && !type.IsAbstract);
                return concreteTypes.Where(type => type.GetInterface(_pluginType.FullName) != null);
            }
            catch (Exception e)
            {
                Logger.Error("DecloudPluginHost", $"Error occured while transforming dlls to plugins: {e.Message}");
                return Enumerable.Empty<Type>();
            }
        }

        private static IDecloudPlugin TypeToIDecloudPlugin(Type pluginType)
        {
            try
            {
                var plugin = Activator.CreateInstance(pluginType) as IDecloudPlugin;
                return plugin;
            }
            catch (Exception e)
            {
                Logger.Error("DecloudPluginHost", $"Error creating plugin instance: {e.Message}");
                return null;
            }
        }

        // load and return all managed plugins from dll 
        public static IEnumerable<IDecloudPlugin> LoadPluginsFromDllFile(string dllFilePath)
        {
            if (!File.Exists(dllFilePath))
            {
                Logger.Info("DecloudPluginHost", $"Plugins dll path doesn't exist: {dllFilePath}");
                return Enumerable.Empty<IDecloudPlugin>();
            }
            var assembly = LoadAssemblyWithoutFileLocking(dllFilePath);
            var plugins = GetAssemblyConcreteTypes(assembly).Select(TypeToIDecloudPlugin).Where(p => p != null);
            return plugins;
        }
    }
}
