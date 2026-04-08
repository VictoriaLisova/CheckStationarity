using CheckStationarity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit.Sdk;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CheckStationarityTest
{
    public class ModelsTest
    {
        private AICcValue aic = new AICcValue();
        private MatrixOperations operations = new MatrixOperations();

        [Fact]
        public void CalcAICTest()
        {
            double res = aic.CalcAIC(0.4, 2, 5, 10);
            Assert.Equal(150.837, Math.Round(res, 3));
        }

        [Fact]
        public void GetMinTest()
        {
            var results = new List<AICValueModel>
            {
                new AICValueModel
                {
                    P = 1,
                    Q = 3, 
                    Fi = new List<double>{ 0.01 },
                    Tao = new List<double> {0.23, 0.544, 0.134 },
                    Value = -130
                },
                new AICValueModel
                {
                    P = 1,
                    Q = 1,
                    Fi = new List<double>{ 0.01 },
                    Tao = new List<double> {0.23 },
                    Value = -260
                },
            };
            var min = aic.GetMin(results);
            Assert.Equal(-260, min.Value);
            Assert.Equal(1, min.P);
            Assert.Equal(1, min.Q);
            Assert.Equal(0.01, min.Fi.First());
            Assert.Equal(0.23, min.Tao.First());
        }

        [Fact]
        public void CalculateARIMATest()
        {
            var data = new List<double> { 1, 2, 3, 4 };
            var errors = new List<double> { 0.5, 0.5, 0.5, 0.5 };

            var fi = new List<double> { 0.6 };
            var tao = new List<double> { 0.4 };
            int start = 2;
            int p = 1;
            int q = 1;

            var model = new ArimaModel();
            double result = model.CalculateArima(start, p, q, fi, tao, errors, data);

            double expected = 1.4;

            Assert.Equal(expected, result, 6);
        }

        [Fact]
        public void CalculateGARCHTest()
        {
            var errors = new List<double> { 1, 2, 3, 4 };
            var sigma2 = new List<double> { 1, 1, 1, 1 };

            var alpha = new List<double> { 1 };
            var beta = new List<double> { 0 };
            double w = 0.6;

            var method = new GarchModel();
            var res = method.GarchForecast(errors, 1, 1, 1);

            Assert.Equal(16.6, Math.Round(res.First(), 1));
            Assert.Equal(w, Math.Round(method.W, 1));
            Assert.Equal(alpha.First(), Math.Round(method.Alpha.First(), 2));
            Assert.Equal(beta.First(), Math.Round(method.Beta.First(), 2));
        }

        private void MatrixCompare(double[,] expectedMatrix, double[,] actualMatrix)
        {
            Assert.Equal(expectedMatrix.GetLength(0), actualMatrix.GetLength(0));
            Assert.Equal(expectedMatrix.GetLength(1), actualMatrix.GetLength(1));

            for (int i = 0; i < expectedMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < expectedMatrix.GetLength(1); j++)
                {
                    Assert.Equal(Math.Round(expectedMatrix[i, j], 4), Math.Round(actualMatrix[i, j], 4));
                }
            }
        }

        [Fact]
        public void MatrixTransposeTest()
        {
            var matrix = new double[,] { { 1, 2, 3 }, { 4, 5, 6 } };
            var actualMatrix = operations.MatrixTranspose(matrix);
            var expectedMatrix = new double[,] { {1, 4 }, { 2, 5 }, { 3, 6 } };

            MatrixCompare(expectedMatrix, actualMatrix);
        }

        public static IEnumerable<object[]> MultipleMatrixData =>
            new List<object[]>
            {
                new object[] { new double[,] { { 1, 2, 3 }, { 4, 5, 6 } }, new double[,] { { 7, 8 }, { 9, 10 }, { 11, 12 } }, 
                               new double[,] { { 58, 64 }, { 139, 154 } } },
                new object[] { new double[,] { { 9, 6, 3 }, { 2, 6, 1 }, { 8, 1, 1 } }, new double[,] { { 1, 2 }, { 3, 8 } },
                               new double[3, 2]}
            };

        [Theory]
        [MemberData(nameof(MultipleMatrixData))]
        public void MultipleMatrixTest(double[,] matrix1, double[,] matrix2, double[,] expectedMatrix)
        {
            var actualMatrix = operations.MultipleMatrix(matrix1, matrix2);

            MatrixCompare(expectedMatrix, actualMatrix);
        }

        public static IEnumerable<object[]> DeterminantMatrixData =>
            new List<object[]>
            {
                new object[] { new double[,] { { 2, 7 }, { 4, 5 } }, -18 },
                new object[] { new double[,] { { 1, 2}, { 2, 4 } }, 0 }
            };

        [Theory]
        [MemberData(nameof(DeterminantMatrixData))]
        public void DeterminantLUTest(double[,] matrix, double expectedDet)
        {
            double actualDet = operations.DeterminantLU(matrix);

            Assert.Equal(expectedDet, actualDet);
        }

        [Fact]
        public void DeterminantLUIncorectMatrixTest()
        {
            var matrix = new double[,] { { 1, 3, 5 }, { 12, 7, 2 }, { 9, 4, 2 }, { 1, 5, 9 } };

            Assert.Throws<IndexOutOfRangeException>(() => operations.DeterminantLU(matrix));
        }

        [Fact]
        public void GetInverseIncorectMatrixTest()
        {
            var matrix = new double[,] { { 1, 2 }, { 2, 4 } };
            Assert.Throws<Exception>(() => operations.GetInverseMatrix(matrix));
        }

        [Fact]
        public void GetInverseMatrixCorrectMatrixTest()
        {
            var matrix = new double[,] { { 2, 5, 7, 3 }, { 0, 4, 1, 5 }, { 9, 3, 0, 1 }, { 6, 2, 1, 3 } };

            var actualMatrix = operations.GetInverseMatrix(matrix);
            var expectedMatrix = new double[,] { { -1.0 / 148.0, -45.0 / 592.0, 9.0 / 296.0, 73.0 / 592.0 },
                                                 { 3.0 / 74.0, 61.0 / 296.0, 47.0 / 148.0, -145.0 / 296.0 },
                                                 { 21.0 / 148.0 , -91.0 / 592.0, -41.0 / 296.0, 95.0 / 592.0 },
                                                 { -9.0 / 148.0, 39.0 / 592.0, -67.0 / 296.0, 213.0 / 592.0 } };

            MatrixCompare(actualMatrix, expectedMatrix);
        }

        [Fact]
        public void AddCorrectMatrixTest()
        {
            var matrix1 = new double[,] { { 1, 2 }, { 3, 4 } };
            var matrix2 = new double[,] { { 5, 6 }, { 7, 8 } };

            var expectedMatrix = new double[,] { { 6, 8 }, { 10, 12 } };
            var actualMatrix = operations.AddMatrix(matrix1, matrix2);

            MatrixCompare(expectedMatrix, actualMatrix);
        }

        [Fact]
        public void AddIncorrectMatrixTest()
        {
            var matrix1 = new double[,] { { 1, 2, 3 }, { 4, 5, 6 } };
            var matrix2 = new double[,] { { 7, 8 }, { 9, 10}, { 11, 12 } };

            Assert.Throws<IndexOutOfRangeException>(() => operations.AddMatrix(matrix1, matrix2));
        }

        [Fact]
        public void MultByNumberTest()
        {
            var matrix = new double[,] { { 1, 2, 3 }, { 4, 5, 6 } };
            double number = 3.0;

            var expectedMatrix = new double[,] { { 3, 6, 9 }, { 12, 15, 18 } };
            var actualMatrix = operations.MultByNumber(matrix, number);

            MatrixCompare(expectedMatrix, actualMatrix);
        }

        [Fact]
        public void GatParamsArimaTest()
        {
            var arima = new ArimaModel();
            var mle = new MLEMethod(2, 2, arima, 0);

            var data = new List<double> { 0.3, 0.2, 1.3, 0.6, 0.5, 1.6, 1.2 };

            var result = mle.GetParams(data);

            Assert.NotNull(result);
            Assert.True(result.P >= 1 && result.P <= 2);
            Assert.True(result.Q >= 1 && result.Q <= 2);
            Assert.NotNull(result.Fi);
            Assert.NotNull(result.Tao);
            Assert.False(double.IsNaN(result.Value));
            Assert.False(double.IsInfinity(result.Value));
        }

        [Fact]
        public void ForecastArimaTest()
        {
            var arima = new ArimaModel();
            var mle = new MLEMethod(1, 0, arima, 0);

            var data = new List<double> { 1, 2, 3 };
            var original = new List<double>(data);
            var fi = new List<double> { 1.0 }; 
            var tao = new List<double>();   
            int period = 2;
            var result = mle.Forecast(1, 0, 0, period, fi, tao, data, original);

            Assert.Equal(2, result.Count);
            Assert.Equal(3, result[0], 6);
            Assert.Equal(3, result[1], 6);
        }

        [Fact]
        public void ForecastArimaNotNullTest()
        {
            var arima = new ArimaModel();
            var mle = new MLEMethod(1, 1, arima, 0);

            var data = new List<double> { 1, 2, 3, 4 };
            var original = new List<double>(data);
            var fi = new List<double> { 0.5 };
            var tao = new List<double> { 0.3 };

            var result = mle.Forecast(1, 0, 1, 3, fi, tao, data, original);

            Assert.All(result, x => Assert.False(double.IsNaN(x)));
        }

        [Fact]
        public void ForecatArimaWithDTest()
        {
            var arima = new ArimaModel();
            var mle = new MLEMethod(1, 0, arima, 1);

            var data = new List<double> { 1, 1, 1 };
            var original = new List<double> { 10, 11, 12 };
            var fi = new List<double> { 0 };
            var tao = new List<double>();

            var result = mle.Forecast(1, 1, 0, 2, fi, tao, data, original);

            Assert.Equal(12, result[0], 6);
            Assert.Equal(12, result[1], 6);
        }

        [Fact]
        public void CalculateEstimateSarimaTest()
        {
            var sarima = new SarimaModel(4);
            var mle = new MLEMethodForSarima(sarima, 1, 1, 1, 1, 0, 0, 4);

            var data = new List<double> { 1, 2, 3, 4, 5, 6, 7, 8 };
            var fi = new List<double> { 0.5 };
            var Fi = new List<double> { 0.2 };
            var tao = new List<double> { 0.1 };
            var Tao = new List<double> { 0.1 };

            var errors = mle.CalculateEstimates(1, 1, 1, 1, fi, Fi, tao, Tao, data, 0);

            Assert.Equal(data.Count, errors.Count);
        }

        public static IEnumerable<object[]> SarimaData =>
           new List<object[]>
           {
                new object[] { 1, 0, 0, 0, 0, 0, 1, new List<double> { 1, 2, 3 }, new List<double> { 1.0 }, new List<double>(), 
                               new List<double>(), new List<double>(), 1, 0, 0, 0, 2, 0 },
                new object[] { 1, 1, 0, 0, 0, 1, 4, new List<double> { 10, 12, 14, 16, 11, 13, 15, 17 }, new List<double> { 0.5 },
                               new List<double> { 0.5 }, new List<double>(), new List<double>(), 1, 1, 0, 0, 2, 0 }
           };

        [Theory]
        [MemberData(nameof(SarimaData))]
        public void ForecastSarimaTest(int p, int P, int q, int Q, int d, int D, int s, List<double> data, 
                                               List<double> fi, List<double> Fi, List<double> tao, List<double> Tao,
                                               int p_, int P_, int q_, int Q_, int period, int c)
        {
            var sarima = new SarimaModel(4);
            var mle = new MLEMethodForSarima(sarima, p, P, q, Q, d, d, s);
            var original = new List<double>(data);
            var result = mle.Forecast(p_, P_, q_, Q_, period, fi, Fi, tao, Tao, original, data, data, c);

            Assert.Equal(period, result.Count);
            Assert.All(result, x => Assert.False(double.IsNaN(x)));
        }

        [Fact]
        public void GetParamsSarimaTest()
        {
            var sarima = new SarimaModel(4);
            var mle = new MLEMethodForSarima(sarima, 2, 2, 2, 2, 0, 0, 4);
            var data = new List<double>{ 10, 12, 14, 16, 11, 13, 15, 17 };
            var result = mle.GetParams(data);

            Assert.NotNull(result);
            Assert.True(result.P >= 1);
            Assert.True(result.Q >= 1);
            Assert.NotNull(result.Fi);
            Assert.NotNull(result.Tao);
        }

        [Fact]
        public void BuildSarimaTest()
        {
            var sarima = new SarimaModel(2);
            var data = new List<double> { 1, 2, 3, 4, 5 };
            var errors = new List<double> { 0.1, 0.2, 0.3, 0.4, 0.5 };
            var fi = new List<double> { 0.5 };
            var Fi = new List<double> { 0.2 };
            var tao = new List<double> { 0.4 };
            var Tao = new List<double> { 0.3 };

            var result = sarima.BuildSarimaModel(4, 1, 1, 1, 1, errors, fi, Fi, tao, Tao, data);

            Assert.Equal(2.674, result, 6);
        }

        [Fact]
        public void ForecastVarTest()
        {
            var model = new VarModel();
            var items = new VarModelItems
            {
                IsGPD = true,
                GPDData = new List<double> { 1, 2, 3, 4, 5, 6 },
                OriginalGPDData = new List<double> { 1, 2, 3, 4, 5, 6 },
                GPD_D = 0,
                GPDMethod = "GDP"
            };

            var result = model.RunVarModel(items, maxP: 1, period: 3);

            Assert.NotNull(result);
            Assert.True(result.Count > 0);
            Assert.All(result, r => Assert.NotEmpty(r.Item1));
        }
    }
}
