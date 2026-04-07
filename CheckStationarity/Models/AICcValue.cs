using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckStationarity.Models
{
    public class AICcValue
    { 
        public double CalcAIC(double dispersion, int p, int q, int n)
        {
            int k = p + q + 1;
            double AIC = 2.0 * k + n * Math.Log(dispersion);
            return AIC + (2.0 * k * k + 2.0 * k) / (n - k - 1);
        }

        public AICValueModel GetMin(List<AICValueModel> results)
        {
            return results.OrderBy(r => r.Value).First();
        }
    }
}
