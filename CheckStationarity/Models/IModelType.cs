using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckStationarity.Models
{
    public interface IModelType
    {
        double BuildModel(List<double> data);
    }
}
