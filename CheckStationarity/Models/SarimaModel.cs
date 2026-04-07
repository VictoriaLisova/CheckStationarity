using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckStationarity.Models
{
    public class SarimaModel : IModelType
    {
        private int s;
        public SarimaModel(int s)
        {
            this.s = s;
        }
        private double AR(List<double> fi, int p, int t, List<double> y)
        {
            double ar = 0;
            for(int i = 0; i < fi.Count; i++)
            {
                if(t - i - 1 >= 0 && t - i - 1 < y.Count)
                {
                    ar += fi[i] * y[t - i - 1];
                }
            }
            return ar;
        }
        private double SAR(List<double> Fi, int P, int t, List<double> y)
        {
            double sar = 0;
            for(int I = 0; I < Fi.Count; I++)
            {
                if(t - (I + 1) * s >= 0 && t - (I + 1) * s < y.Count)
                {
                    sar += Fi[I] * y[t - (I + 1) * s];
                }
            }
            return sar;
        }

        private double MA(List<double> errors, List<double> tao, int q, int t)
        {
            double ma = 0;
            for(int j = 0; j < tao.Count; j++)
            {
                if(t - j - 1 >= 0 && t - j - 1 < errors.Count)
                {
                    ma += tao[j] * errors[t - j - 1];
                }
            }
            return ma;
        }

        private double SMA(List<double> errors, List<double> Tao, int Q, int t)
        {
            double sma = 0;
            for(int J = 0; J < Tao.Count; J++)
            {
                if(t - (J + 1) *s >= 0 && t - (J + 1) * s < errors.Count)
                {
                    sma += Tao[J] * errors[t - (J + 1) * s];
                }
            }
            return sma;
        }

        private double MultipleARandSAR(int p, int P, List<double> fi, List<double> Fi, int t, List<double> y)
        {
            double multRes = 0;
            for (int i = 0; i < fi.Count; i++)
            {
                for (int I = 0; I < Fi.Count; I++)
                {
                    if (i < fi.Count && I < Fi.Count && t - i - 1 - (I + 1) * s >= 0 && t - i - 1 - (I + 1) * s < y.Count)
                    {
                        multRes += fi[i] * Fi[I] * y[t - i - 1 - (I + 1) * s];
                    }
                }
            }
            return multRes;
        }

        private double MultipleMaandSMA(int q, int Q, List<double> errors, List<double> tao, List<double> Tao, int t)
        {
            double multRs = 0;
            for (int j = 0; j < tao.Count; j++)
            {
                for (int J = 0; J < Tao.Count; J++)
                {
                    if (j < tao.Count && J < Tao.Count && t - j - 1 - (J + 1) * s >= 0
                        && t - j - 1 - (J + 1) * s < errors.Count)
                    {
                        multRs += tao[j] * Tao[J] * errors[t - j - 1 - (J + 1) * s];
                    }
                }
            }
            return multRs;
        }

        public double BuildSarimaModel(int t, int p, int P, int q, int Q, 
            List<double> errors, List<double> fi, List<double> Fi, List<double> tao, List<double> Tao, List<double> data)
        {
            return AR(fi, p, t, data) + SAR(Fi, P, t, data) - MultipleARandSAR(p, P, fi, Fi, t, data)
                + MA(errors, tao, q, t) + SMA(errors, Tao, Q, t) + MultipleMaandSMA(q, Q, errors, tao, Tao, t);
        }
        public double BuildModel(List<double> data)
        {
            return 0;
        }
    }
}
