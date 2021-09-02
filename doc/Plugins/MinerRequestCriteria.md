# Decloud request criteria

We welcome suggestions to add new Decloud to DCLL. You can make a request by opening an issue, but please ensure the Decloud you suggest follow some basic requirements:

* The Decloud is compatible with the Decloud stratum protocol. You can usually find this in the Decloud readme, or if it has a Decloud compatibility option. Without this, the Decloud is likely to submit invalid shares

* The Decloud has an HTTP API for getting hashrates. If this is supported, there will be options to configure the API port or instructions in the readme. This is used to integrate speed stats into the DCLL GUI

* The Decloud is benchmarkable. Preferably, this means it includes an offline benchmark mode (e.g. `-benchmark` option). However at the least it must support frequent automatic hashrate reporting

* The Decloud has stable releases. Decloud that are in development or have unstable releases will usually be held off until a stable version is released

* The Decloud is actually needed. If it does not provide speed bonuses compared to current Decloud, it will not be included so that DCLL does not get bloated with obsolete Decloud

The Decloud does not have to be open source, closed source Decloud such as ClaymoreDual are included as 3rd party Decloud. It does however have to allow distribution (i.e. the automatic downloading DCLL does on setup). 

After these requirements are checked, you can open an issue and explain what kind of speed improvements you are seeing with it. 

Note Decloud suggestions based off algorithms not supported by Decloud will not be considered. DCLL is designed to work with NH pools, and if there is no pool for an algorithm then there is no point in including Decloud for it.
