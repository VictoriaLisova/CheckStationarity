using CheckStationarity.Check;
using CheckStationarity.Data_Inversion;
using CheckStationarity.Data_Preparation;
using CheckStationarity.Differentiate.Differentiate;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckStationarity.Models
{
    public class RunArimaModel
    {
        public List<double> Errors {  get; set; } = new List<double>();
        public List<double> Forecast { get; set; } = new List<double>();
        public List<double> RealValues { get; set; } = new List<double>();
        public void ExecuteArima(List<double> data, string methodName, SelectMethod method)
        {
            var dataPreparation = new DataPreparation();
            var updatedData = dataPreparation.ProcessOutliners(data, 1.5, methodName);

            Console.WriteLine("Оброблені дані\n");
            foreach (var i in updatedData)
            {
                Console.WriteLine(i);
            }
            Console.WriteLine();

            // диференціювання
            var differentiate = new Differentiate.Differentiate.Differentiate(method.GetIsStationary("kpss"), method.GetIsStationary("adf"));
            var diffData = differentiate.GetDiffeentiateTimeLine(updatedData, 1);

            Console.WriteLine("Диференційовані дані\n");
            foreach (var item in diffData)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine();

            // розбиття на тестову і навчальну вибірки
            int trainSize = (int)(diffData.Count * 0.8);
            int offset = differentiate.d;

            var trainData = diffData.Take(trainSize - offset).ToList();
            var testData = diffData.Skip(trainSize - offset).ToList();

            var trainOriginal = updatedData.Take(trainSize).ToList();
            var testOriginal = updatedData.Skip(trainSize).ToList();

            // побудова моделі
            var arima = new ArimaModel();
            var mle = new MLEMethod(4, 4, arima, differentiate.d);
            //var paramsModel = mle.GetParams(diffData);
            var paramsModel = mle.GetParams(trainData);

            Console.WriteLine($"Найкраща модель: ARIMA({paramsModel.P},{differentiate.d},{paramsModel.Q})");
            Console.WriteLine($"AICc = {paramsModel.Value:F4}");
            Console.WriteLine($"Fi: {string.Join(", ", paramsModel.Fi.Select(f => f.ToString("F4")))}");
            Console.WriteLine($"Tao: {string.Join(", ", paramsModel.Tao.Select(t => t.ToString("F4")))}");

            //здійснення прогнозу
            var forecasts = mle.Forecast(paramsModel.P,
                differentiate.d, paramsModel.Q,
                testOriginal.Count, paramsModel.Fi,
                paramsModel.Tao, trainData, trainOriginal);

            //var forecasts = mle.Forecast(paramsModel.P,
            //   differentiate.d, paramsModel.Q,
            //   5, paramsModel.Fi,
            //   paramsModel.Tao, diffData, updatedData);

            var invert = new InvertData();
            var updatedForecasts = invert.Invert(forecasts, methodName, -0.5, dataPreparation.Shift);

            Console.WriteLine("\nПрогноз\n");
            foreach (var f in updatedForecasts)
            {
                Console.WriteLine(f);
            }

            Forecast = forecasts;

            Console.WriteLine("\nПохибки\n");
            var checkErrors = new CheckErrors(mle.Errors);
            Errors = mle.Errors;
            foreach (var error in mle.Errors)
            {
                Console.WriteLine(error);
            }

            //перевірка результатів
            var checker = new CheckModelResults();

            Console.Write("\nMSE: ");
            Console.Write(checker.GetMSE(testOriginal, forecasts) + "\n");

            Console.Write("\nMAE: ");
            Console.Write(checker.GetMAE(testOriginal, forecasts) + "\n");

            Console.Write("\nMAPE: ");
            Console.Write(checker.GetMAPE(testOriginal, forecasts) + "%\n");

            Console.Write("\nRMSE: ");
            Console.WriteLine(checker.GetRMSE(testOriginal, forecasts) + "\n");

            RealValues = testOriginal;

            // перевірка залишків
            Console.WriteLine("\nПеревірка залишків\n");
            Console.WriteLine("Середнє: " + checkErrors.GetErrorAverage());
            Console.WriteLine("Дисперсія: " + checkErrors.GetSigmaSquare());
            var acfsList = checkErrors.ErrorsACF(20);
            for (int a = 0; a < acfsList.Count; a++)
            {
                Console.WriteLine("Lag: " + (a + 1) + " ACF: " + acfsList[a]);
            }
            Console.WriteLine("Ljung-Box Test: " + checkErrors.LjungBoxTest(20));
        }
    }
}
