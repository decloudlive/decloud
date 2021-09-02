using System;
using System.Collections.Generic;
using System.IO;

namespace DCL.Common
{
    public static class Paths
    {
        public static string Root { get; private set; } = "";
        public static string AppRoot { get; private set; } = "";

        public static void SetRoot(string rootPath)
        {
            Root = rootPath;
        }

        public static void SetAppRoot(string appRootPath)
        {
            AppRoot = appRootPath;
        }

        [Obsolete("Use DecloudPluginsPath(params string[] paths)", true)]
        public static string DecloudPluginsPath()
        {
            return RootPath("Decloud_plugins");
        }

        public static string DecloudPluginsPath(params string[] paths)
        {
            return RootPath("Decloud_plugins", paths);
        }

        public static string ConfigsPath(params string[] paths)
        {
            return RootPath("configs", paths);
        }

        public static string InternalsPath(params string[] paths)
        {
            return RootPath("internals", paths);
        }

        public static string RootPath(string subPath, params string[] paths)
        {
            var combine = new List<string> { Root, subPath };
            if (paths.Length > 0) combine.AddRange(paths);
            var path = Path.Combine(combine.ToArray());
            return path;
        }

        public static string AppRootPath(string subPath, params string[] paths)
        {
            var combine = new List<string> { AppRoot, subPath };
            if (paths.Length > 0) combine.AddRange(paths);
            var path = Path.Combine(combine.ToArray());
            return path;
        }
    }
}
