using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CheckStationarity
{
    public class Winsorization
    {
        private double GetAverage(List<double> data)
        {
            return data.Average();  
        }

        private double GetStandartEvaluation(List<double> data, double average)
        {
            double n = data.Count;
            double sigma2 = (1.0 / n) * data.Sum(x => Math.Pow((x - average), 2));
            return Math.Sqrt(sigma2);
        }

        public List<double> UpdateList(List<double> data)
        {
            var updatedList = new List<double>();
            double average = GetAverage(data);
            double sigma = GetStandartEvaluation(data, average);
            double lower = average - 1.5 * sigma;
            double upper = average + 1.5 * sigma;
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i] > upper)
                    updatedList.Add(upper);
                else if (data[i] < lower)
                    updatedList.Add(lower);
                else
                    updatedList.Add(data[i]);
            }
            return updatedList;
        }
    }
}
