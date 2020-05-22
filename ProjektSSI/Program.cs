using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShellProgressBar;

namespace ProjektSSI
{
    class Program
    {
        static void Main(string[] args)
        {
            Data data = new Data();
            data.LoadData();

            Network network = new Network(1024, new int []{ 512, 256, 128, 64 }, 26, 0.05, 0.99);
            network.Train(data, 0.1);
            Console.ReadKey();
        }
    }
}
