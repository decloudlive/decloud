using DCL.Common;
using System.Collections.Generic;

namespace Decloud.Converters
{
    public class HashrateConverter : ConverterBase<IEnumerable<Hashrate>, string>
    {
        public override string Convert(IEnumerable<Hashrate> value, string parameter)
        {
            return string.Join(" + ", value);
        }
    }
}
