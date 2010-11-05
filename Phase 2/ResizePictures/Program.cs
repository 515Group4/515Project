using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

// to run this program -- 
// Step 1. set the FolderName
// Step 2. set the doRename variable to 1. Run the program.
// Step 3. set the doRename variable to 2. Run the program.
// Step 4. set the doRename variable to 3. Run the program.

namespace ResizePictures
{
    class Program
    {
        static void Main(string[] args)
        {
            const string FolderName = @"C:\Data\Datasets\t2";
            string[] files = Directory.GetFiles(FolderName);
            
            int doRename = 3;

            // rename the files to numbers
            if (doRename == 2)
            {
                int i = 0;
                foreach (var filename in files)
                {
                    if (filename.EndsWith("Thumbs.db"))
                    {
                        continue;
                    }
                    i++;
                    File.Move(filename, Path.Combine(Path.GetDirectoryName(filename), "foo__" + i + ".tif"));
                }
            }
            else if (doRename == 3)
            {
                int i = 0;
                foreach (var filename in files)
                {
                    if (filename.EndsWith("Thumbs.db"))
                    {
                        continue;
                    }
                    i++;
                    File.Move(filename, Path.Combine(Path.GetDirectoryName(filename), i + ".tif"));
                }
            }
            else // resize the files
            {
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
}
