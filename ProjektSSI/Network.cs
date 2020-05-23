using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using ShellProgressBar;

namespace ProjektSSI
{

    public class Network
    {
        #region Properties
        public double LearningRate { get; set; }

        public double Momentum { get; set; }

        public List<Neuron> InputLayer { get; set; }

        public List<List<Neuron>> HiddenLayers { get; set; }

        public List<Neuron> OutputLayer { get; set; }
        #endregion

        private static readonly Random random = new Random();

        #region Methods
        //Tworzenie nowej sieci
        public Network(int inputSize, int[] hiddenSizes, int outputSize, double learnRate , double momentum)
        {
            LearningRate = learnRate;
            Momentum = momentum;
            InputLayer = new List<Neuron>();
            HiddenLayers = new List<List<Neuron>>();
            OutputLayer = new List<Neuron>();

            for (var i = 0; i < inputSize; i++)
                InputLayer.Add(new Neuron(new Sigmoid()));

            var firstHiddenLayer = new List<Neuron>();
            for (var i = 0; i < hiddenSizes[0]; i++)
                firstHiddenLayer.Add(new Neuron(InputLayer, new Sigmoid()));

            HiddenLayers.Add(firstHiddenLayer);

            for (var i = 1; i < hiddenSizes.Length; i++)
            {
                var hiddenLayer = new List<Neuron>();
                for (var j = 0; j < hiddenSizes[i]; j++)
                    hiddenLayer.Add(new Neuron(HiddenLayers[i - 1], new Sigmoid()));
                HiddenLayers.Add(hiddenLayer);
            }

            for (var i = 0; i < outputSize; i++)
                OutputLayer.Add(new Neuron(HiddenLayers.Last(), new Sigmoid()));
        }

        //Uczenie sieci z zadaną ilością epok lub dopóki błąd się nie zwiększa
        public void Train(Data dataSet, int numEpochs)
        {
            var lastWeights = CurrentWeights();
            var lowestError = Test(dataSet);

            Console.WriteLine();
            Console.WriteLine($"   Accuracy = {lowestError[0] * 100:F4}%");
            Console.WriteLine($"   RMSE = {lowestError[1]}");

            for (var i = 0; i < numEpochs; i++)
            {
                int totalTicks = dataSet.TrainingValues.Length;

                var options = new ProgressBarOptions
                {
                    ProgressCharacter = '─',
                    ProgressBarOnBottom = true
                };
                using (var pbar = new ProgressBar(totalTicks, "Training: Step 0 of " + totalTicks, options))
                {
                    for (int j = 0; j < dataSet.TrainingValues.Length; j++)
                    {
                        ForwardPropagate(dataSet.TrainingValues[j]);
                        BackPropagate(dataSet.TrainingTargets[j]);
                        pbar.Tick("Training: Step " + j + " of " + totalTicks);
                    }
                }

                var currentError = Test(dataSet);

                //Jeśli obency błąd jest większy niż poprzedni to koniec treningu
                if (lowestError[1] < currentError[1])
                {
                    SaveWeights(lastWeights);
                    Console.WriteLine();
                    Console.WriteLine($"   End of epoch {i + 1}");
                    Console.WriteLine($"   Accuracy = {lowestError[0] * 100:F4}%");
                    Console.WriteLine($"   RMSE = {lowestError[1]}");
                    return;
                }

                lastWeights = CurrentWeights();
                lowestError = currentError;

                Console.WriteLine();
                Console.WriteLine($"   End of epoch {i + 1}");
                Console.WriteLine($"   Accuracy = {lowestError[0] * 100:F4}%");
                Console.WriteLine($"   RMSE = {lowestError[1]}");
            }

            SaveWeights(lastWeights);
            Console.WriteLine();
            Console.WriteLine($"   End of epoch {numEpochs}");
            Console.WriteLine($"   Accuracy = {lowestError[0] * 100:F4}%");
            Console.WriteLine($"   RMSE = {lowestError[1]}");
        }

        //Uczenie sieci do uzyskania odpowiednio małego błedu lub dopóki błąd się nie zwiększa
        public void Train(Data dataSet, double maximumError)
        {
            var lastWeights = CurrentWeights();
            var lowestError = Test(dataSet);
            int epoch = 1;

            Console.WriteLine();
            Console.WriteLine($"   Accuracy = {lowestError[0] * 100:F4}%");
            Console.WriteLine($"   RMSE = {lowestError[1]}");

            while (maximumError < lowestError[1])
            {
                int totalTicks = dataSet.TrainingValues.Length;

                var options = new ProgressBarOptions
                {
                    ProgressCharacter = '─',
                    ProgressBarOnBottom = true
                };
                using (var pbar = new ProgressBar(totalTicks, "Training: Step 0 of " + totalTicks, options))
                {
                    for (int j = 0; j < dataSet.TrainingValues.Length; j++)
                    {
                        ForwardPropagate(dataSet.TrainingValues[j]);
                        BackPropagate(dataSet.TrainingTargets[j]);
                        pbar.Tick("Training: Step " + j + " of " + totalTicks);
                    }
                }

                var currentError = Test(dataSet);

                //Jeśli obency błąd jest większy niż poprzedni to koniec treningu
                if (lowestError[1] < currentError[1])
                {
                    SaveWeights(lastWeights);
                    Console.WriteLine();
                    Console.WriteLine($"   End of epoch {epoch}");
                    Console.WriteLine($"   Accuracy = {lowestError[0] * 100:F4}%");
                    Console.WriteLine($"   RMSE = {lowestError[1]}");
                    return;
                }

                lastWeights = CurrentWeights();
                lowestError = currentError;

                SaveWeights(lastWeights);
                Console.WriteLine();
                Console.WriteLine($"   End of epoch {epoch}");
                Console.WriteLine($"   Accuracy = {lowestError[0] * 100:F4}%");
                Console.WriteLine($"   RMSE = {lowestError[1]}");

                epoch++;
            }
            SaveWeights(lastWeights);
            Console.WriteLine();
            Console.WriteLine($"   End of epoch {epoch}");
            Console.WriteLine($"   Accuracy = {lowestError[0] * 100:F4}%");
            Console.WriteLine($"   RMSE = {lowestError[1]}");
        }

