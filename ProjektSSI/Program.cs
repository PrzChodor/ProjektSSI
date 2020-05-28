
using System;
using System.Linq;
using System.IO;

namespace ProjektSSI
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Choose an option:");
                Console.WriteLine("1) Train Neural Network");
                Console.WriteLine("2) Select picture to recognize text");
                Console.WriteLine("3) Exit");
                Console.Write("\r\nSelect an option: ");

                switch (Console.ReadLine())
                {
                    case "1":
                        TrainNN();
                        break;
                    case "2":
                        RecognizeText();
                        break;
                    case "3":
                        return;
                    default:
                        break;
                }
            }
        }

        static void TrainNN()
        {
            Console.Clear();
            Data data = new Data();
            //Wczytanie bazy danych do uczenia sieci neuronowej
            data.LoadData();

            Network network = new Network(784, new int[] { 100, 100, 100, 100, 100, 100 }, 26, 0.01, 0.99);
            //Wczytanie wag
            network.LoadWeights();

            //Uczenie sieci neuronowej do końcowego błedu poniżej 0.001
            network.Train(data, 0.001);
        }

        static void RecognizeText()
        {
            Console.Clear();
            Network network = new Network(784, new int[] { 100, 100, 100, 100, 100, 100 }, 26, 0.01, 0.99);
            //Wczytanie wag
            network.LoadWeights();

            Console.WriteLine("Enter path to the picture:");
            string output = "";
            var path = Console.ReadLine();

            if (File.Exists(path))
            {
                //Podział obrazu na mniejsze części z literami i przesłanie ich do sieci neuronowej
                foreach (var image in SplitImage.Split(path))
                {
                    char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
                    var result = network.Compute(Data.ConvertImage(image));
                    output += Convert.ToChar(65 + Array.IndexOf(result, result.Max()));
                }
                Console.Clear();
                Console.WriteLine("Text on picture " + path + ":\n" + output);
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
            else
            {
                Console.Clear();
                Console.WriteLine("File doesn't exist!");
            }
        }
    }
}
