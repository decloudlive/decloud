using log4net.Core;
using DCL.Common;
using DCL.Common.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using static DCLPackager.PackagerFileDirectoryUtils;
using static DCLPackager.PackagerPaths;

namespace DCLPackager
{
    class Program
    {
        private static bool ExecDCLpackerCreateInstallers(string exePath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            using (var proc = new Process { StartInfo = startInfo })
            {
                var ok = proc.Start();
                while (!proc.StandardOutput.EndOfStream)
                {
                    string line = proc.StandardOutput.ReadLine();
                    Logger.Info("DCLpacker", line);
                }
                return ok && proc.ExitCode == 0;
            }
        }

        private static bool Exec7ZipCreatePasswordArchive(string password, string archiveName, string releasePath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "7z.exe",
                Arguments = $"-p{password} a {archiveName} {releasePath}\\*",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            using (var proc = new Process { StartInfo = startInfo })
            {
                var ok = proc.Start();
                while (!proc.StandardOutput.EndOfStream)
                {
                    string line = proc.StandardOutput.ReadLine();
                    Logger.Info("7z-pwd", line);
                }
                return ok && proc.ExitCode == 0;
            }
        }

        private static void Add7zToPath()
        {
            // Add common folder to path for launched processes
            var sevenZipPath = GetRootPath("DCLpacker", "bins_bundle", "7z");
            if (Directory.Exists(sevenZipPath))
            {
                const string pathKey = "PATH";
                var pathVar = Environment.GetEnvironmentVariable(pathKey);
                pathVar += $";{sevenZipPath}";
                Environment.SetEnvironmentVariable(pathKey, pathVar);
            }
        }

