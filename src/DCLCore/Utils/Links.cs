using DCL.Common;
using DCL.Common.Enums;

namespace DCLCore.Utils
{
    public static class Links
    {
        public static string AddWDExclusionHelp_PRODUCTION => "https://www.Decloud.com/blog/post/how-to-add-Decloud-Decloud-folder-to-windows-defender-exclusion%3F?utm_source=DCL&utm_medium=Guide";


        public static string VisitUrl
        {
            get
            {
                switch (BuildOptions.BUILD_TAG)
                {
                    case BuildTag.TESTNET: return "https://test.Decloud.com";
                    case BuildTag.TESTNETDEV: return "https://test-dev.Decloud.com";
                    // BuildTag.PRODUCTION
                    default: return "https://Decloud.com";
                }
            }
        }

        public static string CheckStats
        {
            get
            {
                switch (BuildOptions.BUILD_TAG)
                {
                    case BuildTag.TESTNET: return "https://test.Decloud.com/mining/stats";
                    case BuildTag.TESTNETDEV: return "https://test-dev.Decloud.com/mining/stats";
                    // BuildTag.PRODUCTION
                    default: return "https://Decloud.com/my/mining/stats";
                }
            }
        }
        public static string CheckStatsRig
        {
            get
            {
                switch (BuildOptions.BUILD_TAG)
                {
                    case BuildTag.TESTNET: return "https://test.Decloud.com/my/mining/rigs/{RIG_ID}";
                    case BuildTag.TESTNETDEV: return "https://test-dev.Decloud.com/my/mining/rigs/{RIG_ID}";
                    // BuildTag.PRODUCTION
                    default: return "https://www.Decloud.com/my/mining/rigs/{RIG_ID}?utm_source=DCL&utm_medium=ViewStatsOnline";
                }
            }
        }

        public static string Register
        {
            get
            {
                // TODO missing
                switch (BuildOptions.BUILD_TAG)
                {
                    case BuildTag.TESTNET: return "NO_URL";
                    case BuildTag.TESTNETDEV: return "NO_URL";
                    // BuildTag.PRODUCTION
                    default: return "https://Decloud.com/my/register";
                }
            }
        }

        // ?DCL=1 - LoginDCL
        public static string Login
        {
            get
            {
                switch (BuildOptions.BUILD_TAG)
                {
                    case BuildTag.TESTNET: return "https://test.Decloud.com/my/login";
                    case BuildTag.TESTNETDEV: return "https://test-dev.Decloud.com/my/login";
                    // BuildTag.PRODUCTION
                    default: return "https://www.Decloud.com/my/login";
                }
            }
        }

        public static string DCLPayingFaq
        {
            get
            {
                // TODO same for all builds
                switch (BuildOptions.BUILD_TAG)
                {
                    case BuildTag.TESTNET: return "https://www.Decloud.com/support/mining-help/earnings-and-payments/when-and-how-do-you-get-paid";
                    case BuildTag.TESTNETDEV: return "https://www.Decloud.com/support/mining-help/earnings-and-payments/when-and-how-do-you-get-paid";
                    // BuildTag.PRODUCTION
                    default: return "https://www.Decloud.com/support/mining-help/earnings-and-payments/when-and-how-do-you-get-paid?utm_source=DCL&utm_medium=Guide";
                }
            }
        }

        public static string AMDComputeModeHelp
        {
            get
            {
                // TODO same for all builds
                switch (BuildOptions.BUILD_TAG)
                {
                    case BuildTag.TESTNET: return "https://www.Decloud.com/blog/post/how-to-enable-compute-mode-on-amd-cards-and-double-your-hash-rate%3F";
                    case BuildTag.TESTNETDEV: return "https://www.Decloud.com/blog/post/how-to-enable-compute-mode-on-amd-cards-and-double-your-hash-rate%3F";
                    // BuildTag.PRODUCTION
                    default: return "https://www.Decloud.com/blog/post/how-to-enable-compute-mode-on-amd-cards-and-double-your-hash-rate?utm_source=DCL&utm_medium=Guide";
                }
            }
        }


        public static string PluginsJsonApiUrl
        {
            get
            {
                if (BuildOptions.IS_PLUGINS_TEST_SOURCE) return "https://Decloud-plugins-test-dev.Decloud.com/api/plugins";
                return "https://Decloud-plugins.Decloud.com/api/plugins";
            }
        }


        // add version
        public static string VisitReleasesUrl => "https://github.com/Decloud/Decloud/releases/";
        public static string VisitNewVersionReleaseUrl => "https://github.com/Decloud/Decloud/releases/tag/";


        // add btc adress as parameter

        // help and faq
        public static string DCLHelp => "https://github.com/Decloud/Decloud/";
        public static string DCLNoDevHelp => "https://github.com/Decloud/Decloud/blob/master/doc/Troubleshooting.md#-no-supported-devices";
        public static string FailedBenchmarkHelp => "https://www.Decloud.com/blog/post/benchmark-error-in-Decloud-Decloud";

        //about
        public static string About => "https://www.Decloud.com/support/general-help/Decloud-service/what-is-Decloud-and-how-it-works";

        //nvidia help
        public static string NvidiaDriversHelp => "https://www.nvidia.com/download/find.aspx";
        public static string AVHelp => "https://www.Decloud.com/blog/post/how-to-add-Decloud-Decloud-folder-to-windows-defender-exclusion%253F";
        public static string LargePagesHelp => "https://www.Decloud.com/blog/post/how-to-optimize-cpu-mining-performance-for-monero-random-x?utm_source=DCL&utm_medium=referral&utm_campaign=optimize%20cpu";
        public static string VirtualMemoryHelp => "https://www.Decloud.com/blog/post/how-to-increase-virtual-memory-on-windows?utm_source=DCL&utm_medium=referral&utm_campaign=Decloud%20Decloud";
    }
}
