using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckStationarity.Models
{
    public class ArimaModel : IModelType
    {
        private double BuildARPart(int start, int p, List<double> fi, List<double> data)
        {
            double sum = 0;
            for(int i = 0; i < p; i++)
            {
                int index = start - i - 1;
                if(index >= 0 && index < data.Count && i < fi.Count)
                {
                    sum += fi[i] * data[index];
                }
            }
            return sum;
        }

        private double BuildMAPart(int start, int q, List<double> tao, List<double> errors)
        {
            double sum = 0;
            for(int i = 0; i < q; i++)
            {
                int index = start - 1 - i;
                if(index >= 0 && i < tao.Count && index < errors.Count)
                {
                    sum += tao[i] * errors[index];
                }
            }
            return sum;
        }

        public double CalculateArima(int start, int p, int q, List<double> fi, List<double> tao, List<double> errors, List<double> data)
        {
            return BuildARPart(start, p, fi, data) + BuildMAPart(start, q, tao, errors);
        }
        public double BuildModel(List<double> data)
        {
            return 0;
        }
    }
}
