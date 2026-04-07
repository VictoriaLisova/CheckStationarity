using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace CheckStationarity.Models
{
    public class MLEMethod
    {
        private int maxP;
        private int maxQ;
        private int d;
        private double resMu;

        private List<double> resFi;
        private List<double> resTao;
        private ArimaModel arimaModel;
        public List<double> Errors {  get; set; }

        public MLEMethod(int p, int q, ArimaModel model, int d)
        {
            arimaModel = model;
            maxP = p;
            maxQ = q;
            this.d = d;
            resFi = new List<double>();
            resTao = new List<double>();
            Errors = new List<double>();
        }

        private List<double> CalculateEstimate(int p, int q, List<double> fi, List<double> tao, double mu, List<double> data)
        {
            var errors = new List<double>();
            int start = Math.Max(p, q);

            for (int i = start; i < data.Count; i++)
            {
                errors.Add(data[i] - (mu + arimaModel.CalculateArima(i, p, q, fi, tao, errors, data)));
            }
            return errors;
        }

        private double ListSquare(List<double> data)
        {
            return data.Sum(x => x * x);
        }

        private double CalculateMLEValue(List<double> errors)
        {
            var errorSquaes = ListSquare(errors);
            int n = errors.Count;
            double dispersion = errorSquaes / errors.Count;
            if (dispersion <= 0 || double.IsNaN(dispersion))
                return double.MinValue;

            return -(n / 2.0) * Math.Log(2 * Math.PI * dispersion) - errorSquaes / (2.0 * dispersion);
        }

        private void GetFinandTao(int p, int q, List<double> data)
        {
            int paramCount = p + q + 1;
            var intialValues = Vector<double>.Build.Dense(paramCount, 0.1);

            Func<Vector<double>, double> objective = parameters =>
            {
                var fi = parameters.SubVector(0, p).ToList();
                var tao = parameters.SubVector(p, q).ToList();
                double muLocal = parameters[p + q];
                var errors = CalculateEstimate(p, q, fi, tao, muLocal, data);
                return -CalculateMLEValue(errors);
            };

            var objfunction = ObjectiveFunction.Value(objective);
            var solver = new NelderMeadSimplex(1e-6, 10000);
            var result = solver.FindMinimum(objfunction, intialValues);
            var optParams = result.MinimizingPoint;

            resFi = optParams.SubVector(0, p).ToList();
            resTao = optParams.SubVector(p, q).ToList();
            resMu = optParams[p + q];
        }

        public AICValueModel GetParams(List<double> data)
        {
            var aic = new AICcValue();
            var aicValues = new List<AICValueModel>();
            for (int p = 1; p <= maxP; p++)
            {
                for (int q = 1; q <= maxQ; q++)
                {
                    GetFinandTao(p, q, data);

                    var errors = CalculateEstimate(p, q, resFi, resTao, resMu, data);

                    aicValues.Add(new AICValueModel
                    {
                        P = p,
                        Q = q,
                        Value = aic.CalcAIC(ListSquare(errors) / errors.Count, p, q, errors.Count),
                        Fi = new List<double>(resFi),
                        Tao = new List<double>(resTao)
                    });
                }
            }
            return aic.GetMin(aicValues);
        }

        private List<double> ConvertToRealFormat(List<double> forecast, List<double> originalData)
        {
            var realForecast = new List<double>();
            if (d == 0)
            {
                return forecast;
            }
            else if (d == 1)
            {
                double lastOriginalValue = originalData.Last();
                foreach (var item in forecast)
                {
                    lastOriginalValue += item;
                    realForecast.Add(lastOriginalValue);
                }
            }
            else if (d == 2)
            {
                double originalLastDiff = originalData.Last() - originalData[originalData.Count - 2];
                double originalLast = originalData.Last();
                foreach (var item in forecast)
                {
                    double diff1 = item + originalLastDiff;
                    double value = originalLast + diff1;
                    realForecast.Add(value);
                    originalLastDiff = diff1;
                    originalLast = value;
                }
            }
            return realForecast;
        }

        public List<double> Forecast(int p, int d, int q, int period, List<double> fi, List<double> tao,
            List<double> data, List<double> originalData)
        {
            var forecast = new List<double>();
            var errors = CalculateEstimate(p, q, fi, tao, resMu, data);

            var tempData = new List<double>(data);
            Errors = new List<double>(errors);
            var tempErrors = new List<double>(errors);

            for (int per = 0; per < period; per++)
            {
                int nextIndex = tempData.Count;
                double AR = 0;
                for (int i = 0; i < p; i++)
                {
                    int index = nextIndex - i - 1;
                    if (index >= 0 && index < tempData.Count && i < fi.Count)
                    {
                        AR += fi[i] * tempData[index];
                    }
                }

                double MA = 0;
                for (int j = 0; j < q; j++)
                {
                    int index = tempErrors.Count - 1 - j;
                    if (index >= 0 && j < tao.Count)
                    {
                        MA += tao[j] * tempErrors[index];
                    }
                }
                double nextValue = resMu + AR + MA;
                forecast.Add(nextValue);

                tempData.Add(nextValue);
                tempErrors.Add(0);
            }

            return ConvertToRealFormat(forecast, originalData);
        }
    }
}
