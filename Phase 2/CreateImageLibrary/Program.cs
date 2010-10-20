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

            int cury = 32;
            //row 2 - hexagons
            for (int i = 0; i < numShapesPerRow; i++)
            {
                int ht = (int)((1-i/(double)numShapesPerRow)*ShapeHeight+i/(double)numShapesPerRow*1);
                int basey = cury + (ShapeHeight)/2;
                Point pt1 = new Point(i * ShapeWidth, basey);
                Point pt2 = new Point(i * ShapeWidth + ShapeWidth / 4, basey - ht / 2);
                Point pt3 = new Point(i * ShapeWidth + 3 * ShapeWidth / 4, basey - ht / 2);
                Point pt4 = new Point((i + 1) * ShapeWidth, basey);
                Point pt5 = new Point(i * ShapeWidth + 3 * ShapeWidth / 4, basey + ht / 2);
                Point pt6 = new Point(i * ShapeWidth + ShapeWidth / 4, basey + ht / 2);
                g.FillPolygon(b, new Point[] { pt1, pt2, pt3, pt4, pt5, pt6 });
            }

            cury = 64;
            // row 3 - pentagons
            for (int i = 0; i < numShapesPerRow; i++)
            {
                int ht = (int)((1 - i / (double)numShapesPerRow) * ShapeHeight + i / (double)numShapesPerRow * 1);
                int basey = cury + (ShapeHeight) / 2;
                Point[] pts = new Point[5];
                pts[0] = new Point(i * ShapeWidth, basey - 4 * ht / 32);
                pts[1] = new Point(i * ShapeWidth + ShapeWidth / 2, basey - 16 * ht / 32);
                pts[2] = new Point((i + 1) * ShapeWidth, basey - 4 * ht / 32);
                pts[3] = new Point((i + 1) * ShapeWidth - 6 * ShapeWidth / 32, basey + ht / 2);
                pts[4] = new Point(i * ShapeWidth + 6 * ShapeWidth / 32, basey + ht / 2);
                g.FillPolygon(b, pts);
            }

            // rows 4 - rectangles
            cury += 32;
            for (int i = 0; i < numShapesPerRow; i++)
            {
                int ht = (int)((1 - i / (double)numShapesPerRow) * ShapeHeight + i / (double)numShapesPerRow * 1);
                int basey = cury + (ShapeHeight) / 2;
                g.FillRectangle(b, i * ShapeWidth, basey - ht / 2, ShapeWidth, ht);
            }

            // row 5 - triangles
            cury += 32;
            for (int i = 0; i < numShapesPerRow; i++)
            {
                int ht = (int)((1 - i / (double)numShapesPerRow) * ShapeHeight + i / (double)numShapesPerRow * 1);
                int basey = cury + (ShapeHeight) / 2;
                Point[] pts = new Point[3];
                pts[0] = new Point(i * ShapeWidth, basey + ht/2);
                pts[1] = new Point(i * ShapeWidth + ShapeWidth / 2, basey - ht / 2);
                pts[2] = new Point((i + 1) * ShapeWidth, basey + ht / 2);
                g.FillPolygon(b, pts);
            }

            // row 6 - stars
            cury += 32;
            for (int i = 0; i < numShapesPerRow; i++)
            {
                int ht = (int)((1 - i / (double)numShapesPerRow) * ShapeHeight + i / (double)numShapesPerRow * 1);
                int basey = cury + (ShapeHeight) / 2;
                Point[] pts = new Point[10];
                pts[0] = new Point(i * ShapeWidth + ShapeWidth/2, basey - ht / 2);
                pts[1] = new Point(i * ShapeWidth + 19 * ShapeWidth / 32, basey - 4 * ht / 32);
                pts[2] = new Point((i+1) * ShapeWidth , basey - 4 * ht / 32);
                pts[3] = new Point(i * ShapeWidth + 22 * ShapeWidth / 32, basey + 3 * ht / 32);
                pts[4] = new Point(i * ShapeWidth + 26 * ShapeWidth / 32, basey + ht / 2);
                pts[5] = new Point(i * ShapeWidth + 16 * ShapeWidth / 32, basey + 8 * ht / 32);
                pts[6] = new Point(i * ShapeWidth + 6 * ShapeWidth / 32, basey + 16 * ht / 32);
                pts[7] = new Point(i * ShapeWidth + 9 * ShapeWidth / 32, basey + 3 * ht / 32);
                pts[8] = new Point(i * ShapeWidth + 0 * ShapeWidth / 32, basey - 4 * ht / 32);
                pts[9] = new Point(i * ShapeWidth + 12 * ShapeWidth / 32, basey - 4 * ht / 32);
                g.FillPolygon(b, pts);
            }
            

            //Pen p = Pens.Red;
            //for (int i = 0; i < numShapesPerRow; i++)
            //{
            //    g.DrawLine(p, i * ShapeWidth, 0, i * ShapeWidth, 256);
            //    g.DrawLine(p, 0, i*ShapeHeight, 256, i * ShapeHeight);
            //}

            bmp.Save("patterns1.png");
        }
    }
}
