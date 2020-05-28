using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjektSSI
{
    public class Neuron
    {
        #region Properties

        public List<Synapse> InputSynapses { get; set; }

        public List<Synapse> OutputSynapses { get; set; }

        public IActivationFunction ActivationFunction { get; set; }

        public double Bias { get; set; }

        public double BiasDelta { get; set; }

        public double Gradient { get; set; }

        public double Value { get; set; }
        #endregion

        #region Methods
        //Tworzenie neuronu bez podłączenia neuronów wejściowych
        public Neuron(IActivationFunction activationFunction)
        {
            InputSynapses = new List<Synapse>();
            OutputSynapses = new List<Synapse>();
            Bias = Network.GetRandom();
            ActivationFunction = activationFunction;
        }

        //Tworzenie neuronu wraz z podłączeniem neuronów wejściowych
        public Neuron(List<Neuron> inputNeurons, IActivationFunction activationFunction) : this(activationFunction)
        {
            foreach (var inputNeuron in inputNeurons)
            {
                var synapse = new Synapse(inputNeuron, this);
                inputNeuron.OutputSynapses.Add(synapse);
                InputSynapses.Add(synapse);
            }
        }

        //Obliczanie wartości na wyjściu neuronu
        public virtual double CalculateValue()
        {
            return Value = ActivationFunction.Output(InputSynapses.Sum(a => a.Weight * a.InputNeuron.Value) + Bias);
        }

        //Obliczanie błedu na wyjściu neuronu
        public double CalculateError(double target)
        {
            return target - Value;
        }

        //Obliczanie gradientu
        public double CalculateGradient(double? target = null)
        {
            //Dla neuronów warstw ukrytych
            if (target == null)
                return Gradient = OutputSynapses.Sum(a => a.OutputNeuron.Gradient * a.Weight) * ActivationFunction.Derivative(Value);

            //Dla neuronów warstwy wyjściowej
            return Gradient = CalculateError(target.Value) * ActivationFunction.Derivative(Value);
        }

        //Aktualizacja wag i biasu
        public void UpdateWeights(double learnRate, double momentum)
        {
            var prevDelta = BiasDelta;
            BiasDelta = learnRate * Gradient;
            Bias += BiasDelta + momentum * prevDelta;

            //Aktualizacja wag w synapsach wejściowych
            foreach (var synapse in InputSynapses)
            {
                prevDelta = synapse.WeightDelta;
                synapse.WeightDelta = learnRate * Gradient * synapse.InputNeuron.Value;
                synapse.Weight += synapse.WeightDelta + momentum * prevDelta;
            }
        }
        #endregion
    }
}
