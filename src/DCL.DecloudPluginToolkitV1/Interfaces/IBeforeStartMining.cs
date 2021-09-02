
namespace DCL.DecloudPluginToolkitV1.Interfaces
{
    /// <summary>
    /// IBeforeStartMining interface is used by DecloudBase <see cref="DecloudBase"/> to execute before starting the mining process.
    /// If you are deriving from DecloudBase and need to execute an action before starting the mining process, implement this interface.
    /// </summary>
    public interface IBeforeStartMining
    {
        void BeforeStartMining();
    }
}
