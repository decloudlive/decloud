using DCL.DecloudPlugin;
using System;
using System.Collections.Generic;

namespace DCL.DecloudPluginToolkitV1.Interfaces
{
    public interface IGetApiMaxTimeoutV2
    {
        bool IsGetApiMaxTimeoutEnabled { get; }
        TimeSpan GetApiMaxTimeout(IEnumerable<MiningPair> miningPairs);
    }
}
