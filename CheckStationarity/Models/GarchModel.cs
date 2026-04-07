using MathNet.Numerics.Optimization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;

namespace CheckStationarity.Models
{
    public class GarchModel : IModelType
    {
        private List<double> Alpha {  get; set; }
        private List<double> Beta {  get; set; }
        private double W {  get; set; }
        public GarchModel()
        {
            Alpha = new List<double>();
            Beta = new List<double>();
        }
        private double GarchEquation(List<double> errors, List<double> sigma2, 
            List<double> alpha, List<double> beta, double w, int q, int p, int t)
        {
            double archPart = 0;
            for(int i = 0; i < q; i++)
            {
                if(i < alpha.Count && t - i - 1 >= 0 && t - i - 1 < errors.Count)
                {
                    archPart += alpha[i] * Math.Pow(errors[t - i - 1], 2);
                }
            }

            double garchPart = 0;
            for(int j = 0; j < p; j++)
            {
                if (j < beta.Count && t - j - 1 >= 0 && t - j - 1 < sigma2.Count)
                {
                    garchPart += beta[j] * sigma2[t - j - 1];
                }
            }

            return w + archPart + garchPart;
        }

        private List<double> CalculateVar(List<double> errors, List<double> alpha, List<double> beta, double w, int q, int p)
        {
            var sigma2 = new List<double>();
            sigma2.Add(errors.Variance());
            for (int i = 1; i < errors.Count; i++)
            {
                var value = GarchEquation(errors, sigma2, alpha, beta, w, q, p, i);
                sigma2.Add(value);
            }
            return sigma2;
        }

        private double MLE(List<double> sigma2, List<double> errors)
        {
            int T = errors.Count;
            double sum = 0;
            for(int i = 0; i < T; i++)
            {
                sum += Math.Log(2 * Math.PI) + Math.Log(sigma2[i]) + Math.Pow(errors[i], 2) / sigma2[i];
            }
            return -0.5 * sum;
        }

        private void GetAlphaandBeta(int q, int p, List<double> errors)
        {
            int paramsAmount = q + p + 1;
            var intialValues = Vector<double>.Build.Dense(paramsAmount, 0.1);

            Func<Vector<double>, double> objective = parameters =>
            {
                var alpha = parameters.SubVector(0, q).ToList();
                var beta = parameters.SubVector(q, p).ToList();
                double wLocal = parameters[q + p];

                if(alpha.Any(a => a < 0) || beta.Any(b => b < 0) 
                            || alpha.Sum() + beta.Sum() >= 1 || wLocal <= 0)
                    return double.MaxValue;

                var sigma2 = CalculateVar(errors, alpha, beta, wLocal, q, p);
                return -MLE(sigma2, errors);
            };

            var objfunction = ObjectiveFunction.Value(objective);
            var solver = new NelderMeadSimplex(1e-6, 10000);
            var result = solver.FindMinimum(objfunction, intialValues);
            var optParams = result.MinimizingPoint;

            Alpha = optParams.SubVector(0, q).ToList();
            Beta = optParams.SubVector(q, p).ToList();
            W = optParams[q + p];
        }

        private AICGarchModel GetParams(int maxQ, int maxP, List<double> errors)
        {
            var aicResults = new List<AICGarchModel>();
            for(int q = 1; q <= maxQ; q++)
            {
                for(int p = 1; p <= maxP; p++)
                {
                    GetAlphaandBeta(q, p, errors);
                    var sigma2 = CalculateVar(errors, Alpha, Beta, W, q, p);

                    double logLikelihood = MLE(sigma2, errors);
                    aicResults.Add(new AICGarchModel
                    {
                        P = p,
                        Q = q,
                        W = W,
                        Alpha = Alpha,
                        Beta = Beta,
                        Value = -2 * logLikelihood + 2 * (q + p + 1)
                    });
                }
            }
            return aicResults.OrderBy(a => a.Value).First();
        }

        public List<double> GarchForecast(List<double> errors, int maxQ, int maxP, int period)
        {
            var modelParams = GetParams(maxQ, maxP, errors);
            Console.WriteLine($"\nGARCH({modelParams.Q}, {modelParams.P})\n");

            Console.WriteLine("Alpha + Beta = " + (modelParams.Alpha.Sum() + modelParams.Beta.Sum()));
            var forecast = new List<double>();
            var sigma2 = CalculateVar(errors, modelParams.Alpha, modelParams.Beta, modelParams.W, modelParams.Q, modelParams.P);

            var tempErrors = new List<double>(errors);
            var temsSigma2 = new List<double>(sigma2);

            for (int i = 0; i < period; i++)
            {
                double nextSigma = 0;
                nextSigma = GarchEquation(tempErrors, temsSigma2, modelParams.Alpha, modelParams.Beta,
                        modelParams.W, modelParams.Q, modelParams.P, tempErrors.Count);
                forecast.Add(nextSigma);
                temsSigma2.Add(nextSigma);
                tempErrors.Add(0);
            }

            return forecast;
        }

        public double BuildModel(List<double> data)
        {
            return 0;
        }
    }
}
