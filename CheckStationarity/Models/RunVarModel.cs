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
    public class RunVarModel
    {
        public void RunVar(List<double> gpd, List<double> inflation, List<double> unemployments, 
            List<double> currency, SelectMethod method)
        {
            var prepareGPD = new DataPreparation();
            var updatedGPD = prepareGPD.ProcessOutliners(gpd, 1.5, "log");
            var prepareInflation = new DataPreparation();
            var updatedInflation = prepareInflation.ProcessOutliners(inflation, 1.5, "yeo");
            var prepareUnemployment = new DataPreparation();
            var updatedUnemployment = prepareUnemployment.ProcessOutliners(unemployments, 1.5, "yeo");
            var prepareCurrency = new DataPreparation();
            var updatedCurrency = prepareCurrency.ProcessOutliners(currency, 1.5, "yeo");

            var differentiate = new Differentiate.Differentiate.Differentiate(method.GetIsStationary("kpss"), method.GetIsStationary("adf"));
            var diffGPD = differentiate.GetDiffeentiateTimeLine(updatedGPD, 1);
            int GPD_D = differentiate.d;
            var diffInflation = differentiate.GetDiffeentiateTimeLine(updatedInflation, 1);
            int inflationD = differentiate.d;
            var diffUnemployment = differentiate.GetDiffeentiateTimeLine(updatedUnemployment, 1);
            int unemploymentD = differentiate.d;
            var diffCurrency = differentiate.GetDiffeentiateTimeLine(updatedCurrency, 1);
            int currencyD = differentiate.d;

            // train + test data for gpd
            int gpdTrainSize = (int)(diffGPD.Count * 0.8);
            int gpdOffset = GPD_D;
            var trainGPD_Data = diffGPD.Take(gpdTrainSize - gpdOffset).ToList();
            var testGPD_Data = diffGPD.Skip(gpdTrainSize - gpdOffset).ToList();
            var trainGPD_Original = updatedGPD.Take(gpdTrainSize).ToList();
            var testGPD_Original = updatedGPD.Skip(gpdTrainSize).ToList();

            // train + test data for inflation
            int inflationTrainSize = (int)(diffInflation.Count * 0.8);
            int inflationOffset = inflationD;
            var trainInflation_Data = diffInflation.Take(inflationTrainSize - inflationOffset).ToList();
            var testInflation_Data = diffInflation.Skip(inflationTrainSize - inflationOffset).ToList();
            var trainInflation_Original = updatedInflation.Take(inflationTrainSize).ToList();
            var testInflation_Original = updatedInflation.Skip(inflationTrainSize).ToList();

            // train + test data for unemployment
            int unemploymentTrainSize = (int)(diffUnemployment.Count * 0.8);
            int unemploymentOffset = unemploymentD;
            var trainUnemployment_Data = diffUnemployment.Take(unemploymentTrainSize - unemploymentOffset).ToList();
            var testUnemployment_Data = diffUnemployment.Skip(unemploymentTrainSize - unemploymentOffset).ToList();
            var trainUnemploymen_Original = updatedUnemployment.Take(unemploymentTrainSize).ToList();
            var testUnemployment_Original = updatedUnemployment.Skip(unemploymentTrainSize).ToList();

            // train + test data for currency
            int currencyTrainSize = (int)(diffCurrency.Count * 0.8);
            int currencyOffset = currencyD;
            var trainCurremcy_Data = diffCurrency.Take(currencyTrainSize - currencyOffset).ToList();
            var testCurrency_Data = diffCurrency.Skip(currencyTrainSize - currencyOffset).ToList();
            var trainCurrency_Original = updatedCurrency.Take(currencyTrainSize).ToList();
            var testCurrency_Original = updatedCurrency.Skip(currencyTrainSize).ToList();

            var varModel = new VarModel();
            //var items = new VarModelItems
            //{
            //    IsGPD = true,
            //    IsInflation = true,
            //    IsUnemployment = false,
            //    IsCurrency = false,
            //    GPD_D = GPD_D,
            //    Inflation_D = inflationD,
            //    Unemployment_D = unemploymentD,
            //    Currency_D = currencyD,
            //    GPDData = diffGPD,
            //    InflationData = diffInflation,
            //    UnemploymentData = diffUnemployment,
            //    CurrencyData = diffCurrency,
            //    OriginalGPDData = updatedGPD,
            //    OriginalInflationData = updatedInflation,
            //    OriginalUnemploymentData = updatedUnemployment,
            //    OriginalCurrencyData = updatedCurrency,
            //    GPDMethod = "log",
            //    InflationMethod = "yeo",
            //    UnemploymentMethod = "yeo",
            //    CurrencyMethod = "yeo"
            //};
            var items = new VarModelItems
            {
                IsGPD = true,
                IsInflation = true,
                IsUnemployment = false,
                IsCurrency = false,
                GPD_D = GPD_D,
                Inflation_D = inflationD,
                Unemployment_D = unemploymentD,
                Currency_D = currencyD,
                GPDData = trainGPD_Data,
                InflationData = trainInflation_Data,
                UnemploymentData = diffUnemployment,
                CurrencyData = diffCurrency,
                OriginalGPDData = trainGPD_Original,
                OriginalInflationData = trainInflation_Original,
                OriginalUnemploymentData = updatedUnemployment,
                OriginalCurrencyData = updatedCurrency,
                GPDMethod = "log",
                InflationMethod = "yeo",
                UnemploymentMethod = "yeo",
                CurrencyMethod = "yeo"
            };
            //var forecast = varModel.RunVarModel(items, 2, 5);
            var forecast = varModel.RunVarModel(items, 2, testGPD_Original.Count);
            var updatedForecast = new List<List<double>>();
            var invert = new InvertData();
            foreach (var item in forecast)
            {
                updatedForecast.Add(invert.Invert(item.Item1, item.Item2, -0.5, prepareGPD.Shift));
            }

            Console.WriteLine("\nКінцевий прогноз\n");
            foreach (var item in updatedForecast)
            {
                foreach (var i in item)
                {
                    Console.Write(i + " ");
                }
                Console.WriteLine();
            }

            var testforecast = new List<double>();
            foreach (var item in forecast)
            {
                testforecast.AddRange(item.Item1);
            }

            var testData = new List<double>();
            testData.AddRange(testGPD_Original);
            testData.AddRange(testInflation_Original);

            var checker = new CheckModelResults();
            Console.WriteLine("\nMSE: ");
            Console.Write(checker.GetMSE(testInflation_Original, forecast[1].Item1) + "\n");

            Console.WriteLine("\nMAE: ");
            Console.Write(checker.GetMAE(testInflation_Original, forecast[1].Item1) + "\n");

            Console.WriteLine("\nMAPE: ");
            Console.Write(checker.GetMAPE(testInflation_Original, forecast[1].Item1) + "\n");

            Console.WriteLine("\nRMSE: ");
            Console.Write(checker.GetRMSE(testInflation_Original, forecast[1].Item1) + "\n");


            var errorsList = new List<double>();
            for (int i = 0; i < varModel.Errors.GetLength(0); i++)
            {
                for (int j = 0; j < varModel.Errors.GetLength(1); j++)
                {
                    errorsList.Add(varModel.Errors[i, j]);
                }
            }
            var checkErrors = new CheckErrors(errorsList.Skip((int)(errorsList.Count / 2.0)).ToList());
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