        private static void CopyInstallerScriptIfAvailable()
        {
            try
            {
                File.Copy(GetRootPath(@"..\", "prepare_installers.bat"), GetTemporaryWorkFolder("prepare_installers.bat"));
            }
            catch (Exception e)
            {
                Logger.Error("Main", $"CopyInstallerScriptIfAvailable error: {e}");
            }
        }

        // #1 copy release folder
        // #2 delete all json settings from release folder
        // #3 get versions from launcher and app (they must be equal)
        // #4 append version to app folder
        // #5 copy assets (EULA,...)
        // #6 create NSIS template '_files_to_pack\nsis'
        static async Task Main(string[] args)
        {
            try
            {
                var randomPart = DateTime.UtcNow.Millisecond;
                var tmpWorkFolder = $"tmp_{randomPart}";
                Logger.ConfigureWithFile(GetRootPath($"{tmpWorkFolder}_log.txt"));
                Logger.ConfigureConsoleLogging(Level.Info);
                // TODO check if this already exists
                SetTemporaryWorkFolder(tmpWorkFolder);
                Add7zToPath();

                // #1
                // assume we are in installer folder
                var DCLReleaseFolder = GetRootPath(@"..\", "Release");
                ExecXCopy(DCLReleaseFolder, GetTemporaryWorkFolder("Release"));

                // run the plugins packer in the installer
                Logger.Info("Main", "DecloudPluginsPacker.Execute START");
                DecloudPluginsPacker.Execute(GetRootPath(@"..\", "src", "Decloud"));
                Logger.Info("Main", "DecloudPluginsPacker.Execute DONE. Press any key to continue\n\n");
                Console.ReadKey();

                // run the plugins packer in the installer
                Logger.Info("Main", "PluginPackageBundler.ExecuteTask START");
                await PluginPackageBundler.ExecuteTask(DecloudPluginsPacker.PreInstalledPlugins, DCL.DecloudPluginToolkitV1.Checkers.SupportedMajorVersions);
                Logger.Info("Main", "PluginPackageBundler.ExecuteTask DONE. Press any key to continue\n\n");
                Console.ReadKey();

                Logger.Info("Main", "Moving 'Decloud_plugins' and 'plugins_packages' to 'Release'");
                Directory.Move(GetDecloudPluginsPath(), GetTemporaryWorkFolder("Release", "Decloud_plugins"));
                Directory.Move(GetPluginsPackagesPath(), GetTemporaryWorkFolder("Release", "plugins_packages"));

                // #2 
                DeletePathIfExists(GetTemporaryWorkFolder("Release", "build_settings.json"));

                // #3 
                var (generatedTemplateLauncher, versionLauncher, buildTagLauncher) = VersionInfoHelpers.GenerateVariableTemplate(GetTemporaryWorkFolder("Release", "Decloud.exe"));
                var (generatedTemplate, version, buildTag) = VersionInfoHelpers.GenerateVariableTemplate(GetTemporaryWorkFolder("Release", "app", "app_DCL.exe"));
                if (generatedTemplateLauncher != generatedTemplate || versionLauncher != version || buildTagLauncher != buildTag)
                {
                    throw new Exception($"Launcher and App TAG or Version missmatch!!!\n{generatedTemplateLauncher} != {generatedTemplate} \n{versionLauncher} != {version} \n{buildTagLauncher} != {buildTag}");
                }
                Logger.Info("Main", "ExecPluginsPacker resumming...");
                // #4 
                var appDirOld = GetTemporaryWorkFolder("Release", "app");
                var appDirNew = GetTemporaryWorkFolder("Release", $"app_{version}");
                Logger.Info("Main", $"moving '{appDirOld}' to '{appDirNew}'");
                Directory.Move(appDirOld, appDirNew);
                // #5
                ZipFile.ExtractToDirectory(GetRootPath("EULA.zip"), GetTemporaryWorkFolder("Release"));
                // #6 
                var filesToPackPath = GetTemporaryWorkFolder("_files_to_pack");
                RecreateDirectoryIfExists(filesToPackPath);
                // copy template and exe files
                ExecXCopy(GetTemporaryWorkFolder("Release"), Path.Combine(filesToPackPath, "bins"));
                ExecXCopy(GetRootPath("nsis_template"), Path.Combine(filesToPackPath, "nsis"));
                File.WriteAllText(Path.Combine(filesToPackPath, "nsis", "include_common", "packageDefsGenerated.nsh"), generatedTemplate, new UTF8Encoding(true));
                File.WriteAllText(Path.Combine(filesToPackPath, "version.txt"), version + buildTag);
                // delete previous _files_to_pack from DCLpacker
                var DCLPackerFilesToPack = GetRootPath("DCLpacker", "_files_to_pack");
                RecreateDirectoryIfExists(DCLPackerFilesToPack);
                ExecXCopy(filesToPackPath, DCLPackerFilesToPack);
                RecreateDirectoryIfExists(GetRootPath("DCLpacker", "_files_to_pack", "assets")); // just so the packer works
                ExecDCLpackerCreateInstallers(GetRootPath("DCLpacker", "DCLpacker.exe"));
                File.Move(GetRootPath("DCLpacker", $"DCL_windows_{version}.exe"), GetTemporaryWorkFolder($"DCL_windows_{version}.exe"));
                File.Move(GetRootPath("DCLpacker", $"DCL_windows_updater_{version}.exe"), GetTemporaryWorkFolder($"DCL_windows_updater_{version}.exe"));
                // move to the temp folder

                // DCL_windows_1.9.2.18_testnetdev.zip
                // DCL_windows_1.9.2.18_testnet.zip
                // TODO create these settings instead of copying them
                var buildSettings = new List<BuildTag> { BuildTag.PRODUCTION, BuildTag.TESTNET, BuildTag.TESTNETDEV };
                foreach (var build in buildSettings)
                {
                    DeletePathIfExists(GetTemporaryWorkFolder("Release", "build_settings.json"));
                    var zipFileName = $"DCL_windows_{version}";
                    if (build != BuildTag.PRODUCTION)
                    {
                        zipFileName += $"_{build.ToString().ToLower()}";
                        File.Copy(GetRootPath("build_settings", $"build_settings_{build}.json"), GetTemporaryWorkFolder("Release", "build_settings.json"), true);
                    }
                    else
                    {
                        var password = $"DCL{version.Replace(".", "")}";
                        var zipFileNamePwd = $"DCL_windows_{version}_pw.zip";
                        Logger.Info("Main", $"Creating password protected {zipFileNamePwd} package password='{password}'");
                        Exec7ZipCreatePasswordArchive(password, GetTemporaryWorkFolder(zipFileNamePwd), GetTemporaryWorkFolder("Release"));
                    }
                    Logger.Info("Main", $"Creating {zipFileName}.zip package");
                    ZipFile.CreateFromDirectory(GetTemporaryWorkFolder("Release"), GetTemporaryWorkFolder($"{zipFileName}.zip"));
                    Logger.Info("Main", $"FINISHED {zipFileName}.zip package");
                }

                Logger.Info("Main", "Clean up temp files...");
                Directory.Delete(GetTemporaryWorkFolder("_files_to_pack"), true);
                Directory.Delete(GetTemporaryWorkFolder("Release"), true);
                Logger.Info("Main", "Finishing...");
                CopyInstallerScriptIfAvailable();
                Directory.Move(GetTemporaryWorkFolder(), GetRootPath($"DCL_windows_{version}_release_files"));
                Logger.Info("Main", "DONE! SUCCESS!!! Press any key to continue\n\n");
                Console.ReadKey();
                return;
            }
            catch (Exception e)
            {
                Logger.Error("Main", $"\t\t PROBLEM: {e.Message} {e.Message}");
                Logger.Error("Main", $"\t\t PROBLEM: {e.StackTrace}");
                Logger.Error("Main", "DONE! FAIL!!! Press any key to continue\n\n");
                Console.ReadKey();
            }
        }
    }
}
