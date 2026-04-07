using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckStationarity.Models
{
    public class AICGarchModel
    {
        public int Q {  get; set; }
        public int P {  get; set; }
        public double W {  get; set; }
        public List<double> Alpha {  get; set; }
        public List<double> Beta {  get; set; }
        public double Value {  get; set; }
    }
}
