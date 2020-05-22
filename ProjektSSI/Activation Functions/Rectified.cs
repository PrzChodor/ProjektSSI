using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjektSSI
{
    public class Rectified : IActivationFunction
    {
        public double Output(double x)
        {
            return x <= 0 ? 0 : x;
        }

        public double Derivative(double x)
        {
            return x <= 0 ? 0 : 1;
        }
    }
}
