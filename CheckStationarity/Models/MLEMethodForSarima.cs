using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CheckStationarity.Models
{
    public class MLEMethodForSarima
    {
        private readonly SarimaModel sarimaModel;

        private int maxPNonSeasonal;
        private int maxPSeasonal;
        private int maxQNonSeasonal;
        private int maxQSeasonal;
        private int d;
        private int D;
        private int s;

        private List<double> resFiNonSeasonal;
        private List<double> resFiSeasonal;
        private List<double> resTaoNonSeasonal;
        private List<double> resTaoSeasonal;
        private double c;

        public List<double> Errors { get; set; }
        public MLEMethodForSarima(SarimaModel sarimaModel, int p, int P, int q, int Q, int d, int D, int s)
        {
            this.sarimaModel = sarimaModel;
            maxPNonSeasonal = p;
            maxPSeasonal = P;
            maxQNonSeasonal = q;
            maxQSeasonal = Q;
            this.d = d;
            this.D = D;
            this.s = s;
            resFiNonSeasonal = new List<double>();
            resFiSeasonal = new List<double>();
            resTaoNonSeasonal = new List<double>();
            resTaoSeasonal = new List<double>();
        }

        public List<double> CalculateEstimates(int p, int P, int q, int Q, 
            List<double> fi, List<double> Fi, List<double> tao, List<double> Tao, List<double> data, double c)
        {
            // for inflation, unemployment
            //int start = Math.Max(Math.Max(p + d, q), Math.Max(P * s + D * s, Q * s));

            // for others
            int start = Math.Max(Math.Max(p, q), Math.Max(P * s, Q * s));
            var errors = Enumerable.Repeat(0.0, data.Count).ToList();

            for (int i = start; i < data.Count; i++)
            {
                double predicted = c + sarimaModel.BuildSarimaModel(i, p, P, q, Q, errors, fi, Fi, tao, Tao, data);
                errors[i] = data[i] - predicted;
            }
            return errors;
        }

        private double ListSumOfSquares(List<double> data)
        {
            return data.Sum(x => x * x);
        }

        private double CalculateMLEValue(List<double> errors)
        {
            var errorSquares = ListSumOfSquares(errors);
            int n = errors.Count;
            double dispersion = errorSquares / n;
            if (dispersion < 1e-10 || double.IsNaN(dispersion))
            {
                return double.MinValue;
            }
            return -(n / 2.0) * Math.Log(2 * Math.PI * dispersion) - errorSquares / (2.0 * dispersion);
        }

        private void GetCoefficients(int p, int P, int q, int Q, List<double> data)
        {
            int paramCount = p + P + q + Q + 1;
            var intialValues = Vector<double>.Build.Dense(paramCount, 0.01);

            Func<Vector<double>, double> objective = parameters =>
            {
                var fiNonSeasonal = parameters.SubVector(0, p).ToList();
                var fiSeasonal = parameters.SubVector(p, P).ToList();
                var taoNonSeasonal = parameters.SubVector(p + P, q).ToList();
                var taoSeasonal = parameters.SubVector(p + P + q, Q).ToList();
                double cLocal = parameters.Last();
                var errors = CalculateEstimates(p, P, q, Q, fiNonSeasonal, fiSeasonal, taoNonSeasonal, taoSeasonal, data, cLocal);
                return -CalculateMLEValue(errors);
            };

            var objfunction = ObjectiveFunction.Value(objective);
            var solver = new NelderMeadSimplex(1e-7, 200000);

            var result = solver.FindMinimum(objfunction, intialValues);
            var optParams = result.MinimizingPoint;

            resFiNonSeasonal = optParams.SubVector(0, p).ToList();
            resFiSeasonal = optParams.SubVector(p, P).ToList();
            resTaoNonSeasonal = optParams.SubVector(p + P, q).ToList();
            resTaoSeasonal = optParams.SubVector(p + P + q, Q).ToList();
            c = optParams.Last();
        }
        public AICcValueSarima GetParams(List<double> data)
        {
            var aic = new AICcValue();
            var aicValues = new List<AICcValueSarima>();
            for(int p = 1; p <= maxPNonSeasonal; p++)
            {
                for(int q = 1; q <= maxQNonSeasonal; q++)
                {
                    for(int P = 1; P <= maxPSeasonal; P++)
                    {
                        for(int Q = 1; Q <= maxQSeasonal; Q++)
                        {
                            GetCoefficients(p, P, q, Q, data);    
                            var errors = CalculateEstimates(p, P, q, Q, resFiNonSeasonal, resFiSeasonal, resTaoNonSeasonal, resTaoSeasonal, data, c);
                            aicValues.Add(new AICcValueSarima
                            {
                                P = p,
                                Q = q,
                                PSeasonal = P,
                                QSeasonal = Q,
                                C = c,
                                Value = aic.CalcAIC(ListSumOfSquares(errors) / errors.Count, p + P, q + Q, errors.Count),
                                Fi = new List<double>(resFiNonSeasonal),
                                Tao = new List<double>(resTaoNonSeasonal),
                                FiSeasonal = new List<double>(resFiSeasonal),
                                TaoSeasonal = new List<double>(resTaoSeasonal)
                            });
                        }
                    }
                }
            }
            return aicValues.OrderBy(x => x.Value).First();
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
        private List<double> InvertSeasonal(List<double> forecast, List<double> originalData)
        {
            if (D == 0) return forecast;

            var result = new List<double>();
            var history = new List<double>(originalData);

            if (D == 1)
            {
                foreach (var f in forecast)
                {
                    double value = f + history[history.Count - s];
                    result.Add(value);
                    history.Add(value);
                }
            }
            else if (D == 2)
            {
                var seasonalDiffHistory = new List<double>();
                for (int i = s; i < originalData.Count; i++)
                    seasonalDiffHistory.Add(originalData[i] - originalData[i - s]);

                foreach (var f in forecast)
                {
                    double lastSeasonalDiff = seasonalDiffHistory[seasonalDiffHistory.Count - s];
                    double diff1 = f + lastSeasonalDiff;
                    seasonalDiffHistory.Add(diff1);

                    double value = diff1 + history[history.Count - s];
                    result.Add(value);
                    history.Add(value);
                }
            }
            return result;
        }

        public List<double> Forecast(int p, int P, int q, int Q, int period, 
            List<double> fi, List<double> Fi, List<double> tao, List<double> Tao, List<double> originalData, 
            List<double> diffNonSeasonalData, List<double> data, double c)
        {
            var forecast = new List<double>();
            var errors = CalculateEstimates(p, P, q, Q, fi, Fi, tao, Tao, data, c);

            foreach (var er in errors)
            {
                Console.Write(er + " ");
            }
            Console.WriteLine("\n");

            var tempData = new List<double>(data);
            var tempErrors = new List<double>(errors);

            Errors = tempErrors;

            for (int per = 0; per < period; per++)
            {
                int nextIndex = tempData.Count;
                var nextValue = c + sarimaModel.BuildSarimaModel(nextIndex, p, P, q, Q, tempErrors, fi, Fi, tao, Tao, tempData);
                forecast.Add(nextValue);

                tempData.Add(nextValue);
                tempErrors.Add(0.0);
            }

            var afterD = InvertSeasonal(forecast, diffNonSeasonalData);
            return ConvertToRealFormat(afterD, originalData);
        }
    }
}
