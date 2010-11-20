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
        public Form1()
        {
            InitializeComponent();

            mymarshal = new JsMarshal(this);
            this.webBrowser1.ObjectForScripting = mymarshal;
        }

        string imageFolder = @"C:\Data\Datasets\t2";
        const string resultsFile = "results.txt";
        const string htmlFile = "a.html";

        public JsMarshal mymarshal = null;

        public void SetStaus(string status)
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

        private void button3_Click(object sender, EventArgs e)
        {
            // Step 1: the index parameters
            string indexFolder = Path.GetDirectoryName(textBox1.Text);
            string[] meta = File.ReadAllLines(Path.Combine(indexFolder, "meta.txt"));

            bool useSift        = (meta[0] == "sift");
            int numFeatures     = int.Parse(meta[1]);
            int numShapes       = int.Parse(meta[2]);
            int pageSize        = int.Parse(meta[3]);

            // Step 2: create the query file
            if (useSift)
            {
                if (File.Exists("query.txt")) { File.Delete("query.txt"); }
                string outfilename = "sift\\output-k" + numShapes + "-l" + numFeatures + ".txt";
                if (File.Exists(outfilename)) { File.Delete(outfilename); }
                //if (File.Exists("sift\\test.pgm")) { File.Delete("sift\\test.pgm"); }

                //Process conversion = new Process();
                //conversion.StartInfo.FileName = "convert.exe";
                //conversion.StartInfo.Arguments = textBox2.Text + " sift\\test.pgm";
                //conversion.Start();
                //conversion.WaitForExit();

                Process querymaker = new Process();
                querymaker.StartInfo.WorkingDirectory = Path.Combine(Application.StartupPath, "sift");
                querymaker.StartInfo.FileName = "SiftExtractor.exe";
                querymaker.StartInfo.Arguments = string.Format("{1} {2} -F {0}", textBox2.Text, numShapes, numFeatures);
                querymaker.StartInfo.CreateNoWindow = true;
                querymaker.Start();
                querymaker.WaitForExit();
                File.Copy(outfilename, "query.txt", true);
            }
            else
            {
                Process querymaker = new Process();
                querymaker.StartInfo.FileName = "ShapeIndexer.exe";
                querymaker.StartInfo.Arguments = string.Format("-o query.txt -l {1} -k {2} -F \"{0}\"", textBox2.Text, numFeatures, numShapes);
                querymaker.StartInfo.CreateNoWindow = true;
                querymaker.Start();
                querymaker.WaitForExit();
            }

            // Step 3: run the query
            Process queryRunner = new Process();
            queryRunner.StartInfo.FileName = "NearestNeighbor.exe";
            queryRunner.StartInfo.Arguments = string.Format("{0} {1} {2}", indexFolder, "query.txt", pageSize);
            queryRunner.StartInfo.CreateNoWindow = true;
            queryRunner.Start();
            queryRunner.WaitForExit();

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
.hate{background-image: url('res/hate-normal.png'); width:29px;}
.hate:hover{background-image: url('res/hate-hot.png'); width:29px;}
        </style>
    </head>
    <body>");

            for (int i = 0; i < filenames.Length && i<numericUpDown1.Value; i++)
            {
                wr.WriteLine("<div class=\"result\">");
                wr.WriteLine("\t<div><img class=\"resimg\" src=\"" + Path.Combine(imageFolder, filenames[i]) + "\" width=\"100\" /></div>");
                wr.WriteLine("\t<div>" + filenames[i] + "</div>");
                wr.WriteLine("\t<div><a class=\"rate like\" href=\"javascript:window.external.LikeButtonPressed('"+filenames[i]+"');\"></a>");
                wr.WriteLine("<a class=\"rate hate\" href=\"javascript:window.external.HateButtonPressed('" + filenames[i] + "');\"></a></div>");
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

        
    }
}
