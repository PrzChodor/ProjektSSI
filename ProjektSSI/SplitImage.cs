
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;


namespace ProjektSSI
{
    static class SplitImage
    {
        //Podział obrazu na mniejsze części z literami
        public static List<Bitmap> Split(string path)
        {
            Bitmap image;

            //Dodanie czarnej obwódki do obrazu w celu zmniejszenia ilości warunków podczas wyszukiwania liter
            using (var temp = new Bitmap(path))
            {
                image = AddBorder(temp, 1);
            }

            BitmapData bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadWrite, image.PixelFormat);
            int bytesPerPixel = System.Drawing.Bitmap.GetPixelFormatSize(image.PixelFormat) / 8;
            int stride = bitmapData.Stride;
            int byteCount = stride * image.Height;
            byte[] pixels = new byte[byteCount];
            IntPtr ptrFirstPixel = bitmapData.Scan0;
            Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);
            int heightInPixels = bitmapData.Height;
            int widthInBytes = bitmapData.Width * bytesPerPixel;
            int[,] labels = new int[bitmapData.Height, bitmapData.Width];
            int currentLabel = 1;
            var dictionary = new Dictionary<int, int>();

            //Znalezienie liter i wszystkich pixeli do nich należących
            for (int y = 1; y < heightInPixels - 1; y++)
            {
                int currentLine = y * stride;
                for (int x = bytesPerPixel; x < widthInBytes - 1; x = x + bytesPerPixel)
                {
                    if (pixels[currentLine + x] != 0 && labels[y, x / bytesPerPixel] == 0)
                    {
                        findNeighbors(x, y, bytesPerPixel, currentLabel, stride, labels, pixels);
                        currentLabel++;
                    }
                }
            }

            image.UnlockBits(bitmapData);

            //Zaznaczenie liter kolorami i zapisanie jako nowy obraz
            Bitmap bitmap = new Bitmap(labels.GetLength(1), labels.GetLength(0));
            Random rnd = new Random();
            List<Color> colors = new List<Color>();
            colors.Add(Color.Black);
            for (int i = 0; i < currentLabel; i++)
            {
                colors.Add(Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256)));
            }

            for (int i = 0; i < labels.GetLength(0); i++)
            {
                for (int j = 0; j < labels.GetLength(1); j++)
                {
                    bitmap.SetPixel(j, i, colors[labels[i, j]]);
                }
            }
            bitmap.Save("characters.png");

            //Wyznaczenie punktów końcowych poszczególnych liter
            var minX = new Dictionary<int, int>();
            var minY = new Dictionary<int, int>();
            var maxX = new Dictionary<int, int>();
            var maxY = new Dictionary<int, int>();

            for (int i = 0; i < labels.GetLength(0); i++)
            {
                for (int j = 0; j < labels.GetLength(1); j++)
                {
                    if (labels[i, j] != 0)
                    {
                        if (!minX.ContainsKey(labels[i, j]))
                        {
                            minX[labels[i, j]] = j;
                            minY[labels[i, j]] = i;
                            maxX[labels[i, j]] = j;
                            maxY[labels[i, j]] = i;
                        }
                        else
                        {
                            if (minX[labels[i, j]] > j)
                                minX[labels[i, j]] = j;

                            if (minY[labels[i, j]] > i)
                                minY[labels[i, j]] = i;

                            if (maxX[labels[i, j]] < j)
                                maxX[labels[i, j]] = j;

                            if (maxY[labels[i, j]] < i)
                                maxY[labels[i, j]] = i;
                        }
                    }
                }
            }

            var imageWithMinX = new Dictionary<Bitmap, int>();

            //Dla każdej litery wycięcie i przeskalowanie obrazu z nią 
            foreach (var i in minX.Keys)
            {
                int length = Math.Max(maxX[i] - minX[i], maxY[i] - minY[i]) + 1;

                //Wycięcie obrazu z literą
                Bitmap cropped = new Bitmap(length, length);
                using (Graphics graphics = Graphics.FromImage(cropped))
                {
                    graphics.Clear(Color.Black);
                    var srcRect = new Rectangle(minX[i], minY[i], maxX[i] - minX[i] + 1, maxY[i] - minY[i] + 1);
                    graphics.DrawImage(image, (length - (maxX[i] - minX[i] + 1)) / 2.0f, (length - (maxY[i] - minY[i] + 1)) / 2.0f, srcRect, GraphicsUnit.Pixel);
                }

                //Przeskalowanie obrazu do 28x28
                Bitmap resized = new Bitmap(28, 28);
                using (Graphics graphics = Graphics.FromImage(resized))
                {
                    graphics.InterpolationMode = InterpolationMode.High;
                    graphics.Clear(Color.Black);
                    graphics.DrawImage(cropped, 1, 1, 26, 26);
                    imageWithMinX.Add(resized,minX[i]);
                    //Zapisanie obrazów z pojedyńczymi literami
                    resized.Save(i + ".png");
                }
            }
            //Ustwaienie odpowedniej kolejności liter
            var imageOrder = imageWithMinX.OrderBy(x => x.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
            var images = imageOrder.Keys.ToList();
            return images;
        }

        //Dodanie obwódki do obrazu
        static Bitmap AddBorder(Bitmap image, int thickness)
        {
            Bitmap newImage = new Bitmap(image.Width + 2 * thickness, image.Height + 2 * thickness);
            using (Graphics graphics = Graphics.FromImage(newImage))
            {
                graphics.Clear(Color.Black);
                int x = (newImage.Width - image.Width) / 2;
                int y = (newImage.Height - image.Height) / 2;
                graphics.DrawImage(image, x, y);
            }
            return newImage;
        }

        //Znajduje wszystkie nieczarne pixele połączone z podanym i przypisuje im tą samą wartość
        static void findNeighbors(int x, int y, int bytesPerPixel,int currentLabel,int stride, int[,] labels, byte[] pixels)
        {
            Stack<int[]> stack = new Stack<int[]>();
            stack.Push(new int[] { x, y });

            int x1 = x / bytesPerPixel;
            labels[y, x1] = currentLabel;

            while (stack.Count > 0)
            {
                if (pixels[(y - 1) * stride + x] != 0 && labels[y - 1, x1] == 0)
                {
                    stack.Push(new int[] { x, y - 1 });
                    labels[y - 1, x1] = currentLabel;
                }

                if (pixels[(y + 1) * stride + x] != 0 && labels[y + 1, x1] == 0)
                {
                    stack.Push(new int[] { x, y + 1 });
                    labels[y - 1, x1] = currentLabel;
                }

                if (pixels[y * stride + x - bytesPerPixel] != 0 && labels[y, x1 - 1] == 0)
                {
                    stack.Push(new int[] { x - bytesPerPixel, y });
                    labels[y, x1 - 1] = currentLabel;
                }

                if (pixels[y * stride + x + bytesPerPixel] != 0 && labels[y, x1 + 1] == 0)
                {
                    stack.Push(new int[] { x + bytesPerPixel, y });
                    labels[y, x1 + 1] = currentLabel;
                }

                int[] point = stack.Pop();
                x = point[0];
                y = point[1];
                x1 = x / bytesPerPixel;
            }
        }
    }
}
