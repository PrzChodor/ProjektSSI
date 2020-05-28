
using System;
using System.IO;
using System.Drawing;
using System.Text;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using ShellProgressBar;

namespace ProjektSSI
{
    public class Data
    {
        #region Properties
        public double[][] TestValues { get; set; }
        public double[][] TestTargets { get; set; }
        public double[][] TrainingValues { get; set; }
        public double[][] TrainingTargets { get; set; }
        #endregion

        #region Methods

        //Wczytywanie danych z plików
        public void LoadData()
        {
            int totalTicks = Directory.GetFiles(@"../../../data/train", "*.png", SearchOption.AllDirectories).Length;
            totalTicks += Directory.GetFiles(@"../../../data/test", "*.png", SearchOption.AllDirectories).Length;

            var options = new ProgressBarOptions
            {
                ProgressCharacter = '─',
                ProgressBarOnBottom = true
            };

            using (var pbar = new ProgressBar(totalTicks, "Creating data...", options))
            {
                var imagesLearnList = new List<double[]>();

                //Wczytanie wszystkich obrazów do treningu
                var directories = Directory.GetDirectories(@"../../../data/train");
                foreach (var d in directories)
                {
                    var letter = Path.GetFileName(d);
                    var letterNorm = new double[26];
                    int n = Encoding.ASCII.GetBytes(letter)[0] - 65;
                    letterNorm[n] = 1;

                    var files = Directory.GetFiles(d);
                    foreach (var f in files)
                    {
                        var imageNorm = ConvertImage(f);
                        var originalLength = imageNorm.Length;
                        Array.Resize<double>(ref imageNorm, originalLength + letterNorm.Length);
                        Array.Copy(letterNorm, 0, imageNorm, originalLength, letterNorm.Length);
                        imagesLearnList.Add(imageNorm);
                        pbar.Tick();
                    }
                }

                var imagesTestList = new List<double[]>();

                //Wczytanie wszystkich obrazów do testowania
                directories = Directory.GetDirectories(@"../../../data/test");
                foreach (var d in directories)
                {
                    var letter = Path.GetFileName(d);
                    var letterNorm = new double[26];
                    int n = Encoding.ASCII.GetBytes(letter)[0] - 65;
                    letterNorm[n] = 1;

                    var files = Directory.GetFiles(d);
                    foreach (var f in files)
                    {
                        var imageNorm = ConvertImage(f);
                        var originalLength = imageNorm.Length;
                        Array.Resize<double>(ref imageNorm, originalLength + letterNorm.Length);
                        Array.Copy(letterNorm, 0, imageNorm, originalLength, letterNorm.Length);
                        imagesTestList.Add(imageNorm);
                        pbar.Tick();
                    }
                }

                var imagesLearn = imagesLearnList.ToArray();
                var imagesTest = imagesTestList.ToArray();

                Split(imagesLearn, imagesTest);
                Shuffle();
            }
        }

        //Wczytanie obrazu z pliku i zamiana go na tablicę jednowymiarową wartości 0-1
        public double[] ConvertImage(string path)
        {
            Bitmap image; 
            using (var temp = new Bitmap(path))
            {
                image = new Bitmap(temp);
            }
            var imageNorm = new double[784];

            BitmapData bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadWrite, image.PixelFormat);
            int byteCount = bitmapData.Stride * image.Height;
            byte[] pixels = new byte[byteCount];
            IntPtr ptrFirstPixel = bitmapData.Scan0;
            Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);

            int j = 0;

            for (int i = 0; i < pixels.Length; i += 4)
            {
                imageNorm[j] = (double)pixels[i] / 255;
                j++;
            }

            image.UnlockBits(bitmapData);

            return imageNorm;
        }

        //Zamiana obrazu z bitmapy na tablicę jednowymiarową wartości 0-1
        public double[] ConvertImage(Bitmap image)
        {
            var imageNorm = new double[784];

            BitmapData bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadWrite, image.PixelFormat);
            int byteCount = bitmapData.Stride * image.Height;
            byte[] pixels = new byte[byteCount];
            IntPtr ptrFirstPixel = bitmapData.Scan0;
            Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);

            int j = 0;

            for (int i = 0; i < pixels.Length; i += 4)
            {
                imageNorm[j] = (double)pixels[i] / 255;
                j++;
            }

            image.UnlockBits(bitmapData);

            return imageNorm;
        }

        //Podzielenie danych na zestawy
        public void Split(double[][] learn, double[][] test)
        {
            int length = learn.Length;

            TrainingValues = new double[length][];
            TrainingTargets = new double[length][];

            for (int i = 0; i < length; i++)
            {
                TrainingValues[i] = new double[1024];
                for (int j = 0; j < 784; j++)
                {
                    TrainingValues[i][j] = learn[i][j];
                }

                TrainingTargets[i] = new double[26];
                int k = 0;
                for (int j = 784; j < 810; j++)
                {
                    TrainingTargets[i][k] = learn[i][j];
                    k++;
                }
            }

            length = test.Length;

            TestValues = new double[length][];
            TestTargets = new double[length][];

            for (int i = 0; i < length; i++)
            {
                TestValues[i] = new double[1024];
                for (int j = 0; j < 784; j++)
                {
                    TestValues[i][j] = learn[i][j];
                }

                TestTargets[i] = new double[26];
                int k = 0;
                for (int j = 784; j < 810; j++)
                {
                    TestTargets[i][k] = learn[i][j];
                    k++;
                }
            }
        }

        //Przetasowanie danych
        public void Shuffle()
        {
            Random rnd = new Random();
            int n = this.TrainingValues.Length;
            for (int i = 0; i < (n - 1); i++)
            {
                int r = i + rnd.Next(n - i);
                double[] t0 = this.TrainingValues[r];
                double[] t1 = this.TrainingTargets[r];
                this.TrainingValues[r] = this.TrainingValues[i];
                this.TrainingTargets[r] = this.TrainingTargets[i];
                this.TrainingValues[i] = t0;
                this.TrainingTargets[i] = t1;
            }

            n = this.TestTargets.Length;
            for (int i = 0; i < (n - 1); i++)
            {
                int r = i + rnd.Next(n - i);
                double[] t0 = this.TestValues[r];
                double[] t1 = this.TestTargets[r];
                this.TestValues[r] = this.TestValues[i];
                this.TestTargets[r] = this.TestTargets[i];
                this.TestValues[i] = t0;
                this.TestTargets[i] = t1;
            }
        }
        #endregion
    }
}

