using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckStationarity.Data_Inversion
{
    public class InvertData
    {
        private List<double> InverseFromYeonJohnson(List<double> data, double l)
        {
            var result = new List<double>();
            foreach (var y in data)
            {
                if (y >= 0)
                {
                    if (Math.Abs(l) < 1e-6) result.Add(Math.Exp(y) - 1);
                    else result.Add(Math.Pow((l * y + 1), (1.0 / l)) - 1);
                }
                else
                {
                    if (Math.Abs(l - 2) < 1e-6) result.Add(1 - Math.Exp(-y));
                    else result.Add(1 - Math.Pow((-(2 - l) * y + 1), (1.0 / (2 - l))));
                }
            }
            return result;
        }
        public List<double> Invert(List<double> data, string methodType, double l = -0.5, double shift = 0)
        {
            if(methodType == "log")
            {
                return data.Select(x => Math.Exp(x) - shift).ToList();
            }
            else if(methodType == "yeo")
            {
                return InverseFromYeonJohnson(data, l);
            }
            return data;
        }
    }
}
