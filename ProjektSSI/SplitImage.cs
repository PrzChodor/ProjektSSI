using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;


namespace ProjektSSI
{
    class SplitImage
    {
        public static List<Bitmap> Split(string path)
        {
            Bitmap image;

            using (var temp = new Bitmap(path))
            {
                image = AddBorder(temp, 1);
            }

            BitmapData bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadWrite, image.PixelFormat);
            int bytesPerPixel = System.Drawing.Bitmap.GetPixelFormatSize(image.PixelFormat) / 8;
            int byteCount = bitmapData.Stride * image.Height;
            byte[] pixels = new byte[byteCount];
            IntPtr ptrFirstPixel = bitmapData.Scan0;
            Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);
            int heightInPixels = bitmapData.Height;
            int widthInBytes = bitmapData.Width * bytesPerPixel;
            int[,] label = new int[bitmapData.Height, bitmapData.Width];
            int currentLabel = 1;
            var dictionary = new Dictionary<int, int>();

            for (int y = 1; y < heightInPixels - 1; y++)
            {
                int currentLine = y * bitmapData.Stride;
                for (int x = bytesPerPixel; x < widthInBytes - 1; x = x + bytesPerPixel)
                {
                    if (pixels[currentLine + x] != 0 && label[y, x / bytesPerPixel] == 0)
                    {
                        findNeighbors(pixels, label, x, y, bytesPerPixel, bitmapData.Stride, currentLabel);
                        currentLabel++;
                    }
                }
            }

            image.UnlockBits(bitmapData);

            Bitmap bitmap = new Bitmap(label.GetLength(1),label.GetLength(0));

            for (int i = 0; i < label.GetLength(0); i++)
            {
                for (int j = 0; j < label.GetLength(1); j++)
                {
                    int color = (int)(255 * (label[i, j] / (double)(currentLabel-1)));
                    bitmap.SetPixel(j, i, Color.FromArgb(color, color, color));
                }
            }
            bitmap.Save("color.png");

            var minX = new Dictionary<int, int>();
            var minY = new Dictionary<int, int>();
            var maxX = new Dictionary<int, int>();
            var maxY = new Dictionary<int, int>();

            for (int i = 0; i < label.GetLength(0); i++)
            {
                for (int j = 0; j < label.GetLength(1); j++)
                {
                    if (label[i, j] != 0)
                    {
                        if (!minX.ContainsKey(label[i, j]))
                        {
                            minX[label[i, j]] = j;
                            minY[label[i, j]] = i;
                            maxX[label[i, j]] = j;
                            maxY[label[i, j]] = i;
                        }
                        else
                        {
                            if (minX[label[i, j]] > j)
                                minX[label[i, j]] = j;

                            if (minY[label[i, j]] > i)
                                minY[label[i, j]] = i;

                            if (maxX[label[i, j]] < j)
                                maxX[label[i, j]] = j;

                            if (maxY[label[i, j]] < i)
                                maxY[label[i, j]] = i;
                        }
                    }
                }
            }

            var imageWithMinX = new Dictionary<Bitmap, int>();

            foreach (var i in minX.Keys)
            {
                int length = Math.Max(maxX[i] - minX[i], maxY[i] - minY[i]) + 1;

                Bitmap cropped = new Bitmap(length, length);
                using (Graphics graphics = Graphics.FromImage(cropped))
                {
                    graphics.Clear(Color.Black);
                    var srcRect = new Rectangle(minX[i], minY[i], maxX[i] - minX[i] + 1, maxY[i] - minY[i] + 1);
                    graphics.DrawImage(image, (length - (maxX[i] - minX[i] + 1)) / 2.0f, (length - (maxY[i] - minY[i] + 1)) / 2.0f, srcRect, GraphicsUnit.Pixel);
                }

                Bitmap resized = new Bitmap(28, 28);
                using (Graphics graphics = Graphics.FromImage(resized))
                {
                    graphics.InterpolationMode = InterpolationMode.High;
                    graphics.Clear(Color.Black);
                    graphics.DrawImage(cropped, 1, 1, 26, 26);
                    resized.Save(i + ".png");
                    imageWithMinX.Add(resized,minX[i]);
                }
            }
            var imageOrder = imageWithMinX.OrderBy(x => x.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
            var images = imageOrder.Keys.ToList();

            return images;
        }

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

        static void findNeighbors(byte[] pixels, int[,] labels, int x, int y, int bytesPerPixel, int stride, int label)
        {
            int x1 = x / bytesPerPixel;
            labels[y, x1] = label;

            if (pixels[(y - 1) * stride + x] != 0 && labels[y - 1, x1] == 0)
                findNeighbors(pixels, labels, x, y - 1, bytesPerPixel, stride, label);
            if (pixels[(y + 1) * stride + x] != 0 && labels[y + 1, x1] == 0)
                findNeighbors(pixels, labels, x, y + 1, bytesPerPixel, stride, label);
            if (pixels[y * stride + x - bytesPerPixel] != 0 && labels[y, x1 - 1] == 0)
                findNeighbors(pixels, labels, x - bytesPerPixel, y, bytesPerPixel, stride, label);
            if (pixels[y * stride + x + bytesPerPixel] != 0 && labels[y, x1 + 1] == 0)
                findNeighbors(pixels, labels, x + bytesPerPixel, y, bytesPerPixel, stride, label);
        }
    }
}
