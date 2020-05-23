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

        //Wczytywanie danych z pliku
        public void LoadData()
        {
            int totalTicks = 26000;

            var options = new ProgressBarOptions
            {
                ProgressCharacter = '─',
                ProgressBarOnBottom = true
            };

            using (var pbar = new ProgressBar(totalTicks, "Creating data...", options))
            {
                var imagesLearnList = new List<double[]>();

                var directories = Directory.GetDirectories(@"../../learning");
                foreach (var d in directories)
                {
                    var letter = Path.GetFileName(d);
                    var letterNorm = new double[26];
                    int n = Encoding.ASCII.GetBytes(letter)[0] - 65;
                    letterNorm[n] = 1;

                    var files = Directory.GetFiles(d);
                    foreach (var f in files)
                    {
                        imagesLearnList.Add(ConvertImage(f, letterNorm));
                        pbar.Tick();
                    }
                }

                var imagesTestList = new List<double[]>();

                directories = Directory.GetDirectories(@"../../test");
                foreach (var d in directories)
                {
                    var letter = Path.GetFileName(d);
                    var letterNorm = new double[26];
                    int n = Encoding.ASCII.GetBytes(letter)[0] - 65;
                    letterNorm[n] = 1;

                    var files = Directory.GetFiles(d);
                    foreach (var f in files)
                    {
                        imagesTestList.Add(ConvertImage(f, letterNorm));
                        pbar.Tick();
                    }
                }

                var imagesLearn = imagesLearnList.ToArray();
                var imagesTest = imagesTestList.ToArray();

                Shuffle(imagesLearn);
                Shuffle(imagesTest);
                Split(imagesLearn, imagesTest);
            }
        }

        public double[] ConvertImage(string file, double [] letterNorm)
        {
            var image = new Bitmap(file);
            var imageNorm = new double[1024];

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

            var originalLength = imageNorm.Length;
            Array.Resize<double>(ref imageNorm, originalLength + letterNorm.Length);
            Array.Copy(letterNorm, 0, imageNorm, originalLength, letterNorm.Length);
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
                for (int j = 0; j < 1024; j++)
                {
                    TrainingValues[i][j] = learn[i][j];
                }

                TrainingTargets[i] = new double[26];
                int k = 0;
                for (int j = 1024; j < 1050; j++)
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
                for (int j = 0; j < 1024; j++)
                {
                    TestValues[i][j] = learn[i][j];
                }

                TestTargets[i] = new double[26];
                int k = 0;
                for (int j = 1024; j < 1050; j++)
                {
                    TestTargets[i][k] = learn[i][j];
                    k++;
                }
            }
        }

        //Przetasowanie danych
        public void Shuffle(double [][] data)
        {
            Random rnd = new Random();
            int n = data.Length;
            for (int i = 0; i < (n - 1); i++)
            {
                int r = i + rnd.Next(n - i);
                double[] t = data[r];
                data[r] = data[i];
                data[i] = t;
            }
        }
        #endregion
    }
}

