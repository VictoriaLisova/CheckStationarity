using CheckStationarity.Check;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckStationarityTest
{
    public class CheckTest
    {
        private static List<double> Errors = new List<double> { 0.001, 0.3, 0.1, 0.001 };
        private List<double> Actual = new List<double> { 3.2, 3.5, 4.1, 3.9, 4.2, 4.5, 4.3 };
        private List<double> Forecast = new List<double> { 3.1, 3.6, 3.7, 4.0, 4.1, 4.6, 4.4 };
        private CheckErrors cheker = new CheckErrors(Errors);
        private CheckModelResults modelRes = new CheckModelResults();

        [Fact]
        public void CheckErrorsAverageTest()
        {
            Assert.Equal(0.1005, cheker.GetErrorAverage());
        }

        [Fact]
        public void CheckSigmaTest()
        {
            Assert.Equal(0.12, Math.Round(cheker.GetSigmaSquare(), 2));
        }

        [Fact]
        public void LjungBoxValueTest()
        {
            Assert.True(cheker.LjungBoxTest(4) == false);
        }

        [Fact]
        public void ErrorsAcfTest()
        {
            var lags = new List<double> { -0.334, -0.332, 0.1661, 0 };
            var res = cheker.ErrorsACF(4);
            Assert.Equal(4, res.Count);
            for (int i = 0; i < res.Count; i++)
            {
                Assert.InRange(res[i], lags[i] - 0.001, lags[i] + 0.001);
            }
        }

        [Fact]
        public void MSETest()
        {
            double actualMSE = modelRes.GetMSE(Actual, Forecast);
            Assert.Equal(0.031, Math.Round(actualMSE, 3));
        }

        [Fact]
        public void MAETest()
        {
            double actualMAE = modelRes.GetMAE(Actual, Forecast);
            Assert.Equal(0.143, Math.Round(actualMAE, 3));
        }

        [Fact]
        public void MAPETest()
        {
            double actualMAPE = modelRes.GetMAPE(Actual, Forecast);
            Assert.Equal(3.604, Math.Round(actualMAPE, 3));
        }

        [Fact]
        public void RMSETest()
        {
            double actualRMSE = modelRes.GetRMSE(Actual, Forecast);
            Assert.Equal(0.177, Math.Round(actualRMSE, 3));
        }
    }
}
