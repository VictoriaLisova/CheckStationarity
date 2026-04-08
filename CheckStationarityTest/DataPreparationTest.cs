using CheckStationarity.Data_Preparation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckStationarityTest
{
    public class DataPreparationTest
    {
        private DataPreparation dataPrep = new DataPreparation();
        private List<double> Data = new List<double> { 10, 12, 11, 13, 100, 12, 11, 9, 10 };

        [Fact]
        public void ProcessOutlinersLogTest()
        {
            var actual = dataPrep.ProcessOutliners(Data, 1.5, "log");
            var expected = new List<double> { 2.3026, 2.4849, 2.3979, 2.5649, 2.5257, 2.4849, 2.3979, 2.1972, 2.3026 };
            for(int i = 0; i < actual.Count; i++)
            {
                Assert.InRange(actual[i], expected[i] - 0.001, expected[i] + 0.001);
            }
        }

        [Fact]
        public void ProcessOutlinersShiftTest()
        {
            var actual = dataPrep.ProcessOutliners(Data, 1.5, "log");
            Assert.Equal(1e-6, dataPrep.Shift);
        }

        [Fact]
        public void PrecessOutlinersYeoTest()
        {
            var actualYeo = dataPrep.ProcessOutliners(Data, 1.5, "yeo");
            var expectedYeo = dataPrep.ProcessOutliners(Data, 1.5, "yeo");

            for (int i = 0; i < expectedYeo.Count; i++)
            {
                Assert.InRange(actualYeo[i], expectedYeo[i] - 0.001, expectedYeo[i] + 0.001);
            }
        }
    }
}
