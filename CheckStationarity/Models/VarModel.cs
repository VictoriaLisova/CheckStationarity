using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Solvers;

namespace CheckStationarity.Models
{
    public class VarModel : IModelType
    {
        private MatrixOperations operations;
        public double[,] Errors { get; set; }
        public VarModel()
        {
            operations = new MatrixOperations();
        }
        private List<(string, List<double>)> GetAllData(VarModelItems items)
        {
            var selectedData = new List<(string, List<double>)>();
            if (items.IsGPD) selectedData.Add(new ("gpd", items.GPDData));
            if (items.IsInflation) selectedData.Add(new ("inflation", items.InflationData));
            if (items.IsUnemployment) selectedData.Add(new ("unemployment", items.UnemploymentData));
            if (items.IsCurrency) selectedData.Add(new ("currency", items.CurrencyData));
            return selectedData;
        }

        private double[,] CreateMatrixData(List<(string, List<double>)> data)
        {
            var matrix = new double[data[0].Item2.Count, data.Count];
            for(int i = 0; i < data.Count; i++)
            {
                for(int j = 0; j < data[i].Item2.Count; j++)
                {
                    matrix[j, i] = data[i].Item2[j];
                }
            }
            return matrix;
        }

        private void PrintMatrix(double[,] matrix)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    Console.Write(matrix[i, j] + " ");
                }
                Console.WriteLine();
            }
        }

        private double[,] CretaeYMatrix(double[,] data, int p)
        {
            var Y = new double[data.GetLength(0) - p, data.GetLength(1)];
            for(int i = p; i < data.GetLength(0); i++)
            {
                for(int j = 0; j < data.GetLength(1); j++)
                {
                    Y[i - p, j] = data[i, j];
                }
            }
            return Y;
        }

        private double[,] CreateXMatrix(double[,] data, int p)
        {
            var X = new double[data.GetLength(0) - p, data.GetLength(1) * p + 1];
            for (int i = p; i < data.GetLength(0); i++)
            {
                X[i - p, 0] = 1;
                for (int j = 1; j <= p; j++)
                {
                    for (int k = 0; k < data.GetLength(1); k++)
                    {
                        X[i - p, 1 + (j - 1) * data.GetLength(1) + k] = data[i - j, k];
                    }
                }
            }
            return X;
        }

        private double[,] OLSMethod(double[,] data, int p)
        {
            var Y = CretaeYMatrix(data, p);
            var X = CreateXMatrix(data, p);
            var XT = operations.MatrixTranspose(X);
            var multXTandX = operations.MultipleMatrix(XT, X);
            var multXTandY = operations.MultipleMatrix(XT, Y);

            var inversedX = operations.GetInverseMatrix(multXTandX);
            return operations.MultipleMatrix(inversedX, multXTandY);
        }

        private double[,] BuildVarModel(int p, double[,] B, double[,] data, int t)
        {
            double[,] c = new double[B.GetLength(1), 1];
            for(int i = 0; i < B.GetLength(1); i++)
            {
                c[i, 0] = B[0, i];
            }

            List<double[,]> AMatrix = new List<double[,]>();
            int k = data.GetLength(1);
            for(int lag = 0; lag < p; lag++)
            {
                double[,] A = new double[k, k];
                for(int row = 0; row < k; row++)
                {
                    for(int col = 0; col < k; col++)
                    {
                        A[row, col] = B[1 + col + lag * k, row];
                    }
                }
                AMatrix.Add(A);
            }

            double[,] res = c;
            for(int lag = 0; lag < AMatrix.Count; lag++)
            {
                double[,] yLag = new double[data.GetLength(1), 1];
                for(int i = 0; i < data.GetLength(1); i++)
                {
                    yLag[i, 0] = data[t - 1 - lag, i];
                }
                res = operations.AddMatrix(res, operations.MultipleMatrix(AMatrix[lag], yLag));
            }

            return res;
        }

        private double[,] CalculateEstimate(int p, double[,] B, double[,] data)
        {
            var errors = new double[data.GetLength(0) - p, data.GetLength(1)];
            for(int i = p; i < data.GetLength(0); i++)
            {
                double[,] yPred = BuildVarModel(p, B, data, i);
                for(int j = 0; j < data.GetLength(1); j++)
                {
                    errors[i - p, j] = data[i, j] - yPred[j, 0];
                }
            }
            return errors;
        }

        private double CalculateAIC(int k, int p, int T, double[,] errors)
        {
            var errorsT = operations.MatrixTranspose(errors);
            var mult = operations.MultByNumber(operations.MultipleMatrix(errorsT, errors), 1.0 / T);
            double detErrors = Math.Log(operations.DeterminantLU(mult));
            return detErrors + (2 * k * k * p / T);
        }

        private AICValueVarModel GetParams(int pMax, double[,] data)
        {
            var aicResults = new List<AICValueVarModel>();
            for(int i = 1; i <= pMax; i++)
            {
                var B = OLSMethod(data, i);
                var errors = CalculateEstimate(i, B, data);
                aicResults.Add(new AICValueVarModel
                {
                    P = i,
                    B = B,
                    Value = CalculateAIC(data.GetLength(1), i, data.GetLength(0) - i, errors)
                });
            }
            return aicResults.OrderBy(x => x.Value).First();
        }

        private List<double> ConvertToRealFormat(List<double> forecast, List<double> originalData, int d)
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

        private List<List<double>> ConvertMatrixToArray(List<double[,]> data)
        {
            var result = new List<List<double>>();
            for (int j = 0; j < data[0].GetLength(0); j++)
            {
                result.Add(new List<double>());
            }

            for (int item = 0; item < data.Count; item++)
            {
                for(int j = 0; j < data[item].GetLength(0); j++)
                {
                    result[j].Add(data[item][j, 0]);
                }
            }
            return result;
        }

        private List<(List<double>, string)> Convert(VarModelItems items, List<double[,]> forecast)
        {
            var realForecast = new List<(List<double>, string)>();
            var selectedData = new List<(List<double>, int, string)>();
            if (items.IsGPD) selectedData.Add(new (items.OriginalGPDData, items.GPD_D, items.GPDMethod));
            if (items.IsInflation) selectedData.Add(new (items.OriginalInflationData, items.Inflation_D, items.InflationMethod));
            if (items.IsUnemployment) selectedData.Add(new (items.OriginalUnemploymentData, items.Unemployment_D, items.UnemploymentMethod));
            if (items.IsCurrency) selectedData.Add(new (items.OriginalCurrencyData, items.Currency_D, items.CurrencyMethod));

            var forecastsList = ConvertMatrixToArray(forecast);

            for (int i = 0; i < forecastsList.Count; i++)
            {
                var updated = ConvertToRealFormat(forecastsList[i], selectedData[i].Item1, selectedData[i].Item2);
                realForecast.Add(new(updated, selectedData[i].Item3));
            }

            Console.WriteLine("\nInverted list\n");
            foreach(var item in  realForecast)
            {
                foreach(var i in item.Item1)
                {
                    Console.Write(i + " ");
                }
                Console.WriteLine();
            }
            return realForecast;
        }
        public List<(List<double>, string)> RunVarModel(VarModelItems items, int maxP, int period)
        {
            var finalForecast = new List<double[,]>();
            var selectedDataList = GetAllData(items);
            var matrixData = CreateMatrixData(selectedDataList);
            var param = GetParams(maxP, matrixData);

            var extendedData = new List<double[]>();
            for(int  i = 0; i < matrixData.GetLength(0); i++)
            {
                var row = new double[matrixData.GetLength(1)];
                for(int j = 0; j < matrixData.GetLength(1); j++)
                {
                    row[j] = matrixData[i, j];  
                }
                extendedData.Add(row);
            }

            Console.WriteLine("\nErrors\n");
            var errors = CalculateEstimate(param.P, param.B, matrixData);
            Errors = errors;
            for(int i = 0; i < errors.GetLength(0); i++)
            {
                for(int j = 0; j < errors.GetLength(1); j++)
                {
                    Console.Write(errors[i, j] + " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();

            for(int i = 0; i < period; i++)
            {
                var currentData = new double[extendedData.Count, matrixData.GetLength(1)];
                for(int k = 0; k < extendedData.Count; k++)
                {
                    for(int l = 0; l < matrixData.GetLength(1); l++)
                    {
                        currentData[k, l] = extendedData[k][l];
                    }
                }
                var forecast = BuildVarModel(param.P, param.B, currentData, currentData.GetLength(0));
                finalForecast.Add(forecast);
                Console.WriteLine("Forecast " + (i + 1));
                PrintMatrix(forecast);
                var newRow = new double[matrixData.GetLength(1)];
                for(int r = 0; r < matrixData.GetLength(1); r++)
                {
                    newRow[r] = forecast[r, 0];
                }
                extendedData.Add(newRow);
            }

            return Convert(items, finalForecast);
        }
        public double BuildModel(List<double> data)
        {
            return 0;
        }
    }
}
