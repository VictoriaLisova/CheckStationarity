using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckStationarity.Differentiate.Differentiate
{
    public class SelectMethod
    {
        public StationarityMethod GetIsStationary(string type)
        {
            switch (type)
            {
                case "kpss": return new KPSSMethod();
                case "adf":return new ADFMethod();
                default: throw new ArgumentException();
            }
        }
    }
}
