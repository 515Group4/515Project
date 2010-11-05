using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

namespace ResizePictures
{
    class Program
    {
        static void Main(string[] args)
        {
            const string FolderName = @"C:\Data\Datasets\t2";
            string[] files = Directory.GetFiles(FolderName);
            foreach (var filename in files)
            {
                if (filename.EndsWith("Thumbs.db"))
                {
                    continue;
                }
                Console.WriteLine(filename);
                Bitmap b = (Bitmap)Bitmap.FromFile(filename);
                if (b.Width > 1000 || b.Height > 1000)
                {
                    double fx = 600 / (double)b.Width;
                    double fy = 600 / (double)b.Height;
                    double f = Math.Min(fx, fy);

                    int reducedWidth = (int)(f * b.Width);
                    int reducedHeight = (int)(f * b.Height);
                    Bitmap b2 = new Bitmap(reducedWidth, reducedHeight);
                    using (Graphics g = Graphics.FromImage(b2))
                    {
                        g.DrawImage(b, new Rectangle(0, 0, reducedWidth, reducedHeight));
                    }

                    b.Dispose();
                    b2.Save(filename, System.Drawing.Imaging.ImageFormat.Tiff);
                }
            }
        }
    }
}
