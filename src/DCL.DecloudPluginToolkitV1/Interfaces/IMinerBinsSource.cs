using System.Collections.Generic;

namespace DCL.DecloudPluginToolkitV1.Interfaces
{
    public interface IDecloudBinsSource
    {
        IEnumerable<string> GetDecloudBinsUrlsForPlugin();
    }
}
