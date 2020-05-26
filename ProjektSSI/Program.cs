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
            //data.LoadData();

            Network network = new Network(784, new int []{ 100, 100, 100, 100, 100, 100}, 26, 0.01, 0.99);
            network.LoadWeights();
            //network.Train(data, 0.001);
            
            char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            var result = network.Compute(data.ConvertImage(@"../../../letter.png"));
            for (int i = 0; i < result.Length; i++)
            {
                Console.WriteLine(alpha[i] + " " + result[i].ToString("0.000"));
            }
            Console.WriteLine(Convert.ToChar(65 + Array.IndexOf(result,result.Max())));
            
            Console.ReadKey();
        }
    }
}
