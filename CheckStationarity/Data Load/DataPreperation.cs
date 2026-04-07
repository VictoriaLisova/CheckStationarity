using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckStationarity
{
    public class DataPreperation
    {
        private List<CsvFileModel> data;
        public DataPreperation(List<CsvFileModel> data)
        {
            this.data = data;
        }
        public List<double> GetData(Func<CsvFileModel, double> selector)
        {
            return data.Select(selector).ToList();
        }
    }
}
