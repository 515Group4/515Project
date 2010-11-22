using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NearestN
{
    class NearestN : IndexFile
    {
        private String file;
        public void pickIndex(String i)
        {
            file = i;
        }

        public int getNextMatch()
        {
            return 1; // will eventually be random
        }

        static void Main(string[] args)
        {
            Console.Write("TEST TEST TEST");
        }
    }
}
