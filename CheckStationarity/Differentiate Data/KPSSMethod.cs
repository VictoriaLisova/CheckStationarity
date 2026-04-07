using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckStationarity.Differentiate
{
    public class KPSSMethod : StationarityMethod
    {
        private const double ControlValue = 0.463;

        private List<double> CumulativeSum(List<double> data)
        {
            var newData = new List<double>();
            newData.Add(data[0]);
            for(int i = 1; i < data.Count; i++)
            {
                newData.Add(newData[i - 1] + data[i]);
            }
            return newData;
        }

        public override bool IsStationary(List<double> data)
        {
            var diff = GetDifference(data, GetAverage(data));
            var diffSquares = GetListSquares(diff);
            var cumSum = CumulativeSum(diff);
            var cumSquare = GetListSquares(cumSum);
            var dispersion = Dispersion(diffSquares);

            var kpssCoef = cumSquare.Sum()/(Math.Pow(data.Count, 2) * dispersion);
            return kpssCoef < ControlValue;
        }
    }
}
