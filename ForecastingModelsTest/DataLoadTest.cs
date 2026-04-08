using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Xunit;

namespace ForecastingModelsTest
{
    [TestClass]
    public class DataLoadTest
    {
        [Fact]
        public void LoadDataFromCSV()
        {
            string path = "test.csv";
            File.WriteAllLines(path, new[]{
                "1", "2", "3", "4"
            });
            var csvReader = new ReadCsvFile();
            var list = csvReader.ReadFile(path);
            list.Should().HaveCount(4);
            list.Should().Contain(1, 2, 3, 4);
            File.Delete(path);
        }
    }
}
