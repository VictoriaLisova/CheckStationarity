using CheckStationarity;
using Xunit;

namespace CheckStationarityTest
{
    public class DataLoadTest
    {
        private string Path = "test.csv";
        private List<CsvFileModel> GetFileData()
        {
            File.WriteAllLines(Path, new[]{
                "Квартал,Рік,Номер кварталу,ВВП,Інфляція,Безробіття,USD/UAH",
                "2000-Q1,2000,1,5.4,28.2,11.9,5.45",
                "2000-Q2,2000,2,6.2,25.1,11.6,5.6"
            });
            var csvReader = new ReadCsvFile();
            var list = csvReader.ReadFile(Path);
            return list;
        }

        [Fact]
        public void LoadDataCsvTest()
        {
            var list = GetFileData();
            Assert.NotNull(list);
            Assert.Equal(2, list.Count);
            Assert.Equal(5.4, list[0].GDP);
            Assert.Equal(2, list[1].QuartalNumber);

            File.Delete(Path);
        }

        [Fact]
        public void GetDataByTypeTest()
        {
            var list = GetFileData();
            var prep = new DataPreperation(list);

            Assert.NotNull(prep.GetData(x => x.GDP));
            Assert.Equal(new List<double> { 5.4, 6.2 }, prep.GetData(x => x.GDP));

            Assert.NotNull(prep.GetData(x => x.Inflation));
            Assert.Equal(new List<double> { 28.2, 25.1 }, prep.GetData(x => x.Inflation));

            Assert.NotNull(prep.GetData(x => x.Unemployment));
            Assert.Equal(new List<double> { 11.9, 11.6 }, prep.GetData(x => x.Unemployment));

            Assert.NotNull(prep.GetData(x => x.Currency));
            Assert.Equal(new List<double> { 5.45, 5.6 }, prep.GetData(x => x.Currency));

            File.Delete(Path);
        }
    }
}
