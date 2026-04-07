using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckStationarity.Check
{
    public class CheckErrors
    {
        private List<double> errors;
        public CheckErrors(List<double> errors)
        {
            this.errors = errors;
        }

        public double GetErrorAverage()
        {
            return errors.Average();
        }

        public double GetSigmaSquare()
        {
            return Math.Sqrt(errors.Sum(r => Math.Pow((r - GetErrorAverage()), 2)) / errors.Count);
        }

        public bool LjungBoxTest(int maxLag)
        {
            int n = errors.Count;
            double Q = 0.0;

            for (int lag = 1; lag <= maxLag; lag++)
            {
                double acf = Autocorrelation(errors, lag);
                Q += acf * acf / (n - lag);
            }

            Q *= n * (n + 2);

            double chiCritical = ChiSquared.InvCDF(maxLag, 0.95);

            if (Q < chiCritical)
                Console.WriteLine("Залишки схожі на білий шум");
            else
                Console.WriteLine("Є автокореляція у залишках");

            return Q < chiCritical;
        }
        private double Autocorrelation(List<double> data, int lag)
        {
            int n = data.Count;
            double mean = 0.0;
            foreach (var d in data) mean += d;
            mean /= n;

            double num = 0.0, den = 0.0;
            for (int i = 0; i < n - lag; i++)
                num += (data[i] - mean) * (data[i + lag] - mean);

            for (int i = 0; i < n; i++)
                den += (data[i] - mean) * (data[i] - mean);

            return num / den;
        }

        public List<double> ErrorsACF(int maxLag)
        {
            double mean = GetErrorAverage();
            var acfs = new List<double>();
            for (int lag = 1; lag <= maxLag; lag++)
            {
                double numerator = 0;
                double denominator = 0;

                for (int i = lag; i < errors.Count; i++)
                    numerator += (errors[i] - mean) * (errors[i - lag] - mean);

                denominator = errors.Sum(r => (r - mean) * (r - mean));
                acfs.Add(numerator / denominator);
            }
            return acfs;
        }
    }
}
