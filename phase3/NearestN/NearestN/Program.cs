using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

using NearestNeighbor;

namespace NearestN
{
    class NearestN 
    {

        static void Main(string[] args)
        {
            // would love to do a
            // Program1 pg1 = new Program1(); here

            // All this is just stub garbage, ignore...
            int[] f1_names = { 1, 5, 7, 2, 9, 11, 3, 4, 8, 6, 10 };
            double[] f1_scores = { 0.9, 0.8, 0.7, 0.65, 0.6, 0.4, 0.2, 0.15, 0.1, 0.05, 0.05 };
            int[] f2_names = { 7, 1, 3, 11, 5, 9, 2, 10, 6, 8, 4 };
            double[] f2_scores = f1_scores;
            Queue<int> f1n = new Queue<int>();
            Queue<double> f1s = new Queue<double>();
            Queue<int> f2n = new Queue<int>();
            Queue<double> f2s = new Queue<double>();

            for (int j = 0; j < 11; j++)
            {
                f1n.Enqueue(f1_names[j]);
                f1s.Enqueue(f1_scores[j]);
                f2n.Enqueue(f2_names[j]);
                f2s.Enqueue(f2_scores[j]);
            }
            // end stub garbage

            NRAMerge nra = new NRAMerge();

            int i = 0;
            bool keepGoing = true;
            while (keepGoing)
            {
                if (i % 2 == 0)
                {
                    nra.addData(f1n.Dequeue(), f1s.Dequeue());
                }
                else
                {
                    nra.addData(f2n.Dequeue(), f2s.Dequeue());
                }

                i++;
                if (i > 15)
                {
                    keepGoing = false;
                }
            }

            nra.printAll();

            Console.WriteLine("Dominant: ");
            List<int> foo = nra.getDominant();
            foreach(int f in foo)
            {
                Console.WriteLine(f);
            }



        }
    }
}
