using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace NearestNeighbor
{
    class DataPoint
    {
        public uint[] values;
        public string fileId;

        public DataPoint(int numFeatures)
        {
            values = new uint[numFeatures];
        }

        public double distance(DataPoint other)
        {
            double d = 0;
            for (int i = 0; i < values.Length; i++)
			{
                d += (this.values[i] - other.values[i]) * (this.values[i] - other.values[i]);
			}

            return Math.Sqrt(d);
        }

        internal static DataPoint ParseString(string sm)
        {
            string[] sarr = sm.Split(',');
            DataPoint p = new DataPoint(sarr.Length-1);
            p.fileId = sarr[0];
            for (int i = 0; i < sarr.Length-1; i++)
            {
                p.values[i] = uint.Parse(sarr[i + 1]);
            }

            return p;
        }
    }

    class LeafPage
    {
        public List<DataPoint> values = new List<DataPoint>();

        public static LeafPage ParseFromBytes(byte[] data)
        {
            LeafPage t = new LeafPage();
            string s = ASCIIEncoding.ASCII.GetString(data);
            string[] sarr = s.Trim().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // each element of sarr is an MBR
            foreach (string sm in sarr)
            {
                if (!sm.StartsWith("\0"))
                {
                    t.values.Add(DataPoint.ParseString(sm));
                }
            }

            return t;
        }
    }

    /// <summary>
    /// Representa a multi-rectangle. a set of min values, a set of max values and a poitner
    /// </summary>
    class MBR
    {
        public uint[] minValues;
        public uint[] maxValues;
        public int pointer;

        public MBR(int numFeatures)
        {
            minValues = new uint[numFeatures];
            maxValues = new uint[numFeatures];
        }

        /// <summary>
        /// Given a string representation, creates and MBR
        /// </summary>
        /// <param name="s">The input string in the format "min, min, ... min, max, max, ... max, pointer"</param>
        /// <returns></returns>
        public static MBR ParseString(string s)
        {
            string[] sarr = s.Split(',');
            int numFeatures = sarr.Length/2;
            MBR m = new MBR(numFeatures);
            m.pointer = int.Parse(sarr[sarr.Length - 1]);
            for (int i = 0; i < numFeatures; i++)
			{
                m.minValues[i] = uint.Parse(sarr[i]);
                m.maxValues[i] = uint.Parse(sarr[numFeatures+i]);
			}

            return m;
        }

        /// <summary>
        ///  Gets the min-distance accoring the the Roussopoulos paper
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public double MinDistance(DataPoint p)
        {
            double d = 0;
            for (int i = 0; i < minValues.Length; i++)
            {
                double ri = 0; // make this double so that we dont have overflow
                if (p.values[i] < this.minValues[i]) { ri = this.minValues[i]; }
                else if (p.values[i] > this.maxValues[i]) { ri = this.maxValues[i]; }
                else { ri = p.values[i]; }

                d += (p.values[i] - ri) * (p.values[i] - ri);
            }

            return Math.Sqrt(d);
        }
    }

    /// <summary>
    /// Represents an internal Tree Page, with a list of MBRs
    /// </summary>
    class InternalTreePage
    {
        public List<MBR> rectangles = new List<MBR>();

        /// <summary>
        /// Reads in a page from a byte array and creates a logical representation for it
        /// </summary>
        /// <param name="data">The byte array read from </param>
        /// <returns>A created Internal Tree Page</returns>
        public static InternalTreePage ParseFromBytes(byte[] data)
        {
            InternalTreePage t = new InternalTreePage();
            string s = ASCIIEncoding.ASCII.GetString(data);
            string[] sarr = s.Trim().Split(new char[]{'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);

            // each element of sarr is an MBR
            foreach (string sm in sarr)
            {
                if (!sm.StartsWith("\0"))
                {
                    t.rectangles.Add(MBR.ParseString(sm));
                }
            }

            return t;
        }
    }

    struct BestMatch : IComparable<BestMatch>
    {
        public DataPoint point;
        public double distance;

        public int CompareTo(BestMatch other)
        {
            if (this.distance < other.distance) return -1;
            if (this.distance == other.distance) return 0;
            return 1;
        }
    }

    class MyResultSet : List<MyResultEntry>
    {
        public void AddTo(string fname, double distance)
        {
            MyResultEntry entry = new MyResultEntry() { filename = fname };
            if (this.Contains(entry))
            {
                this[this.IndexOf(entry)].score += distance == 0 ? 1000 : 1 / distance;
            }
            else
            {
                entry.score = distance == 0 ? 10000 : 100 / distance;
                this.Add(entry);
            }
        }
    }

    class MyResultEntry : IComparable<MyResultEntry>, IEquatable<MyResultEntry>
    {
        public string filename;
        public double score;

        public int CompareTo(MyResultEntry other)
        {
            if (this.score > other.score) return -1;
            if (this.score == other.score) return 0;
            return 1;
        }

        public bool Equals(MyResultEntry other)
        {
            return this.filename == other.filename;
        }
    }

    class Program
    {
        static string folder = "..\\..";
        static int pageSize = 6000;
        static string treeFileName = "STRTree.txt";
        static string leafFileName = "STRLeaf.txt";
        static int numNeighbors = 10;
        static int numPagesAccessed = 0;
        static BestMatch[] best;
        static string queryfile = null;
        static string outputfile = "results.txt";
        static List<int> pointers = new List<int>();

        static int findTheRootOffset(FileStream fs)
        {
            // problem: we dont know where the root is.
            fs.Seek(-1, SeekOrigin.End);
            int rb = -1;
            int offsetFromEnd = 0;
            while ((rb = fs.ReadByte()) != '\0')
            {
                fs.Seek(-2, SeekOrigin.Current);
                offsetFromEnd++;
            }

            return offsetFromEnd;
        }


        static void Main(string[] args)
        {
            // Usage:
            // NearestNeighbour [indexFolder queryfile pagesize=6000 outputfile=results.txt]

            if (args.Length > 0)
            {
                folder = args[0];
            }
            if (args.Length > 1)
            {
                queryfile = args[1];
            }
            if (args.Length > 2)
            {
                pageSize = int.Parse(args[2]);
            }
            if (args.Length > 3)
            {
                outputfile = args[3];
            }

            // Step 1. Find the root node
            FileStream fs = new FileStream(Path.Combine(folder, treeFileName), FileMode.Open);
            FileStream fl = new FileStream(Path.Combine(folder, leafFileName), FileMode.Open);
            int offsetFromEnd = findTheRootOffset(fs);
            fs.Seek(-offsetFromEnd, SeekOrigin.End);
            byte[] rootpage = new byte[offsetFromEnd];
            fs.Read(rootpage, 0, offsetFromEnd);
            var root = InternalTreePage.ParseFromBytes(rootpage);

            long TotalNumSectors = (fs.Length + fl.Length) / (long)pageSize;
            long ActualPagesAccessed = 0;
            long NumQueryAccesses = 0;

            //fs.Seek(root.rectangles[0].pointer, SeekOrigin.Begin);
            //byte[] intPage = new byte[pageSize];
            //fs.Read(intPage, 0, pageSize);
            //var level2 = InternalTreePage.ParseFromBytes(intPage);

            // Step 2. Find the query
            MyResultSet set = new MyResultSet();
            if (queryfile != null)
            {
                string[] querystrings = File.ReadAllLines(queryfile);
                foreach (string szQuery in querystrings)
                {
                    // reset the recursion breaker
                    pointers.Clear();

                    string[] queryterms = szQuery.Split(new char[] { ',' }, StringSplitOptions.None);
                    int numFeatures = queryterms.Length-1;
                    DataPoint query = new DataPoint(numFeatures);
                    for (int i = 0; i < numFeatures; i++)
                    {
                        query.values[i] = uint.Parse(queryterms[i+1]); // avoid the filename
                    }

                    #region Sample queries
                    //query.values = new uint[]{ 0, 0, 5277, 5414848, 0 }; // 1.1.02.tiff
                    //query.values = new uint[] { 201330451, 0, 15444, 2180706326, 138805416 };//4.1.01.tiff,
                    //query.values = new uint[] { 0, 63080, 1611, 423413, 143130760 };//1.4.10.tiff,
                    //query.values = new uint[] { 1266436991, 0, 138795, 392952608, 138805928 };//7.2.01
                    //query.values = new uint[] { 336, 96, 315, 83, 119, 149, 339, 26, 331, 82, 99, 43, 167, 85, 154, 36 }; // 1002.tif,
                    //query.values = new uint[] { 82, 4, 37, 5, 69, 0, 51, 13, 20, 29, 1, 402, 39, 1, 40, 18 };//1042.tif
                    //query.values = new uint[] { 36, 59, 98, 10, 42, 177, 89, 17, 99, 45, 13, 67, 167, 43, 81, 26 };//1008.tif,
                    //query.values = new uint[] { 127, 143, 146, 76, 115, 227, 342, 60, 297, 147, 113, 39, 228, 112, 101, 121 }; // 104.tif, 
                    //query.values = new uint[] { 0, 0, 0, 0, 95, 41, 0, 0, 263, 35, 0, 2, 101, 218, 139, 5, 0, 0, 0, 0, 49, 44, 54, 8, 200, 7, 7, 21, 125, 44, 162, 38, 0, 0, 0, 0, 72, 0, 41, 68, 167, 0, 2, 96, 46, 11, 117, 71, 0, 0, 0, 0, 80, 0, 0, 50, 154, 0, 0, 123, 15, 4, 11, 85 };
                    //query.values = new uint[]{4150645879, 55876, 5, 378477, 134742016}; // 0033.tiff
                    //query.values = new uint[] { 0, 73, 76, 1, 54, 44, 1, 9, 161, 0, 0, 2, 115, 0, 0, 2, 0, 207, 41, 0, 45, 140, 78, 1, 150, 4, 
                    //    6, 6, 139, 0, 0, 6, 0, 13, 116, 6, 32, 6, 195, 7, 145, 0, 11, 21, 142, 0, 1, 2, 0, 0, 152, 31, 21, 0, 130, 95, 139, 0, 
                    //    4, 54, 116, 0, 1, 6 };

                    #endregion

                    // Step 3. Find a random point in the database
                    byte[] leafPage = new byte[pageSize];
                    fl.Read(leafPage, 0, pageSize);
                    LeafPage p = LeafPage.ParseFromBytes(leafPage);

                    best = new BestMatch[numNeighbors];
                    for (int i = 0; i < numNeighbors; i++)
                    {
                        best[i].point = new DataPoint(numFeatures);
                        best[i].point.fileId = p.values[0].fileId;
                        best[i].point.values = p.values[0].values;
                        best[i].distance = best[i].point.distance(query);
                    }

                    // Step 4. recurse over the tree, trying to find candidate nodes to expand
                    recurseFind(query, root, fs, fl);

                    foreach (var item in best)
                    {
                        set.AddTo(item.point.fileId, item.distance);
                    }

                    // count the number of pages expanded
                    ActualPagesAccessed  += pointers.Count;
                    NumQueryAccesses++;
                }
            }

            List<string> outputfilenames = new List<string>();
            foreach (var item in set)
            {
                Console.WriteLine("File: {0}: \t {1}", item.filename, item.score);
                outputfilenames.Add(item.filename);
            }

            File.WriteAllLines(outputfile, outputfilenames.ToArray());
            File.WriteAllText("pagestatistics.txt", "Percentage: " + ((100.0 * ActualPagesAccessed) / ((double)TotalNumSectors * NumQueryAccesses)).ToString("F4"));

            //foreach (var item in best)
            //{
            //    Console.Write(item.point.fileId + "\t\t: " );
            //    for (int j = 0; j < numFeatures; j++)
            //    {
            //        //Console.Write(", " + item.point.values[j]);
            //    }
            //    Console.WriteLine();
            //}

            // Step f. Cleanup
            fs.Close();
            fl.Close();
        }


        private static void recurseFind(DataPoint query, InternalTreePage page, FileStream fs, FileStream fl)
        {
            foreach (var node in page.rectangles)
            {
                if (node.MinDistance(query) <= best[numNeighbors-1].distance)
                {
                    if (pointers.Contains(node.pointer))
                    {
                        continue;
                    }
                    else
                    {
                        pointers.Add(node.pointer);
                    }

                    numPagesAccessed++;
                    // check if its a leaf
                    if (node.pointer < 0)
                    {
                        fl.Seek(-node.pointer, SeekOrigin.Begin);
                        byte[] leafPage = new byte[pageSize];
                        fl.Read(leafPage, 0, pageSize);
                        LeafPage p = LeafPage.ParseFromBytes(leafPage);

                        // go through all the elements in p and check which one is the closest
                        foreach (var item in p.values)
                        {
                            var bestDist_t = item.distance(query);
                            if (bestDist_t < best[numNeighbors-1].distance)
                            {
                                List<BestMatch> ll = new List<BestMatch>();
                                ll.AddRange(best);
                                ll.Add(new BestMatch() { distance = bestDist_t, point = item });
                                ll.Sort();

                                ll.RemoveAt(numNeighbors);
                                best = ll.ToArray();
                            }
                        }
                    }
                    else // its an internal node
                    {
                        fs.Seek(node.pointer, SeekOrigin.Begin);
                        byte[] intPage = new byte[pageSize];
                        fs.Read(intPage, 0, pageSize);
                        var next_level = InternalTreePage.ParseFromBytes(intPage);

                        recurseFind(query, next_level, fs, fl);
                    }
                }
            }
        }

    }
}
