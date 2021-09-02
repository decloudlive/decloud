# Plugins

<ul>
<li><a href="#plugin">What is a Plugin?</a></li>
<li><a href="#toolkit">What is DecloudPluginToolkitV1 used for?</a></li>
<li><a href="#listOfPlugins">List of included plugins</a></li>
</ul>

<h1 id="plugin">What is a Plugin?</h1>
<p>A plugin is an external or internal dependcy (dll). The ABI (Application Binary Interface) is defined in <b>DecloudPlugin</b> project and plugin developers must implement the following interfaces <a href="../../src/DCL.DecloudPlugin/IDecloudPlugin.cs">IDecloudPlugin</a> and <a href="../../src/DCL.DecloudPlugin/IDecloud.cs">IDecloud</a>. This means that you create a new project inside Decloud directory and implement all required functions from IDecloudPlugin and IDecloud interfaces. It is recommended that each one is in its own file (Plugin and Decloud file).</p>
<p>Each plugin project should implement at least 1 plugin. You can implement more, but good practice is to keep 1 plugin inside 1 project.
<br><b>IDecloudPlugin</b> is used for registering the plugin and there will be only 1 instance created. Its job is to give basic info such as name, UUID, version, etc.
<br><b>IDecloud</b> is the mandatory interface for all Decloud containing bare minimum functionalities and is being used as Decloud process instance created by IDecloudPlugin.</p>
<p>Bare minimum example of plugin is written in <a href="../../src/Decloud/__DEV__ExamplePlugin">Example Plugin</a> project. The <a href="../../src/Decloud/__DEV__ExamplePlugin/ExamplePlugin.cs">Plugin</a> file contains implementation of IDecloudPlugin interface for registration and creation of the plugin instance. The <a href="../../src/Decloud/__DEV__ExamplePlugin/ExampleDecloud.cs">Decloud</a> file contains implementation of IDecloud interface, providing required functionalities.</p>

<h1 id="toolkit">What is DecloudPluginToolkitV1 used for?</h1>
<p>It is recommended to use <b>DecloudPluginToolkitV1</b> as this will enable full integration with Decloud. It will save time developing it and enable implementation of additional advanced features. If you are writing a plugin we highly recommend that you use DecloudPluginToolkitV1. All Decloud plugins that are developed by Decloud dev team are using DecloudPluginToolkitV1. For example you can check <a href="../../src/Decloud/GDecloud">GDecloud Plugin</a>.</p>
<p>DecloudPluginToolkitV1 also enables creation of <b>Background Services</b>, check out <a href="../../src/DCLCore/Mining/Plugins/EthlargementIntegratedPlugin.cs">Ethlargement plugin</a> for example.</p>

<table style="width:100%">
<tr>
  <th>Advantages</th>
  <th>Disadvantages</th>
</tr>
<tr>
  <td><p>Implementation of all basic actions like Start/Stop mining, Start benchmarking, retrieve data from API, create command line, etc.</p>
  <p>Use of additional features like Configs and Extra Launch Parameters</p>
  <p>Access to usefull <a href="../../src/DCL.DecloudPluginToolkitV1/Interfaces">interfaces</a> providing features like checking for missing files, device cross referencing, initializing internal settings, etc.</p>
</td>
  <td>The current API is not final and might change in the future.</td> 
</tr>
</table> 
<br>
<h2 id="listOfPlugins">List of Included Plugins</h2>

## Decloud Plugins

### All devices

* [Xmr-Stak](./PluginDocs/XmrStak.md)

### NVIDIA and AMD

* BDecloud
* ClaymoreDual
* GDecloud
* Phoenix
* LolDecloud
* NanoDecloud

### NVIDIA

* CCDecloud
* NBDecloud
* T-Rex
* TT-Decloud
* CryptoDredge
* MiniZ
* Z-Enemy

### AMD

* SGDecloud
* TeamRedDecloud
* WildRig

### CPU

* XMRig

## Background Services

* Ethlargement Pill


