using System.Collections.Generic;

namespace DCL.DecloudPluginToolkitV1.Interfaces
{
    /// <summary>
    /// IBinaryPackageMissingFilesChecker interface is used by plugin to return missing required files and depencencies (*.dll or *.exe files) from Decloud binary package.
    /// If the return value is not empty, this will indicate that the binary cannot be properly executed.
    /// </summary>
    public interface IBinaryPackageMissingFilesChecker
    {
        IEnumerable<string> CheckBinaryPackageMissingFiles();
    }
}
