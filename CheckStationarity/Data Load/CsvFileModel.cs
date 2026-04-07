using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckStationarity
{
    public class CsvFileModel
    {
        public string? QuartalName {  get; set; }
        public int Year {  get; set; }
        public int QuartalNumber {  get; set; }
        public double GDP {  get; set; }    
        public double Inflation {  get; set; }
        public double Unemployment {  get; set; }
        public double Currency {  get; set; }
    }
}
