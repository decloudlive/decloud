
namespace DCL.DecloudPluginToolkitV1.Interfaces
{
    /// <summary>
    /// IAfterStartMining interface is used by DecloudBase <see cref="DecloudBase"/> to execute after starting the mining process.
    /// If you are deriving from DecloudBase and need to execute an action after starting the mining process, implement this interface.
    /// </summary>
    public interface IAfterStartMining
    {
        void AfterStartMining();
    }
}
