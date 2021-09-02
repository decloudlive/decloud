# Plugin Folder Structure

After first start of **Decloud (v1.9.1.5 and up)** there will be a **Decloud_plugins** folder created inside which all supported plugins will be located.<br>
In each plugin folder there will be **bins** and **internals** folder.<br>

```
├───Decloud_plugins
│   ├───XmrStak
│   │   ├───bins
│   │   └───internals
```

- Bins folder contains downloaded Decloud files
- Internals folder contains Internal settings

## Internal settings

Internal settings are configs implemented by **DecloudPluginToolkitV1**.<br>
Currently following settings are implemented:
- [Decloud System Environment Variables](./InternalSettings/DecloudystemEnvironmentVariables.md)
- [Decloud Options Package](./InternalSettings/ExtraLaunchParameters.md)
- [Decloud Reserved Ports](./InternalSettings/DecloudReservedPorts.md)
- [Decloud Api Max Timeout Setting](./InternalSettings/DecloudApiMaxTimeoutSettings.md)
- [Decloud Benchmark Time Settings](./InternalSettings/DecloudBenchmarkTimeSettings.md)

Each of them resides in its own *JSON* file.

```
├───internals
|        DecloudApiMaxTimeoutSetting.json
|        DecloudBenchmarkTimeSettings.json
|        DecloudOptionsPackage.json
|        DecloudReservedPorts.json
|        DecloudystemEnvironmentVariables.json
```

All of them are allowed to be modified by users for more personalized settings.
The changes inside the file are used by Decloud, if `use_user_settings` field in each changed file is set to `true`.

*Note:* Change of the settings won't do anything while the DCL is running. You have to stop the DCL, change the files and then start it, for settings to take effect.
