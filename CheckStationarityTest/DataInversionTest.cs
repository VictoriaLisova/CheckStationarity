using CheckStationarity.Data_Inversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckStationarityTest
{
    public class DataInversionTest
    {
        private InvertData invert=  new InvertData();
        public static IEnumerable<object[]> LogData =>
            new List<object[]>
            {
                new object[] { new List<double> { 0, 1, 0.2, 0.3 }, 
                    new List<double> { Math.Exp(0), Math.Exp(1), Math.Exp(0.2), Math.Exp(0.3) } },
                new object[] { new List<double> { 1, 3.2, 8.3, 1.4, 7.3 }, 
                    new List<double> { Math.Exp(1), Math.Exp(3.2), Math.Exp(8.3), Math.Exp(1.4), Math.Exp(7.3) } }
            };

        [Theory]
        [MemberData(nameof(LogData))]
        public void InvertLogTest(List<double> input, List<double> expected)
        {
            var actualRes = invert.Invert(input, "log");
            for(int i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i], actualRes[i]);
            }
        }

        public static IEnumerable<object[]> LogDataShift =>
            new List<object[]>
            {
                new object[] { new List<double> { 3.4, 12.5, 1.5 },
                    new List<double> { Math.Exp(3.4) - 1.3, Math.Exp(12.5) - 1.3, Math.Exp(1.5) - 1.3 }, 1.3 },
                new object[] { new List<double> { 10.4, 9.3, 10.5, 4.8, 11.5 },
                    new List<double> { Math.Exp(10.4) - 4.1, Math.Exp(9.3) - 4.1, Math.Exp(10.5) - 4.1, Math.Exp(4.8) - 4.1, Math.Exp(11.5) - 4.1 }, 4.1 }
            };

        [Theory]
        [MemberData(nameof(LogDataShift))]
        public void InvertLogWithShiftTest(List<double> input, List<double> expected, double shift)
        {
            var actualLogShift = invert.Invert(input, "log", shift: shift);
            for(int i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i], actualLogShift[i]);
            }
        }

        public static IEnumerable<object[]> YeoData =>
            new List<object[]>
            {
                new object[] { new List<double> { 1.2, 3.1, 3.3, 4.1, 2.7 },
                    new List<double> { Math.Pow((-1 * 1.2 + 1), (1.0 / - 1.0)) - 1, Math.Pow((-1 * 3.1 + 1), (1.0 / -1.0)) - 1,
                                       Math.Pow((-1 * 3.3 + 1), (1.0 / -1.0)) - 1, Math.Pow((-1 * 4.1 + 1), (1.0 / -1.0)) - 1,
                                       Math.Pow((-1 * 2.7 + 1), (1.0 / -1.0)) - 1 }, -1 },

                new object[] { new List<double> { 21.5, 6.2, -8.1 },
                    new List<double> { Math.Pow((-0.5 * 21.5 + 1), (1.0 / -0.5)) - 1, Math.Pow((-0.5 * 6.2 + 1), (1.0 / -0.5)) - 1,
                                       1 - Math.Pow((-(2 + 0.5) * (-8.1) + 1), (1.0 / (2 + 0.5)))}, -0.5 },

                new object[] { new List<double> { -1.6, 7.3, 9.2, -12, 7 },
                    new List<double> { 1 - Math.Pow((-(2 + 0) * (-1.6) + 1), (1.0 / (2 + 0))), Math.Exp(7.3) - 1, Math.Exp(9.2) - 1,
                                       1 - Math.Pow((-(2 + 0) * (-12) + 1), (1.0 / (2 + 0))), Math.Exp(7) - 1 }, 0 },

                new object[] { new List<double> { 9.3, 5.5 },
                    new List<double> { Math.Pow((0.5 * 9.3 + 1), (1.0 / 0.5)) - 1, Math.Pow((0.5 * 5.5 + 1), (1.0 / 0.5)) - 1 }, 0.5 },

                new object[] { new List<double> { 0.3, -0.2, -4.2, 9.2, 3.1, -0.3, 7.3 },
                    new List<double> { Math.Pow((1 * 0.3 + 1), (1.0 / 1.0)) - 1, 1 - Math.Pow((-(2 - 1) * (-0.2) + 1), (1.0 / (2 - 1))),
                        1 - Math.Pow((-(2 - 1) * (-4.2) + 1), (1.0 / (2 - 1))), Math.Pow((1 * 9.2 + 1), (1.0 / 1.0)) - 1, 
                        Math.Pow((1 * 3.1 + 1), (1.0 / 1.0)) - 1, 1 - Math.Pow((-(2 - 1) * (-0.3) + 1), (1.0 / (2 - 1))),
                        Math.Pow((1 * 7.3 + 1), (1.0 / 1.0)) - 1 }, 1 }
            };

        [Theory]
        [MemberData(nameof(YeoData))]
        public void InvertYeoJohnsonTest(List<double> input, List<double> expected, double l)
        {
            var actualYeoJohnson = invert.Invert(input, "yeo", l: l);
            for(int i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i], actualYeoJohnson[i]);                    
            }
        }
    }
}
