using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckStationarity.Differentiate.Differentiate
{
    public class Differentiate
    {
        private StationarityMethod kpss;
        private StationarityMethod adf;
        public int d { get; set; }
        public Differentiate(StationarityMethod kpss, StationarityMethod adf)
        {
            this.kpss = kpss;
            this.adf = adf;
            d = 0;
        }
        private List<double> UpdateList(List<double> data, int s)
        {
            var newList = new List<double>();
            for (int i = s; i < data.Count; i++)
            {
                newList.Add(data[i] - data[i - s]);
            }
            return newList;
        }

        public List<double> GetDiffeentiateTimeLine(List<double> data, int s)
        {
            while (true)
            {
                if (kpss.IsStationary(data) && adf.IsStationary(data)) break;
                else data = UpdateList(data, s);

                d++;
            }
            return data;
        }
    }
}
