using CheckStationarity.Differentiate;
using CheckStationarity.Differentiate.Differentiate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckStationarityTest
{
    public class DifferentiateTest
    {
        private KPSSMethod kpss = new KPSSMethod();
        private ADFMethod adf = new ADFMethod();    
        public static IEnumerable<object[]> DataADF =>
            new List<object[]>
            {
                new object[] { new List<double> { 1, 3, 0.2, 12, 4, 10, 0.3, 0.4 }, false},
                new object[] { new List<double> { 0.1, 0.5, 0.2, 0.6, 0.2, 0.8, 0.5 }, true}
            };

        [Theory]
        [MemberData(nameof(DataADF))]
        public void ADFTest(List<double> data, bool res)
        {
            Assert.True(res == adf.IsStationary(data));
        }

        public static IEnumerable<object[]> DataKPSS =>
            new List<object[]>
            {
                new object[] { new List<double> { 1, 3, 0.2, 12, 4, 10, 0.3, 0.4 }, true},
                new object[] { new List<double> { 0.1, 0.5, 0.2, 0.6, 0.2, 0.8, 0.5 }, true}
            };

        [Theory]
        [MemberData(nameof(DataKPSS))]
        public void KPSSTest(List<double> data, bool res)
        {
            Assert.True(res == kpss.IsStationary(data));
        }

        public static IEnumerable<object[]> DataDiff =>
            new List<object[]>
            {
                new object[] { new List<double> { 1, 2, 3, 4, 8, 6, 7, 8, 11 }, 1, new List<double> { 1, 1, 1, 4, -2, 1, 1, 3}, 1 },
                new object[] { new List<double> { 10, 11, 13, 12, 15, 17, 18, 20, 21, 23 }, 1, new List<double> { 1, 2, -1, 3, 2, 1, 2, 1, 2}, 1 }
            };

        [Theory]
        [MemberData(nameof(DataDiff))]  
        public void DifferentiateTimeLineTest(List<double> data, int s, List<double> expected, int d)
        {
            var diff = new Differentiate(kpss, adf);
            var actual = diff.GetDiffeentiateTimeLine(data, s);
            Assert.Equal(d, diff.d);
            for (int i = 0; i < actual.Count; i++)
            {
                Assert.Equal(expected[i], actual[i]);
            }
        }
    }
}
