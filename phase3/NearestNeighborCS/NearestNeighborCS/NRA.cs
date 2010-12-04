using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections; // for hashtable
using NearestNeighbor;

namespace NearestNeighborCS
{
    class NRA
    {
        private int numAccess;
        private NearestNeighbor.NearestNeighbor obj1; // first object to merge
        private NearestNeighbor.NearestNeighbor obj2; // second object to merge

        private Hashtable best = new Hashtable(); // tracks the best
        private Hashtable worst = new Hashtable(); // trakcs the worst
        private Hashtable single = new Hashtable(); // tracks elements who we've only seen from object so far

        private delegate double COMP(double x, double y); // compare method always returns a double and takes two doubles
        private COMP op; // delegate comparator method
        private List<string> dom; 


        public NRA(NearestNeighbor.NearestNeighbor in1, NearestNeighbor.NearestNeighbor in2)
        {
            numAccess = 0;
            obj1 = in1;
            obj2 = in2;
            op = new COMP(avg);
        }

        public List<string> mergeAndReturn(int target)
        {
            
            // we need to do these two getFirst calls to initialize all the stuff
            // inside the various NN objects for retrival.  
            MyResultSet rs = obj1.getFirst(1);
            this.addResultSet(rs);
            rs = obj2.getFirst(1);
            

            // dangerous to have this in a while?
            while (this.getNumDominant() < target)
            {
                Console.WriteLine("Num Dominant: " + this.getNumDominant());
                if (numAccess % 2 == 0)
                {
                    rs = obj1.getNext(1);
                }
                else
                {
                    rs = obj2.getNext(1);
                }
                this.addResultSet(rs);
            }
            
            return dom;
        }

        private void addResultSet(MyResultSet r)
        {
            foreach (MyResultEntry e in r)
            {
                this.addData(e.filename, fix(e.score));
                numAccess++;
                Console.WriteLine("[" + numAccess + "] added image: " + e.filename);
            }
        }

        private int getNumDominant()
        {
            int num = 0;
            dom = new List<string>();
            var worsti = from k in worst.Keys.Cast<string>() orderby worst[k] descending select k;
            foreach (string de in worsti)
            {
                bool dominates = true;
                foreach (DictionaryEntry d2 in best)
                {
                    if (((string)d2.Key != de) && (!dom.Contains((string)d2.Key)))
                    {
                        if ((double)worst[de] < (double)best[d2.Key])
                        {
                            dominates = false;
                        }
                    }
                }
                if (dominates)
                {
                    dom.Add((string)de);
                    num++;
                }
            }

            return num;
        }

        private void addData(string image, double val)
        {
            if (!single.ContainsKey(image))
            {
                best.Add(image, op(fix(val), 1000));
                worst.Add(image, op(fix(val), 0));
                single.Add(image, fix(val));
            }
            else
            {
                best[image] = op((double)single[image], fix(val));
                worst[image] = op((double)single[image], fix(val));
                single.Remove(image);
            }
        }

        // our "best case" score is 1000, so anything greater than
        // 1000 we normalize to 1000
        private static double fix(double x)
        {
            if (x > 1000)
            {
                return 1000;
            }
            else
            {
                return x;
            }
        }

        // potential for the COMP delegate
        public double avg(double x, double y)
        {
            return (double)((x + y) / 2);
        }

        // potential for the COMP delegate
        public double min(double x, double y)
        {
            return Math.Min(x, y);
        }

    }
}
