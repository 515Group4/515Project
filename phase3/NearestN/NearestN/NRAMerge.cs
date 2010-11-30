using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace NearestN
{
    class NRAMerge
    {
        private Hashtable best = new Hashtable();
        private Hashtable worst = new Hashtable();
        private Hashtable single = new Hashtable(); // placeholder

        private static COMP op = new COMP(avg); // todo -> accessor to change this guy

        /*
         * set up a delegate so we can do multiple kinds of functions to compare (ie min or average if we wanted)
         */
        private delegate double COMP(double x, double y);
        private static double min(double x, double y)
        {
            return Math.Min(x, y);
        }
        private static double avg(double x, double y)
        {
            return ((x + y) / 2);
        }

        public void printAll()
        {
            Console.WriteLine("Print all....");
            foreach( DictionaryEntry de in best)
            {
                Console.WriteLine(de.Key + "-> best: " + best[de.Key] + "\tworst: " + worst[de.Key]);
            }
           

        }

        public List<int> getDominant()
        {
            List<int> dom = new List<int>();
            var worsti = from k in worst.Keys.Cast<int>() orderby worst[k] descending select k;
            foreach (int de in worsti)
            {
                bool dominates = true;
                foreach (DictionaryEntry d2 in best)
                {
                    if(((int)d2.Key != de) && (!dom.Contains((int)d2.Key)))
                    {
                        if((double)worst[de] < (double)best[d2.Key])
                        {
                            dominates = false;
                        }
                    }
                }
                if(dominates)
                {
                    dom.Add((int)de);
                }
            }

            return dom;
        }
        public void addData(int image, double val)
        {
            if (!single.ContainsKey(image))
            {
                best.Add(image, op(val, 1));
                worst.Add(image, op(val, 0));
                single.Add(image, val);
            }
            else
            {
                best[image] = op((double)single[image], val);
                worst[image] = op((double)single[image], val);
                single.Remove(image);
            }
        }
    }
}

/*
goal number of matches
 * best
 * worst
 * single
 * 
 * addData
 * howManyDominant
 * 

*/