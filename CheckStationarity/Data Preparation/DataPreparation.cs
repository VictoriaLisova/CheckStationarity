using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;

namespace CheckStationarity.Data_Preparation
{
    public class DataPreparation
    {
        public double Shift { get; set; }
        private double LinearInterpolation(List<double> data, int index)
        {
            int left = index - 1;
            int right = index + 1;

            while(left >= 0 && double.IsNaN(data[left]))
                left--;

            while(right < data.Count && double.IsNaN(data[right]))
                right++;

            if(left >= 0 && right < data.Count)
                return data[left] + (data[right] - data[left]) * ((double)(index - left) / (right - left));

            if (left >= 0)
                return data[left];

            if(right < data.Count)
                return data[right];

            return double.NaN;
        }

        private double GetQ(List<double> data, int p)
        {
            var sortedData = data.OrderBy(x => x).ToList();
            double R = (p / 100.0) * (sortedData.Count - 1);
            int lower = (int)Math.Floor(R);
            int upper = (int)Math.Ceiling(R);

            if (lower == upper) return sortedData[lower];

            double weight = R - lower;
            return sortedData[lower] * (1 - weight) + sortedData[upper] * weight;
        }

        private List<double> GetIQR(List<double> data, double k)
        {
            double Q1 = GetQ(data, 25);
            double Q3 = GetQ(data, 75);
            double IQR = Q3 - Q1;

            double lower = Q1 - k * IQR;
            double upper = Q3 + k * IQR;

            return data.Select(x => x < lower || x > upper ? double.NaN : x).ToList();
        }
        private List<double> LogTransform(List<double> data)
        {
            double min = data.Min();
            Shift = min <= 0 ? Math.Abs(min) + 1 : 1e-6;

            return data.Select(x => Math.Log(x + Shift)).ToList();
        }
        private List<double> YeoJohnson(List<double> data, double lambda)
        {
            return data.Select(x =>
            {
                if (x >= 0)
                {
                    if (Math.Abs(lambda) < 1e-6)
                        return Math.Log(x + 1);
                    else
                        return (Math.Pow(x + 1, lambda) - 1) / lambda;
                }
                else
                {
                    if (Math.Abs(lambda - 2) < 1e-6)
                        return -Math.Log(-x + 1);
                    else
                        return -((Math.Pow(-x + 1, 2 - lambda) - 1) / (2 - lambda));
                }
            }).ToList();
        }
        public List<double> ProcessOutliners(List<double> data, double k, string method)
        {
            var IQR = GetIQR(data, k);
            var result = new List<double>(IQR);

            for (int i = 0; i < result.Count; i++)
            {
                if (double.IsNaN(result[i]))
                {
                    result[i] = LinearInterpolation(result, i);
                }
            }
            
            List<double> resultData = new List<double>();
            if(method == "log")
            {
                resultData = LogTransform(result);
            }
            else if(method == "yeo")
            {
                // для unemployment можливо lambda = 1, тоді не має автокореляції в залишках, але mse високий
                resultData = YeoJohnson(result, -0.5);
            }
            return resultData;
        }
    }
}
