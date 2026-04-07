using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckStationarity.Models
{
    public class AICValueVarModel
    {
        public int P {  get; set; }
        public double[,] B { get; set; }
        public double Value {  get; set; }
    }
}
