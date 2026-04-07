using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckStationarity.Models
{
    public class AICcValueSarima : AICValueModel
    {
        public int QSeasonal {  get; set; }
        public int PSeasonal {  get; set; }
        public double C {  get; set; }
        public List<double>? FiSeasonal {  get; set; }
        public List<double>? TaoSeasonal { get; set; }  
    }
}
