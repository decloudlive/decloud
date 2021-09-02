using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace DCLCore.DCLws.Models
{
#pragma warning disable 649, IDE1006
    class DecloudtatusMessage
    {
        public string method => "Decloud.status";
        [JsonProperty("params")]
        public List<JToken> param { get; set; }
    }
#pragma warning restore 649, IDE1006
}
