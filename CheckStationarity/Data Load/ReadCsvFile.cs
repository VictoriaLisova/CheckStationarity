using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckStationarity
{
    public class ReadCsvFile : IReadFile
    {
        private List<CsvFileModel> ParseFileContent(List<string> lines)
        {
            var data = new List<CsvFileModel>();
            foreach(var line in lines)
            {
                var columns = line.Split(",");
                data.Add(new CsvFileModel
                {
                    QuartalName = columns[0],
                    Year = int.Parse(columns[1]),
                    QuartalNumber = int.Parse(columns[2]),
                    GDP = double.Parse(columns[3], CultureInfo.InvariantCulture),
                    Inflation = double.Parse(columns[4], CultureInfo.InvariantCulture),
                    Unemployment = double.Parse(columns[5], CultureInfo.InvariantCulture),
                    Currency = double.Parse(columns[6], CultureInfo.InvariantCulture)
                });
            }
            return data;
        }
        public List<CsvFileModel> ReadFile(string filePath)
        {
            var content = File.ReadLines(filePath).Skip(1);
            return ParseFileContent(content.ToList());
        }
    }
}
