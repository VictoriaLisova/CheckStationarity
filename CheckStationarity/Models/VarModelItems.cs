using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckStationarity.Models
{
    public class VarModelItems
    {
        public bool IsGPD {  get; set; }
        public bool IsInflation {  get; set; }
        public bool IsUnemployment {  get; set; }
        public bool IsCurrency {  get; set; }
        public int GPD_D {  get; set; }
        public int Inflation_D { get; set; }
        public int Unemployment_D {  get; set; }
        public int Currency_D { get; set; }
        public List<double> GPDData {  get; set; }
        public List<double> InflationData {  get; set; }
        public List<double> UnemploymentData { get; set; }
        public List<double> CurrencyData { get; set; }
        public List<double> OriginalGPDData { get; set; }
        public List<double> OriginalInflationData { get; set; }
        public List<double> OriginalUnemploymentData { get; set; }
        public List<double> OriginalCurrencyData { get; set; }
        public string GPDMethod {  get; set; }
        public string InflationMethod { get; set; }
        public string UnemploymentMethod { get; set;}
        public string CurrencyMethod { get; set; }
    }
}
