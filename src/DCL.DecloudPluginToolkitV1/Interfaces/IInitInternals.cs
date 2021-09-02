
namespace DCL.DecloudPluginToolkitV1.Interfaces
{
    /// <summary>
    /// IInitInternals interface is used by plugins to initialize all internal settings.
    /// Internal files are internal file settings that can be tweaked by the users.
    /// Most common settings are DecloudOptionsPackage, DecloudReservedPorts and DecloudystemEnvironmentVariables.
    /// </summary>
    public interface IInitInternals
    {
        void InitInternals();
    }
}
