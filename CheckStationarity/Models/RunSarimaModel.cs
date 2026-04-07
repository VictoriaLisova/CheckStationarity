using CheckStationarity.Check;
using CheckStationarity.Data_Inversion;
using CheckStationarity.Data_Preparation;
using CheckStationarity.Differentiate.Differentiate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckStationarity.Models
{
    public class RunSarimaModel
    {
        public void RunSarima(List<double> data, string methodType, SelectMethod method)
        {
            var dataPreparation = new DataPreparation();
            var updatedData = dataPreparation.ProcessOutliners(data, 1.5, methodType);

            int rawTrainSize = (int)(updatedData.Count * 0.8);

            // параметри диференціювання
            var differentiateNonSeasonal = new Differentiate.Differentiate.Differentiate(method.GetIsStationary("kpss"), method.GetIsStationary("adf"));
            var diffNonSeasonalData = differentiateNonSeasonal.GetDiffeentiateTimeLine(updatedData, 1);
            Console.WriteLine("d = " + differentiateNonSeasonal.d);
            int dSarima = differentiateNonSeasonal.d;

            // диференціювання
            int s = 4;
            var differentiateSeasonal = new Differentiate.Differentiate.Differentiate(method.GetIsStationary("kpss"), method.GetIsStationary("adf"));
            var diffSeasonalData = differentiateSeasonal.GetDiffeentiateTimeLine(updatedData, s);
            Console.WriteLine("D = " + differentiateSeasonal.d);
            int DSarima = differentiateSeasonal.d;

            var finallDiffData = new List<double>(updatedData);
            for (int d = 0; d < dSarima; d++)
            {
                var temp = new List<double>();
                for (int ind = 1; ind < finallDiffData.Count; ind++)
                {
                    temp.Add(finallDiffData[ind] - finallDiffData[ind - 1]);
                }
                finallDiffData = temp;
            }

            var finallSeasonalDiffData = new List<double>(finallDiffData);
            for (int D = 0; D < DSarima; D++)
            {
                var temp = new List<double>();
                for (int j = s; j < finallSeasonalDiffData.Count; j++)
                {
                    temp.Add(finallSeasonalDiffData[j] - finallSeasonalDiffData[j - s]);
                }
                finallSeasonalDiffData = temp;
            }

            // поділ на навчальні і тренувальні дані
            int trainSize = (int)(finallSeasonalDiffData.Count * 0.8);
            int offset = dSarima + DSarima;

            var trainData = finallSeasonalDiffData.Take(trainSize - offset).ToList();
            var testData = finallSeasonalDiffData.Skip(trainSize - offset).ToList();

            var trainOriginal = updatedData.Take(rawTrainSize).ToList();
            var testOriginal = updatedData.Skip(rawTrainSize).ToList();

            var sarima = new SarimaModel(s);
            var sarimaMLE = new MLEMethodForSarima(sarima, 2, 1, 2, 1, dSarima, DSarima, s);
            //var sarimaParams = sarimaMLE.GetParams(finallSeasonalDiffData);
            var sarimaParams = sarimaMLE.GetParams(trainData);

            Console.WriteLine($"Найкраща модель: SARIMA({sarimaParams.P},{dSarima},{sarimaParams.Q})" +
                $"({sarimaParams.PSeasonal},{DSarima},{sarimaParams.QSeasonal})[{s}]");
            Console.WriteLine($"AICc = {sarimaParams.Value:F4}");
            Console.WriteLine($"Fi: {string.Join(", ", sarimaParams.Fi.Select(f => f.ToString("F4")))}");
            Console.WriteLine($"Tao: {string.Join(", ", sarimaParams.Tao.Select(t => t.ToString("F4")))}");

            //var forecasts = sarimaMLE.Forecast(sarimaParams.P, sarimaParams.PSeasonal, sarimaParams.Q, sarimaParams.QSeasonal,
            //    5, sarimaParams.Fi, sarimaParams.FiSeasonal, sarimaParams.Tao, sarimaParams.TaoSeasonal, updatedData, finallDiffData, finallSeasonalDiffData, sarimaParams.C);
            var forecasts1 = sarimaMLE.Forecast(sarimaParams.P, sarimaParams.PSeasonal, sarimaParams.Q, sarimaParams.QSeasonal,
                testOriginal.Count, sarimaParams.Fi, sarimaParams.FiSeasonal, sarimaParams.Tao, sarimaParams.TaoSeasonal, trainOriginal, finallDiffData, trainData, sarimaParams.C);

            var invert = new InvertData();
            var finalForecast = invert.Invert(forecasts1, methodType, -0.5, dataPreparation.Shift);
            foreach (var forecast in finalForecast)
            {
                Console.WriteLine(forecast);
            }

            Console.Write("\nMSE: ");
            var checker = new CheckModelResults();
            Console.WriteLine(checker.GetMSE(testOriginal, forecasts1));

            Console.Write("\nMAE: ");
            Console.Write(checker.GetMAE(testOriginal, forecasts1) + "\n");

            Console.Write("\nMAPE: ");
            Console.Write(checker.GetMAPE(testOriginal, forecasts1) + "%\n");

            Console.Write("\nRMSE: ");
            Console.WriteLine(checker.GetRMSE(testOriginal, forecasts1) + "\n");

            var checkErrors = new CheckErrors(sarimaMLE.Errors);
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
