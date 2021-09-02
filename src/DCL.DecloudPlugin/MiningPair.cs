using DCL.Common.Algorithm;
using DCL.Common.Device;

namespace DCL.DecloudPlugin
{
    /// <summary>
    /// MiningPair class is used to combine active devices and algorithms
    /// </summary>
    public class MiningPair
    {
        /// <summary>
        /// Device represents mining device of type <see cref="BaseDevice"/>
        /// </summary>
        public BaseDevice Device { get; set; }

        /// <summary>
        /// Algorithm represents active algorithm of type <see cref="DCL.Common.Algorithm.Algorithm"/>
        /// </summary>
        public Algorithm Algorithm { get; set; }
    }
}
