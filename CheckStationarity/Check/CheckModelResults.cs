using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace CheckStationarity.Check
{
    public class CheckModelResults
    {
        private List<double> SubstractList(List<double> list1, List<double> list2, int pow)
        {
            var newList = new List<double>();
            for(int i = 0; i < list1.Count; i++)
            {
                newList.Add(Math.Pow(list1[i] - list2[i], pow));
            }
            return newList;
        }
        public double GetMSE(List<double> testedData, List<double> forecastData)
        {
            double n = testedData.Count;
            var substractList = SubstractList(testedData, forecastData, 2);
            return 1.0 / n * substractList.Sum();
        }

        public double GetMAE(List<double> real, List<double> forecast)
        {
            double n = real.Count;
            var subtractList = SubstractList(real, forecast, 1).Select(x => Math.Abs(x)).ToList();
            return (1.0 / n) * subtractList.Sum(); 
        }

        public double GetMAPE(List<double> real, List<double> forecast)
        {
            double n = real.Count;
            var list = new List<double>();
            for(int i = 0; i < real.Count; i++)
            {
                list.Add(Math.Abs((real[i] - forecast[i]) / real[i]));
            }
            return (1.0 / n) * list.Sum() * 100;
        }

        public double GetRMSE(List<double> real, List<double> forecast)
        {
            double n = real.Count;
            var list = SubstractList(forecast, real, 2).Select(x => x / n).ToList();
            return Math.Sqrt(list.Sum());
        }
    }
}
