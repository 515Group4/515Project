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

    class Program
    {
        static string folder = "..\\..";
        static int pageSize = 420;
        static int numFeatures = 5;
        static string treeFileName = "STRTree.txt";
        static string leafFileName = "STRLeaf.txt";
        static int numNeighbors = 5;

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

        static int numPagesAccessed = 0;
        static BestMatch[] best;

        static void Main(string[] args)
        {
            // Step 1. Find the root node
            FileStream fs = new FileStream(Path.Combine(folder, treeFileName), FileMode.Open);
            FileStream fl = new FileStream(Path.Combine(folder, leafFileName), FileMode.Open);
            int offsetFromEnd = findTheRootOffset(fs);
            fs.Seek(-offsetFromEnd, SeekOrigin.End);
            byte[] rootpage = new byte[offsetFromEnd];
            fs.Read(rootpage, 0, offsetFromEnd);
            var root = InternalTreePage.ParseFromBytes(rootpage);

            fs.Seek(root.rectangles[0].pointer, SeekOrigin.Begin);
            byte[] intPage = new byte[pageSize];
            fs.Read(intPage, 0, pageSize);
            var level2 = InternalTreePage.ParseFromBytes(intPage);

            // Step 2. Find the query
            DataPoint query = new DataPoint(numFeatures);
            query.values[0] = 4150645879; query.values[1] = 55876; query.values[2] = 5; query.values[3] = 378477; query.values[4] = 134742016; // 0033.tiff

            // Step 3. Find a random point in the database
            best = new BestMatch[numNeighbors];
            for (int i = 0; i < numNeighbors; i++)
            {
                best[i].point = new DataPoint(numFeatures);
                best[i].point.values = new uint[] { 0, 0, 5277, 5414848, 0 }; // 1.1.02.tiff
                best[i].distance = best[i].point.distance(query);
            }

            // Step 4. recurse over the tree, trying to find candidate nodes to expand
            recurseFind(query, level2, fs, fl);

            foreach (var item in best)
            {
                Console.WriteLine(item.point.fileId);
            }

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
                        numPagesAccessed++;
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
