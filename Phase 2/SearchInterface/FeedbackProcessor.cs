using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SearchInterface
{
    internal delegate double MyFunctionType(double a, double b);

    class FeedbackProcessor
    {
        private Dictionary<string, int> data;
        public Dictionary<string, ImageObj> relImages;
        public Dictionary<string, ImageObj> notRelImages;
        public double[] threshold;
        public int[] featureRelIndex;
        public int[] featureNotRelIndex;
        public ImageObj query;
        
        private string[] index;
        private int numFeatures;
        private int numShapes;
        private int relShapeCount; // Keeps the total of shapes in all of the images
        private int notRelShapeCount;

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
            threshold = new double[numFeatures];
            featureRelIndex = new int[numFeatures];
            featureNotRelIndex = new int[numFeatures];
            relShapeCount = 0;
            notRelShapeCount = 0;

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

                // Set up query obj also
                if (sarr[0] == query.name)
                {
                    query.addShape(index[i]);
                }
            }

        }

        public double[] getFeatureAdjustedValues()
        {
            computeFeatureThresholds();
            computeFeatureRelevance();

            double pfR, pfI;
            double[] results = new double[numFeatures];

            for (int i = 0; i < numFeatures; i++)
            {
                pfR = relShapeCount == 0 || featureRelIndex[i] == 0 ? 1 : featureRelIndex[i] / (double)relShapeCount;
                pfI = notRelShapeCount == 0 || featureNotRelIndex[i] == 0 ? 1 : featureNotRelIndex[i] / (double)notRelShapeCount;

                pfR = pfR == 1 ? 1 : pfR / (1 - pfR);
                pfI = pfI == 1 ? 1 : pfI / (1 - pfI);

                results[i] = Math.Log(pfR/pfI);
            }

            return results;
        }

        public void computeFeatureThresholds()
        {
            // This requires iterating through twice
            // only the equation changes so the function is combined
            avgOrThresh(false);
            avgOrThresh(true);
        }

        private void avgOrThresh(bool thres)
        {
            // Feature thresholds are the avg Euclidean distance
            // from the query
            MyFunctionType worker = thres ? new MyFunctionType(square) : new MyFunctionType(sum);

            double[] avgFeature = new double[numFeatures];

            for(int i=0; i<numFeatures; i++){
                double d = 0;
                int total = 0;
                double avgVal = thres ? avgFeature[i] : 0;
                // For each feature go through each image
                foreach (KeyValuePair<String, ImageObj> pair in relImages)
                {
                    // And each of its shapes
                    foreach(Shape shape in pair.Value.shapes){
                        if (shape == null) break;
                        d += worker(shape.feature[i], avgVal);
                        total++;
                    }
                    
                }

                foreach (KeyValuePair<String, ImageObj> pair in notRelImages)
                {
                    // And each of its shapes
                    foreach (Shape shape in pair.Value.shapes)
                    {
                        if(shape==null) break;
                        d += worker(shape.feature[i], avgVal);
                        total++;
                    }
                }

                for (int j = 0; j < numFeatures; j++)
                {
                    // Still not sure about including the shape
                    // in the avg but it's here for now
                    if(query.shapes[j]==null) break;
                    d += worker(query.shapes[j].feature[i], avgVal);
                    total++;
                }

                if (thres)
                {
                    threshold[i] = (double)Math.Sqrt(d / total);
                }
                else
                {
                    avgFeature[i] = (double)(d / total);
                }
            }
        }

        private double square(double f, double s)
        {
            return ((f - s) * (f - s));
        }

        private double sum(double f, double s)
        {
            return f + s;
        }

        public void computeFeatureRelevance()
        {
            // Compute for both rel and not rel
            featureRel(true);
            featureRel(false);
        }

        private void featureRel(bool rel)
        {
            // If  feature is in the relevant set and is within the threshold
            // of the query, add one.  The argument says whether to use rel or not rel.
            var index = rel ? featureRelIndex : featureNotRelIndex;
            var images = rel ? relImages : notRelImages;

            for (int i=0; i<numShapes; i++)
            {
                // Go through each shape
                if (query.shapes[i] == null) break;

                for (int j = 0; j < numFeatures; j++)
                {
                    // And each feature and compute distance
                    // of feature to each relevent/not rel feature

                    // First go through each image in rel/not rel
                    foreach (KeyValuePair<string, ImageObj> pair in images)
                    {
                        // Now each shape in each image
                        foreach (Shape shape in pair.Value.shapes)
                        {
                            // Check the distance to the threshold.  If it is smaller, add
                            // one to the index score
                            if (shape == null) break; // No more shapes
                            double distance = Math.Abs(shape.feature[j] - query.shapes[i].feature[j]);
                            index[j] += distance < threshold[j] ? 1 : 0;
                            if(j==0){
                                // These are added to for each shape in the query being matched
                                // because technically that is the full relevance set
                                if (rel) { relShapeCount++; } else { notRelShapeCount++; };
                            }
                        }
                    }
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
