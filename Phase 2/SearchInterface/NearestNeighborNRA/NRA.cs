using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections; // for hashtable

namespace NearestNeighborNRA
{
    class NRA
    {
        private int numAccess;
        private double scale1 = -1;
        private double scale2 = -1; // normalization factors for each object
        private NearestNeighbor obj1; // first object to merge
        private NearestNeighbor obj2; // second object to merge

        private Hashtable best = new Hashtable(); // tracks the best
        private Hashtable worst = new Hashtable(); // trakcs the worst
        private Hashtable single = new Hashtable(); // tracks elements who we've only seen from object so far

        private delegate double COMP(double x, double y); // compare method always returns a double and takes two doubles
        private COMP op; // delegate comparator method
        private List<string> dom;


        public NRA(NearestNeighbor in1, NearestNeighbor in2)
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
            MyResultSet rs = obj1.getFirst(70);

            this.addResultSet(rs, 1);
            rs = obj2.getFirst(70);
            this.addResultSet(rs, 2);
            this.removeSingle();
            this.getNumDominant();


            // 
            /*   while (this.getNumDominant() < target)
               {
                   Console.WriteLine("Num Dominant: " + this.getNumDominant());
                   if (numAccess % 2 == 0)
                   {
                       Console.WriteLine("OBJ1");
                       rs = obj1.getNext(1);
                       this.addResultSet(rs, 1);
                   }
                   else
                   {
                       Console.WriteLine("OBJ2");
                       rs = obj2.getNext(1);
                       this.addResultSet(rs, 2);
                   }
                
               }*/

            return dom;
        }

        private void addResultSet(MyResultSet r, int obj)
        {
            double score;
            foreach (MyResultEntry e in r)
            {
                score = fix(e.score);
                if (obj == 1)
                {
                    if (scale1 == -1)
                    {
                        scale1 = 1 / score;
                    }
                    score *= scale1;
                }
                if (obj == 2)
                {
                    if (scale2 == -1)
                    {
                        scale2 = 1 / score;
                    }
                    score *= scale2;
                }
                this.addData(fixfn(e.filename), score);
                numAccess++;
                Console.WriteLine("[" + numAccess + "] added image: " + fixfn(e.filename) + " score(" + score + ")");

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

        private void removeSingle()
        {
            foreach (DictionaryEntry d in single)
            {
                if (best.Contains(d.Key))
                {
                    best.Remove(d.Key);
                }
                if (worst.Contains(d.Key))
                {
                    worst.Remove(d.Key);
                }
            }
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
            if (x > 1)
            {
                return 1;
            }
            else if (x < 0.01)
            {
                return 100 * x;
            }
            else
            {
                return x;
            }
        }

        private static string fixfn(string f)
        {
            string pretty = f.TrimStart('0');
            if (!pretty.Contains(".tif"))
            {
                pretty = pretty + ".tif";
            }

            return pretty;
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


        public void doCleanup()
        {
            obj1.doCleanup();
            obj2.doCleanup();
        }
    }
}
