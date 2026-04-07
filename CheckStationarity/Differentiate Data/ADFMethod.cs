using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckStationarity.Differentiate.Differentiate
{
    public class ADFMethod : StationarityMethod
    {
        private const double ControlValue = -2.86;

        private List<double> GetShorterList(List<double> data)
        {
            return data.Take(data.Count - 1).ToList();
        }

        private List<double> GetDiff(List<double> data)
        {
            var newList = new List<double>();
            for(int i = 1; i < data.Count; i++)
            {
                newList.Add(data[i] - data[i - 1]);
            }
            return newList;
        }

        private List<double> Mult(List<double> list1, List<double> list2)
        {
            var newList = new List<double>();
            for(int i = 0; i < list1.Count; i++)
            {
                newList.Add(list1[i] * list2[i]);
            }
            return newList;
        }
        private double Cov(List<double> deltaList, double deltaAverage, List<double> shorter, double average)
        {
            var firstMult = deltaList.Select(x => x - deltaAverage).ToList();
            var secondList = shorter.Select(x => x - average).ToList();
            return Mult(firstMult, secondList).Sum() / secondList.Count;
        }

        private List<double> ErrorsList(List<double> deltaY, double delta, double gamma, List<double> difference)
        {
            var errors = new List<double>();
            for(int i = 0; i < deltaY.Count; i++)
            {
                errors.Add(deltaY[i] - (delta + gamma * difference[i]));
            }
            return errors;
        }

        public override bool IsStationary(List<double> data)
        {
            var shorterList = GetShorterList(data);
            var difference = GetDiff(data);
            var deltaAverage = GetAverage(difference);
            var average = GetAverage(shorterList);
            var diff = GetDifference(shorterList, average);
            var cov = Cov(difference, deltaAverage, shorterList, average);
            var dispersion = Dispersion(GetListSquares(diff));
            var gamma = cov / dispersion;

            var errors = ErrorsList(difference, deltaAverage, gamma, diff);
            var sse = errors.Select(x => x * x).Sum();
            var sumSquares = GetListSquares(diff).Sum();
            var SE = Math.Sqrt(sse / ((errors.Count - 2) * sumSquares));
            var tCoef = gamma / SE;
            return tCoef < ControlValue;
        }
    }
}
