using System;
using System.Linq;
using System.IO;

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
            Console.WriteLine("Podaj ścieżkę do obrazka z tekstem:");
            while (true)
            {
                string output = "";
                var path = Console.ReadLine();
                SplitImage split = new SplitImage();

                if (File.Exists(path))
                {
                    foreach (var image in split.Split(path))
                    {
                        char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
                        var result = network.Compute(data.ConvertImage(image));
                        output += Convert.ToChar(65 + Array.IndexOf(result, result.Max()));
                    }
                    Console.Clear();
                    Console.WriteLine("Tekst na obrazku " + path + " to:\n" + output);
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("Nieprawidłowa ścieżka!");
                }

                Console.WriteLine();
                Console.WriteLine("Podaj ścieżkę do kolejnego obrazka z tekstem:");
            }
        }
    }
}
