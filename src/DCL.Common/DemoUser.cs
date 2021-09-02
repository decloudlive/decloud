
using DCL.Common.Enums;

namespace DCL.Common
{
    public static class DemoUser
    {
        public static string BTC
        {
            get
            {
                if (BuildOptions.BUILD_TAG == BuildTag.TESTNET) return "2N6ibfrTwUSSvzAz1esPe1gYULG82asTHiS";
                if (BuildOptions.BUILD_TAG == BuildTag.TESTNETDEV) return "2N2e2ET1jMY9r5is9KaTKnU3bkCFaYHEEEx";
                //BuildTag.PRODUCTION
                return "33hGFJZQAfbdzyHGqhJPvZwncDjUBdZqjW";
            }
        }
    }
}
