using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SearchInterface
{
    class FeedbackProcessor
    {
        private Dictionary<string, int> data;
        public Dictionary<string, ImageObj> relImages;
        public Dictionary<string, ImageObj> notRelImages;
        public ImageObj query;
        
        private string[] index;
        private int numFeatures;
        private int numShapes;
        

        public FeedbackProcessor(string qry, Dictionary<string, int> results, string outputLocation, int ft, int shp)
        {
            data = results;
            try
            {
                index = File.ReadAllLines(outputLocation);
            } catch(Exception e){
                System.Windows.Forms.MessageBox.Show("Couldn't find output file"+e);
            }
          
            numFeatures = ft;
            numShapes = shp;

            query = new ImageObj(numShapes, numFeatures, qry);
            relImages = new Dictionary<string,ImageObj>();
            notRelImages = new Dictionary<string,ImageObj>();

            buildFeedbackDictionaries();
        }

        private void buildFeedbackDictionaries()
        {
            for (int i = 0; i < index.Length; i++)
            {
                string[] sarr = index[i].Split(',');
                if (data.Keys.Contains(sarr[0]))
                {
                    var toStore = relImages;
                    if (data[sarr[0]] < 0)
                    {
                        toStore = notRelImages;
                    }

                    if (!toStore.ContainsKey(sarr[0]))
                    {
                        ImageObj o = new ImageObj(numShapes, numFeatures, sarr[0]);
                        toStore.Add(sarr[0], o);
                    }

                    toStore[sarr[0]].addShape(index[i]);
                }
            }

        }
    }

    class Shape
    {
        public uint[] feature;
        public Shape(int ft, string val)
        {
            feature = new uint[ft];
            string[] values = val.Split(',');

            for (int i = 0; i < ft; i++)
            {
                // Start at one since 0 is the name of the shape
                feature[i] = uint.Parse(values[i + 1]);
            }
        }
    }

    class ImageObj
    {
        public Shape[] shapes;
        public string name;
        private int numFeatures;
        private int numShapes;
        private int idx;

        public ImageObj(int numshp, int numft, string nm)
        {
            numShapes = numshp;
            numFeatures = numft;
            idx = 0;

            shapes = new Shape[numShapes];
            name = nm;
        }

        public void addShape(string shape){
            if (idx >= numShapes) {
                System.Windows.Forms.MessageBox.Show("Idx is greater than numShapes.  Weird.  shape: "+name+" features: "+shape+" idx: "+idx);
                return;
            };
            shapes[idx] = new Shape(numFeatures, shape);
            idx++;
        }

    }
}
