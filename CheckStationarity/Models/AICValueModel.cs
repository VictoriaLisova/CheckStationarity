using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckStationarity.Models
{
    public class AICValueModel
    {
        public int P { get; set; }
        public int Q { get; set; }
        public double Value { get; set; }
        public List<double>? Fi {  get; set; }
        public List<double>? Tao { get; set; }
    }
}
