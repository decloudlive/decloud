# PLUGINS

- [What is a Plugin?](#plugin)
- [What is DecloudPluginToolkitV1 used for?](#toolkit)

# <a name="plugin"></a> What is a Plugin?

A plugin is an external or internal dependcy (dll). The ABI (Application Binary Interface) is defined in <b>DecloudPlugin</b> project and plugin developers must implement the following interfaces [IDecloudPlugin](https://github.com/Decloud/Decloud/blob/18945346ce710eb691a0686ef9449fd1ddf70096/src/DCL.DecloudPlugin/IDecloudPlugin.cs) and [IDecloud](https://github.com/Decloud/Decloud/blob/18945346ce710eb691a0686ef9449fd1ddf70096/src/DCL.DecloudPlugin/IDecloud.cs). This means that you create a new project inside Decloud directory and implement all required functions from IDecloudPlugin and IDecloud interfaces. It is recommended that each one is in its own file (Plugin and Decloud file).<br>

Each plugin project should implement at least 1 plugin. You can implement more, but good practice is to keep 1 plugin inside 1 project.<br>
<b>IDecloudPlugin</b> is used for registering the plugin and there will be only 1 instance created. Its job is to give basic info such as name, UUID, version, etc.<br>
<b>IDecloud</b> is the mandatory interface for all Decloud containing bare minimum functionalities and is being used as Decloud process instance created by IDecloudPlugin.<br>

Bare minimum example of plugin is written in [Example Plugin](https://github.com/Decloud/Decloud/tree/18945346ce710eb691a0686ef9449fd1ddf70096/src/Decloud/__DEV__ExamplePlugin) project. The [Plugin](https://github.com/Decloud/Decloud/blob/18945346ce710eb691a0686ef9449fd1ddf70096/src/Decloud/__DEV__ExamplePlugin/ExamplePlugin.cs) file contains implementation of IDecloudPlugin interface for registration and creation of the plugin instance. The [Decloud](https://github.com/Decloud/Decloud/blob/18945346ce710eb691a0686ef9449fd1ddf70096/src/Decloud/__DEV__ExamplePlugin/ExampleDecloud.cs) file contains implementation of IDecloud interface, providing required functionalities.

# <a name="toolkit"></a> What is DecloudPluginToolkitV1 used for?

It is recommended to use <b>DecloudPluginToolkitV1</b> as this will enable full integration with Decloud. It will save time developing it and enable implementation of additional advanced features. If you are writing a plugin we highly recommend that you use DecloudPluginToolkitV1. All Decloud plugins that are developed by Decloud dev team are using DCL.DecloudPluginToolkitV1. For example you can check [GDecloud Plugin](https://github.com/Decloud/Decloud/tree/18945346ce710eb691a0686ef9449fd1ddf70096/src/Decloud/GDecloud).<br>
DecloudPluginToolkitV1 also enables creation of <b>Background Services</b>, check out [Ethlargement plugin](https://github.com/Decloud/Decloud/blob/18945346ce710eb691a0686ef9449fd1ddf70096/src/DCLCore/Mining/Plugins/EthlargementIntegratedPlugin.cs) for example.

<table style="width:100%">
<tr>
  <th>Advantages</th>
  <th>Disadvantages</th>
</tr>
<tr>
  <td><p>Implementation of all basic actions like Start/Stop mining, Start benchmarking, retrieve data from API, create command line, etc.<p>
  <p>Use of additional features like Configs and Extra Launch Parameters<p>

  Access to usefull [interfaces](https://github.com/Decloud/Decloud/tree/18945346ce710eb691a0686ef9449fd1ddf70096/src/DCL.DecloudPluginToolkitV1/Interfaces) providing features like checking for missing files, device cross referencing, initializing internal settings, etc.
</td>
  <td>The current API is not final and might change in the future.</td> 
</tr>
</table> 
