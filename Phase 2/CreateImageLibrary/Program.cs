using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace CreateImageLibrary
{
    class Program
    {
        static void Main(string[] args)
        {
            const int numShapesPerRow = 8;
            const int numRows = 8;
            const int ShapeWidth = 32;
            const int ShapeHeight = 32;
            
            Bitmap bmp = new Bitmap(256, 256);
            Graphics g = Graphics.FromImage(bmp);
            Brush b = Brushes.Black;

            // row 1 - ovals
            for (int i = 0; i < numShapesPerRow; i++)
            {
                int ht = (int)((1-i/(double)numShapesPerRow)*ShapeHeight+i/(double)numShapesPerRow*1);
                g.FillEllipse(b, new Rectangle(i*ShapeWidth, (ShapeHeight-ht)/2, ShapeWidth, ht));
            }

            bmp.Save("patterns1.png");
        }
    }
}