        //Test sieci dla zestawu testowego danych
        public double[] Test(Data dataSet)
        {
            double goodValues = 0;
            double error = 0;

            int totalTicks = dataSet.TestValues.Length;

            var options = new ProgressBarOptions
            {
                ProgressCharacter = '─',
                ProgressBarOnBottom = true
            };
            using (var pbar = new ProgressBar(totalTicks, "Testing: Step 0 of " + totalTicks, options))
            {
                for (int i = 0; i < dataSet.TestValues.Length; i++)
                {
                    var result = Compute(dataSet.TestValues[i]).ToList();
                    var expected = dataSet.TestTargets[i].ToList();
                    for (int j = 0; j < dataSet.TestTargets[i].Length; j++)
                        error += Math.Pow(result[j] - dataSet.TestTargets[i][j], 2);
                    if (result.IndexOf(result.Max()) == expected.IndexOf(expected.Max()))
                        goodValues++;
                    pbar.Tick("Testing: Step " + i + " of " + totalTicks);
                }
            }

            return new double[] { goodValues / dataSet.TestValues.Length, Math.Sqrt(error / dataSet.TestValues.Length) };
        }

        //Obliczenie wartości wyjściowej sieci neuronowej dla pojedyńczego elementu
        private void ForwardPropagate(double[] inputs)
        {
            var i = 0;
            InputLayer.ForEach(a => a.Value = inputs[i++]);
            HiddenLayers.ForEach(a => a.ForEach(b => b.CalculateValue()));
            OutputLayer.ForEach(a => a.CalculateValue());
        }

        //Zaktualizowanie wag
        private void BackPropagate(double[] targets)
        {
            var i = 0;
            OutputLayer.ForEach(a => a.CalculateGradient(targets[i++]));
            HiddenLayers.Reverse();
            HiddenLayers.ForEach(a => a.ForEach(b => b.CalculateGradient()));
            HiddenLayers.ForEach(a => a.ForEach(b => b.UpdateWeights(LearningRate, Momentum)));
            HiddenLayers.Reverse();
            OutputLayer.ForEach(a => a.UpdateWeights(LearningRate, Momentum));
        }

        //Obliczenie i przesłanie wartości wyjściowej sieci neuronowej dla pojedyńczego elementu
        public double[] Compute(params double[] inputs)
        {
            ForwardPropagate(inputs);
            return OutputLayer.Select(a => a.Value).ToArray();
        }

        //Zapisywanie obecnych wag do listy
        public List<double> CurrentWeights()
        {
            var weights = new List<double>();

            foreach (var layer in HiddenLayers)
            {
                foreach (var neuron in layer)
                {
                    weights.Add(neuron.Bias);
                    foreach (var synapse in neuron.InputSynapses)
                    {
                        weights.Add(synapse.Weight);
                    }
                }
            }

            foreach (var neuron in OutputLayer)
            {
                weights.Add(neuron.Bias);
                foreach (var synapse in neuron.InputSynapses)
                {
                    weights.Add(synapse.Weight);
                }
            }

            return weights;
        }

        //Zapisywanie wag
        public void SaveWeights(List<double> weights)
        {
            using (StreamWriter file = new StreamWriter(GetPath(), false))
            {
                Console.WriteLine("sss");
                int totalTicks = weights.Count;

                var options = new ProgressBarOptions
                {
                    ProgressCharacter = '─',
                    ProgressBarOnBottom = true
                };
                using (var pbar = new ProgressBar(totalTicks, "Saving: Step 0 of " + totalTicks, options))
                {
                    int i = 0;
                    foreach (var item in weights)
                    {
                        file.WriteLine(item);
                        pbar.Tick("Saving: Step 0 of " + i);
                        i++;
                    }
                }
            }
        }

        //Wczytywanie wag
        public void LoadWeights()
        {
            try
            {
                using (StreamReader file = new StreamReader(GetPath()))
                {

                    foreach (var layer in HiddenLayers)
                    {
                        foreach (var neuron in layer)
                        {
                            neuron.Bias = Convert.ToDouble(file.ReadLine());
                            foreach (var synapse in neuron.InputSynapses)
                            {
                                synapse.Weight = Convert.ToDouble(file.ReadLine());
                            }
                        }
                    }

                    foreach (var neuron in OutputLayer)
                    {
                        neuron.Bias = Convert.ToDouble(file.ReadLine());
                        foreach (var synapse in neuron.InputSynapses)
                        {
                            synapse.Weight = Convert.ToDouble(file.ReadLine());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
        }

        //Ustalenie ścieżki dla poszczególnych sieci jako "../../data/weights(ilość neuronów poszczególnych warstw).data"
        private string GetPath()
        {
            string path = @"../../data/weights(";

            path += InputLayer.Count.ToString() + "-";
            foreach (var layer in HiddenLayers)
            {
                path += layer.Count.ToString() + "-";
            }
            path += OutputLayer.Count.ToString() + ").data";

            return path;
        }

        //Losowa wartość pomiędzy -1 a 1 
        public static double GetRandom()
        {
            return 2 * random.NextDouble() - 1;
        }
        #endregion
    }
}