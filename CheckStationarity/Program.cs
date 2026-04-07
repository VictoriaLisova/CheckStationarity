using CheckStationarity;
using CheckStationarity.Check;
using CheckStationarity.Data_Inversion;
using CheckStationarity.Data_Preparation;
using CheckStationarity.Differentiate;
using CheckStationarity.Differentiate.Differentiate;
using CheckStationarity.Models;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

public static class Program
{
    public static void Main(string[] args)
    {
        string path = "D:/Диплом/Файли/data.csv";
        var readFile = new ReadCsvFile();

        var data = readFile.ReadFile(path).Take(100).ToList();

        var dataPrep = new DataPreperation(data);
        var gpd = dataPrep.GetData(x => x.GDP);
        var inflation = dataPrep.GetData(x => x.Inflation);
        var unemployments = dataPrep.GetData(x => x.Unemployment);
        var currency = dataPrep.GetData(x => x.Currency);

        var method = new SelectMethod();

        // ARIMA
        var runArima = new RunArimaModel();
        runArima.ExecuteArima(currency, "yeo", method);


        // SARIMA
        //var runSarima = new RunSarimaModel();
        //runSarima.RunSarima(currency, "yeo", method);

        // VAR
        //var runVar = new RunVarModel();
        //runVar.RunVar(gpd, inflation, unemployments, currency, method);

        // GARCH
        var runGarch = new RunGarch();
        runGarch.ExecuteGarch(runArima);
    }
}
