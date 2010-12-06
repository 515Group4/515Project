using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SearchInterface
{
    internal delegate double MyFunctionType(double a, double b);
    internal delegate double AvgStdDev(Shape a, Shape b, double c);

    class FeedbackProcessor
    {
        private Dictionary<string, int> data;
        public Dictionary<string, ImageObj> relImages;
        public Dictionary<string, ImageObj> notRelImages;
        public Dictionary<string, ImageObj> untouchedImages;

        public double[] featureThreshold;
        private double[] avgFeature;
        public int[] featureRelIndex;
        public int[] featureNotRelIndex;

        public double[] shapeThreshold;
        private double[] avgShape;
        private int[] shapeRelIndex;
        private int[] shapeNotRelIndex;

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

            featureThreshold = new double[numFeatures];
            avgFeature = new double[numFeatures];
            featureRelIndex = new int[numFeatures];
            featureNotRelIndex = new int[numFeatures];

            shapeThreshold = new double[numShapes];
            avgShape = new double[numShapes]; 
            shapeRelIndex = new int[numShapes];
            shapeNotRelIndex = new int[numShapes];

            relShapeCount = 0;
            notRelShapeCount = 0;

            query = new ImageObj(numShapes, numFeatures, qry);
            relImages = new Dictionary<string,ImageObj>();
            notRelImages = new Dictionary<string,ImageObj>();
            untouchedImages = new Dictionary<string, ImageObj>();

            buildFeedbackDictionaries();
        }

        private void buildFeedbackDictionaries()
        {
            for (int i = 0; i < index.Length; i++)
            {
                string[] sarr = index[i].Split(',');

                // Set up query obj
                if (sarr[0] == query.name)
                {
                    query.addShape(index[i]);
                }

                // Create relevant and irrelevant and untouched
                // dictionaries
                if (data.Keys.Contains(sarr[0]))
                {
                    var toStore = untouchedImages;
                    if (data[sarr[0]] < 0)
                    {
                        toStore = notRelImages;
                    } else if(data[sarr[0]] > 0){
                        toStore = relImages;
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

        public double[] getShapeAdjustValues()
        {
            computeShapeThresholds();
            computeShapeRelevance();

            double pfR, pfI;
            double[] results = new double[numShapes];

            for (int i = 0; i < numShapes; i++)
            {
                pfR = relShapeCount == 0 || shapeRelIndex[i] == 0 ? 1 : shapeRelIndex[i] / (double)relShapeCount;
                pfI = notRelShapeCount == 0 || shapeNotRelIndex[i] == 0 ? 1 : shapeNotRelIndex[i] / (double)notRelShapeCount;

                pfR = pfR == 1 ? 1 : pfR / (1 - pfR);
                pfI = pfI == 1 ? 1 : pfI / (1 - pfI);

                results[i] = Math.Log(pfR / pfI);
            }

            return results;
        }

        private void computeShapeThresholds()
        {
            // Get avg shape match score for relevant
            // and then avg shape match for not rel
            calcShapeAvgOrStdDev(false);
            calcShapeAvgOrStdDev(true);
        }

        private double dev(Shape a, Shape b, double c)
        {
            double dist = shapeDistance(a, b);
            return (dist - c) * (dist - c);
        }

        private double sum(Shape a, Shape b, double c)
        {
            return shapeDistance(a, b);
        }

        private void calcShapeAvgOrStdDev(bool de)
        {
            // Calculates the avg score for the shapes in the query
            // image if de is false, and calculates the std dev of the
            // shapes when it is true
            AvgStdDev worker = de ? new AvgStdDev(dev) : new AvgStdDev(sum);

            // Go through each shape in the query object
            for (int i = 0; i < numShapes; i++)
            {
                if (query.shapes[i] == null) break;
                
                double d = 0;
                int total = 0;

                double avg = de ? avgShape[i] : 0;
                // For each feature go through each image
                foreach (KeyValuePair<String, ImageObj> pair in relImages)
                {
                    // And each of its shapes
                    foreach (Shape shape in pair.Value.shapes)
                    {
                        if (shape == null || pair.Key == query.name) break;
                        double tmp = worker(shape, query.shapes[i], avg);
                        d += tmp;
                        total++;
                    }

                }

                foreach (KeyValuePair<String, ImageObj> pair in notRelImages)
                {
                    // And each of its shapes
                    foreach (Shape shape in pair.Value.shapes)
                    {
                        if (shape == null || pair.Key == query.name) break;
                        double tmp = worker(shape, query.shapes[i], avg);
                        d += tmp;
                        total++;
                    }
                }
                
                foreach (KeyValuePair<String, ImageObj> pair in untouchedImages)
                {
                    // And each of its shapes
                    foreach (Shape shape in pair.Value.shapes)
                    {
                        if (shape == null || pair.Key == query.name) break;
                        double tmp = worker(shape, query.shapes[i], avg);
                        d += tmp;
                        total++;
                    }
                }

                if (de)
                {
                    shapeThreshold[i] = (double)Math.Sqrt(d / total);
                }
                else
                {
                    avgShape[i] = (double)(d / total);
                }
            }
        }

        private double shapeDistance(Shape s, Shape n)
        {
            double results = 0;

            for (int i = 0; i < numFeatures; i++)
            {
                results += (double)(s.feature[i] - n.feature[i]) * (double)(s.feature[i] - n.feature[i]);
            }

            return results==0 ? 0 : Math.Sqrt(results);
        }

        private void computeShapeRelevance()
        {
            shapeRel(true);
            shapeRel(false);
        }

        private void shapeRel(bool rel)
        {
            // If  feature is in the relevant set and is within the threshold
            // of the query, add one.  The argument says whether to use rel or not rel.
            var index = rel ? shapeRelIndex : shapeNotRelIndex;
            var images = rel ? relImages : notRelImages;
            if (rel) { relShapeCount = 0; } else { notRelShapeCount = 0; }

            for (int i = 0; i < numShapes; i++)
            {
                // Go through each shape
                if (query.shapes[i] == null) break;
                
                // First go through each image in rel/not rel
                foreach (KeyValuePair<string, ImageObj> pair in images)
                {
                    // Now each shape in each image
                    foreach (Shape shape in pair.Value.shapes)
                    {
                        // Check the distance to the threshold.  If it is smaller, add
                        // one to the index score
                        if (shape == null) break; // No more shapes
                        double distance = Math.Abs(shapeDistance(shape, query.shapes[i]));
                        index[i] += distance < shapeThreshold[i] ? 1 : 0; // two std devs
                        
                        // These are added to for each shape in the query being matched
                        // because technically that is the full relevance set
                        if (rel) { relShapeCount++; } else { notRelShapeCount++; };
                    }
                }

            }
        }

        /*
         * Below is the processing for getting feature relevance feedback
         * 
         */
        public double[] getFeatureAdjustValues()
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

        private void computeFeatureThresholds()
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

            for(int i=0; i<numFeatures; i++){
                double d = 0;
                int total = 0;
                double avgVal = thres ? avgFeature[i] : 0;
                // For each feature go through each image
                foreach (KeyValuePair<String, ImageObj> pair in relImages)
                {
                    // And each of its shapes
                    foreach(Shape shape in pair.Value.shapes){
                        if (shape == null || pair.Key == query.name) break;
                        d += worker(shape.feature[i], avgVal);
                        total++;
                    }
                    
                }

                foreach (KeyValuePair<String, ImageObj> pair in notRelImages)
                {
                    // And each of its shapes
                    foreach (Shape shape in pair.Value.shapes)
                    {
                        if (shape == null || pair.Key == query.name) break;
                        d += worker(shape.feature[i], avgVal);
                        total++;
                    }
                }

                foreach (KeyValuePair<String, ImageObj> pair in untouchedImages)
                {
                    // And each of its shapes
                    foreach (Shape shape in pair.Value.shapes)
                    {
                        if (shape == null || pair.Key == query.name) break;
                        d += worker(shape.feature[i], avgVal);
                        total++;
                    }
                }

                if (thres)
                {
                    featureThreshold[i] = (double)Math.Sqrt(d / total);
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

        private void computeFeatureRelevance()
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
            if (rel) { relShapeCount = 0; } else { notRelShapeCount = 0; }

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
                            index[j] += distance < featureThreshold[j] ? 1 : 0;
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
