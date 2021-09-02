﻿using DCL.Common.Device;
using DCL.Common.Enums;
using System;

namespace DCL.DecloudPluginToolkitV1.Interfaces
{
    /// <summary>
    /// IReBenchmarkChecker interface is used by plugins to check if previously saved benchmark algorithms should be re-benchmarked.
    /// This comes usefull when updating plugin to a version with different performances.
    /// </summary>
    public interface IReBenchmarkChecker
    {
        bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids);
    }
}
