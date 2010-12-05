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

            nnObj1.setFolderDir(@"C:\cse515\idx\sift_k50_l16\");
            nnObj1.setQueryFile(@"C:\cse515\sift-query.txt");

            NearestNeighbor.NearestNeighbor nnObj2 = new NearestNeighbor.NearestNeighbor();
            nnObj2.setFolderDir(@"C:\cse515\idx\shape_k8_l5\");
            nnObj2.setQueryFile(@"C:\cse515\query18.txt");

            NRA merge = new NRA(nnObj1, nnObj2);
            List<string> images = merge.mergeAndReturn(2);
            foreach (string i in images)
            {
                Console.WriteLine("image: " + i);
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
