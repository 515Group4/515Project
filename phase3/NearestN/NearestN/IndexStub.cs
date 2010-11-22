using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NearestN
{
    interface IndexFile
    {
        // This method sets the index file we want to use (i.e., which feature)
        // I think this can just be thrown in the constructor but i'm not sure how to do that
        // with an interface
        void pickIndex(String filename);

        // Returns the next matched file (which I believe we represent as integers)
        int getNextMatch();
    }
}
