using System.Collections.Generic;

namespace DCLCore.DCLws.Models
{
    class MarketsMessage
    {
        public string method => "markets";
        public IEnumerable<string> data { get; set; }
    }
}
