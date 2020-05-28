
namespace ProjektSSI
{
    public class Synapse
    {
        #region Properties

        public Neuron InputNeuron { get; set; }

        public Neuron OutputNeuron { get; set; }

        public double Weight { get; set; }

        public double WeightDelta { get; set; }
        #endregion

        //Tworzenie nowej synapsy
        public Synapse(Neuron inputNeuron, Neuron outputNeuron)
        {
            InputNeuron = inputNeuron;
            OutputNeuron = outputNeuron;
            Weight = Network.GetRandom();
        }
    }
}
