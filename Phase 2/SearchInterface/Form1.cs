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
        }

        const string imageFolder = @"C:\Data\Datasets\t2";
        const string resultsFile = "results.txt";
        const string htmlFile = "a.html";

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                textBox2.Text = openFileDialog1.FileName;
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
                Process querymaker = new Process();
                if (File.Exists("sift\\output.txt")) { File.Delete("sift\\output.txt"); }
                if (File.Exists("query.txt")) { File.Delete("query.txt"); }
                querymaker.StartInfo.WorkingDirectory = Path.Combine(Application.StartupPath, "sift");
                querymaker.StartInfo.FileName = "SiftExtractor.exe";
                querymaker.StartInfo.Arguments = string.Format("{1} {2} -F \"{0}\"", textBox2.Text, numFeatures, numShapes);
                querymaker.StartInfo.CreateNoWindow = true;
                querymaker.Start();
                querymaker.WaitForExit();
                File.Move("sift\\output.txt", "query.txt");
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
            wr.WriteLine(
@"<html>
    <head>
        <title>Results</title>
        <style type=text/css>
body{font-family: sans-serif; font-size: 14px; }
.result{ display: inline; width: 120px; }
        </style>
    </head>
    <body>");

            for (int i = 0; i < filenames.Length; i++)
            {
                wr.WriteLine("<div class=\"result\"><div><img src=\"" + Path.Combine(imageFolder, filenames[i])
                    + "\" width=\"100\" /></div><div>" + filenames[i] + "</div></div>");
            }

            wr.WriteLine("</body></html>");
            wr.Flush();
            wr.Close();

            webBrowser1.Navigate(Path.Combine(Application.StartupPath, htmlFile));
        }

        
    }
}
