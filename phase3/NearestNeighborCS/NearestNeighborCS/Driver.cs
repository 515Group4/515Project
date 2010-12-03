using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NearestNeighbor;

namespace NearestNeighborCS
{
    class Driver
    {
        static void Main(string[] args)
        {
            // Usage:
            // NearestNeighbour [indexFolder queryfile pagesize=6000 outputfile=results.txt]
            NearestNeighbor.NearestNeighbor nnObj1 = new NearestNeighbor.NearestNeighbor();

            nnObj1.setFolderDir(@"C:\cse515\idx\sift_k200_l16\");
            nnObj1.setQueryFile(@"query.txt");

            
            MyResultSet resultSet = nnObj1.getFirst(10);
            foreach (MyResultEntry e in resultSet)
            {
                Console.WriteLine("Name: " + e.filename + " score: " + e.score);
            }
            resultSet = nnObj1.getNext(5);
            Console.WriteLine("Next 5....");
            foreach (MyResultEntry e in resultSet)
            {
                Console.WriteLine("Name: " + e.filename + " score: " + e.score);
            }
            //resultSet = nnObj1.getNext(7);


            //resultSet = nnObj.getNext(10);
            //resultSet = nnObj.getNext(10);
            //resultSet = nnObj.getNext(10);
            //resultSet = nnObj.getNext(10);
            //resultSet = nnObj.getNext(10);
            //resultSet = nnObj.getNext(10);
            //resultSet = nnObj.getNext(10);
            //resultSet = nnObj.getNext(10);

            //feature index starts from 0, so if you wanna change the 5th feature, pass 4 here...
            //nnObj.setFeatureWeight(2, 4.0);
            //MyResultSet newResults = nnObj.getFirst(10);
            //newResults = nnObj.getNext(10);
            //newResults = nnObj.getNext(10);
            //newResults = nnObj.getNext(10);
            //newResults = nnObj.getNext(10);
            //newResults = nnObj.getNext(10);
            //newResults = nnObj.getNext(10);
            //newResults = nnObj.getNext(10);
            //newResults = nnObj.getNext(10);
            //newResults = nnObj.getNext(10);

            //nnObj.setQueryFile(@"E:\CG\NearestNeighborCS\NearestNeighborCS\bin\Debug\query1.txt");
            //newResults = nnObj.getFirst(10);
        }
    }
}
