using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace SearchInterface
{
    public partial class Form1 : Form
    {
        private int siftFeatures;
        private int siftShapes;
        private int shapeFeatures;
        private int shapeShapes;
        private string queryImageName;
        private string[] siftQueryRepresentation;
        private string[] shapeQueryRepresentation;

        public Form1()
        {
            InitializeComponent();

            mymarshal = new JsMarshal(this);
            this.webBrowser1.ObjectForScripting = mymarshal;
            this.comboBox1.SelectedIndex = 0;
        }

        string imageFolder = @"C:\Data\Datasets\t3";
        const string resultsFile = "results.txt";
        const string htmlFile = "a.html";

        public JsMarshal mymarshal = null;

        public void SetStatus(string status)
        {
            this.statusbar.Text = status;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog3.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                textBox2.Text = openFileDialog3.FileName;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void siftQuery(string[] meta)
        {
            bool useSift = (meta[0] == "sift");
            siftFeatures = int.Parse(meta[1]);
            siftShapes = int.Parse(meta[2]);

            if (!useSift)
            {
                System.Windows.Forms.MessageBox.Show("You input a shape index in the SIFT box.  Shame on you");
                return;
            }

            if (File.Exists("query.txt")) { File.Delete("query.txt"); }
            string outfilename = "sift\\output-k" + siftShapes + "-l" + siftFeatures + ".txt";
            if (File.Exists(outfilename)) { File.Delete(outfilename); }

            Process querymaker = new Process();
            querymaker.StartInfo.WorkingDirectory = Path.Combine(Application.StartupPath, "sift");
            querymaker.StartInfo.FileName = "SiftExtractor.exe";
            querymaker.StartInfo.Arguments = string.Format("{1} {2} -F {0}", queryImageName, siftShapes, siftFeatures);
            querymaker.StartInfo.CreateNoWindow = true;
            querymaker.Start();
            querymaker.WaitForExit();
            File.Copy(outfilename, "query.txt", true);
        }

        private void shapeQuery(string[] meta)
        {
            bool useSift = (meta[0] == "sift");
            shapeFeatures = int.Parse(meta[1]);
            shapeShapes = int.Parse(meta[2]);

            if (useSift)
            {
                System.Windows.Forms.MessageBox.Show("You input a SIFT index in the shape box.  Shame on you");
                return;
            }

            Process querymaker = new Process();
            querymaker.StartInfo.FileName = "ShapeIndexer.exe";
            querymaker.StartInfo.Arguments = string.Format("-o query.txt -l {1} -k {2} -F \"{0}\"", queryImageName, shapeFeatures, shapeShapes);
            querymaker.StartInfo.CreateNoWindow = true;
            querymaker.Start();
            querymaker.WaitForExit();
        }

        private void runQuery(string indexFolder, int pageSize)
        {
            Process queryRunner = new Process();
            queryRunner.StartInfo.FileName = "NearestNeighbor.exe";
            queryRunner.StartInfo.Arguments = string.Format("{0} {1} {2}", indexFolder, "query.txt", pageSize);
            queryRunner.StartInfo.CreateNoWindow = true;
            queryRunner.Start();
            queryRunner.WaitForExit();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            queryImageName = textBox2.Text;
            string indexFolder = Path.GetDirectoryName(textBox4.Text);
            string[] meta = File.ReadAllLines(Path.Combine(indexFolder, "meta.txt"));

            siftQuery(meta);
            runQuery(indexFolder, int.Parse(meta[3]));

            indexFolder = Path.GetDirectoryName(textBox1.Text);
            meta = File.ReadAllLines(Path.Combine(indexFolder, "meta.txt"));
            
            shapeQuery(meta);
            runQuery(indexFolder, int.Parse(meta[3]));
            
            // For now this will always show the shape results
            string[] filenames = File.ReadAllLines(resultsFile);
            StreamWriter wr = new StreamWriter(htmlFile);

            if (Directory.Exists(textBox3.Text))
            {
                imageFolder = textBox3.Text;
            }
            wr.WriteLine(
@"<html>
    <head>
        <title>Results</title>
        <style type=text/css>
body{font-family: sans-serif; font-size: 14px; }
.result{ display: inline; width: 120px; }
.rate{margin: 0; padding: 0; border-width: 0; height:25px;}
.resimg{max-height: 120px; margin-top: 2em;}
.like{background-image: url('res/like-normal.png'); width:55px;}
.like:hover{background-image: url('res/like-hot.png'); width:55px;}
.like_active{background-image: url('res/like-hot.png'); width:55px;}
.hate{background-image: url('res/hate-normal.png'); width:29px;}
.hate:hover{background-image: url('res/hate-hot.png'); width:29px;}
.hate_active{background-image: url('res/hate-hot.png'); width:29px;}
        </style>
        <script>
            filenames = {};
            function likeClickHandler(filename, elem){
                var active = same(elem.className, 'active');
                window.external.LikeButtonPressed(filename, !active);
                elem.className = active ? 'like rate' : 'like_active rate';
                if(filenames[filename]==elem) return false;
                if(!filenames[filename]){ filenames[filename] = elem; return false};
                filenames[filename].className = 'hate rate';
                filenames[filename] = elem;
                return false;
            }

            function hateClickHandler(filename, elem){
                var active = same(elem.className, 'active');
                window.external.HateButtonPressed(filename, !active);
                elem.className = active ? 'hate rate' : 'hate_active rate';
                if(filenames[filename]==elem) return false;
                if(!filenames[filename]){ filenames[filename] = elem; return false};
                filenames[filename].className = 'like rate';
                filenames[filename] = elem;
                return false;
            }

            function changeClass(elem, lh){
                elem.className = lh + ' rate';
            }

            function same(arr, nm){
                arr = arr.split(' ');
                for(var i = 0; i<arr.length; i++){
                    if(arr[i].indexOf(nm)>0) return true;
                }
                return false;
            }
        </script>
    </head>
    <body>");

            for (int i = 0; i < filenames.Length && i<numericUpDown1.Value; i++)
            {
                wr.WriteLine("<div class=\"result\">");
                wr.WriteLine("\t<div><img class=\"resimg\" src=\"" + Path.Combine(imageFolder, filenames[i]) + "\" width=\"100\" /></div>");
                wr.WriteLine("\t<div>" + filenames[i] + "</div>");
                wr.WriteLine("\t<div><a class=\"rate like\" href=\"#1\" onClick=\"likeClickHandler('"+filenames[i]+"',this);\"></a>");
                wr.WriteLine("<a class=\"rate hate\" href=\"#1\" onClick=\"hateClickHandler('" + filenames[i] + "',this);\"></a></div>");
                wr.WriteLine("</div>");
            }

            wr.WriteLine("</body></html>");
            wr.Flush();
            wr.Close();

            webBrowser1.Navigate(Path.Combine(Application.StartupPath, htmlFile));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (openFileDialog2.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                textBox3.Text = Path.GetDirectoryName(openFileDialog2.FileName);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string shapeOutput = Path.Combine(Path.GetDirectoryName(textBox1.Text), "output.txt");
            FeedbackProcessor shapeFeedback = new FeedbackProcessor(queryImageName, mymarshal.getFeedback(), shapeOutput, shapeFeatures, shapeShapes);
            
            /* Commented because for now only Shape results are used
             
            string siftOutput = Path.Combine(Path.GetDirectoryName(textBox4.Text), "output.txt");
            FeedbackProcessor siftFeedback = new FeedbackProcessor(mymarshal.getFeedback(), siftOutput, siftFeatures, siftShapes);
            */

            shapeFeedback.computeAvgFeatures();
             string test = "test";
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (openFileDialog2.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                textBox4.Text = openFileDialog2.FileName;
            }
        }
    }
}
