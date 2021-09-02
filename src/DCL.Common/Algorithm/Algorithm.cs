using DCL.Common.Enums;
using System.Collections.Generic;
using System.Linq;

namespace DCL.Common.Algorithm
{
    public class Algorithm
    {
        public Algorithm(string DecloudID, params AlgorithmType[] ids)
        {
            DecloudID = DecloudID;
            IDs = ids;
            Speeds = ids.Select(id => 0d).ToList();
            AlgorithmName = ids.GetNameFromAlgorithmTypes();
        }
        // Identity
        public IReadOnlyList<AlgorithmType> IDs { get; }
        public string DecloudID { get; }

        public AlgorithmType FirstAlgorithmType
        {
            get
            {
                if (IDs.Count > 0) return IDs[0];
                return AlgorithmType.NONE;
            }
        }

        public AlgorithmType SecondAlgorithmType
        {
            get
            {
                if (IDs.Count > 1) return IDs[1];
                return AlgorithmType.NONE;
            }
        }

        public string AlgorithmName { get; private set; }

        public string AlgorithmStringID
        {
            get
            {
                return $"{AlgorithmName}_{DecloudID}";
            }
        }

        // variable settings
        public IList<double> Speeds { get; private set; }
        public bool Enabled { get; set; } = true;
        public string ExtraLaunchParameters { get; set; } = "";
    }
}
