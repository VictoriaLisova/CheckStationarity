using CheckStationarity.Check;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckStationarity.Models
{
    public class RunGarch
    {
        public void ExecuteGarch(RunArimaModel runArima)
        {
            var garch = new GarchModel();
            var forecast = garch.GarchForecast(runArima.Errors, 4, 4, runArima.Forecast.Count);

            Console.WriteLine("\nForecast\n");
            foreach (var f in forecast)
            {
                Console.Write(f + " ");
            }
            Console.WriteLine();

            double errorMean = runArima.Errors.Average();

            double errorVariance = runArima.Errors
                .Select(e => Math.Pow(e - errorMean, 2))
                .Average();

            var checkErrors = new CheckErrors(runArima.Errors);
            var acf = checkErrors.ErrorsACF(20);
            for (int a = 0; a < acf.Count; a++)
            {
                Console.WriteLine("Lag: " + (a + 1) + " ACF: " + acf[a]);
            }
            Console.WriteLine("Ljung-Box Test: " + checkErrors.LjungBoxTest(20));

            var intervals = new List<(double, double, double)>();
            double z = 1.96;
            for (int i = 0; i < runArima.Forecast.Count; i++)
            {
                double mean = runArima.Forecast[i];
                double sigma = Math.Sqrt(forecast[i]);

                double lower = mean - z * sigma;
                double upper = mean + z * sigma;
                intervals.Add((lower, runArima.Forecast[i], upper));
            }

            Console.WriteLine("\nІнтервали довіри\n");
            foreach (var interval in intervals)
            {
                Console.WriteLine(interval.Item1 + " - " + interval.Item2 + " - " + interval.Item3);
            }

            int hits = 0;
            foreach (var real in runArima.RealValues)
                Console.WriteLine("Real value " + real);

            for (int i = 0; i < intervals.Count; i++)
            {
                if (runArima.RealValues[i] >= intervals[i].Item1 &&
                    runArima.RealValues[i] <= intervals[i].Item3)
                {
                    hits++;
                }
            }

            double coverage = (double)hits / intervals.Count;
            Console.WriteLine("Coverage: " + coverage);
        }
    }
}
