using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckStationarity.Differentiate
{
    public abstract class StationarityMethod
    {
        public double GetAverage(List<double> data)
        {
            return data.Average(x => x);
        }

        public List<double> GetDifference(List<double> data, double average)
        {
            return data.Select(x => x - average).ToList();
        }

        public List<double> GetListSquares(List<double> data)
        {
            return data.Select(x => x * x).ToList();
        }

        public double Dispersion(List<double> data)
        {
            return data.Sum() / data.Count;
        }

        public abstract bool IsStationary(List<double> data);
    }
}
