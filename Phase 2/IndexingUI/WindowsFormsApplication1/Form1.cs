using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace IndexingUI
{
    public partial class Form1 : Form
    {
        private string[] ops = { "Shape", "Sift" };

        public Form1()
        {
            InitializeComponent();
            algorithm.Items.AddRange(ops);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            // This is the Go method
            // First collect vars
            // Console.WriteLine("Begin Processing");
            String alg, dir, flags, targetDir = "c:\\Preetika\\MWD\\ProjectCode\\STR\\";
            bool shape;
            dir = textBox1.Text+"\\";
            decimal l = numericUpDown1.Value;
            decimal k = numericUpDown2.Value;
            shape = algorithm.Text == "Shape" ? true : false;
            if(shape){
                alg = "ShapeIndexer.exe";
                flags = "-l "+l+" -k "+k+" -o output.txt "+dir;
            }else{
                alg = "sift\\SiftExtractor.exe";
                flags = k+" "+l+" "+dir;
            };
            // MessageBox.Show("command: "+alg+" "+flags);

            string siftFileName = string.Format("output-k{0}-l{1}.txt", k, l);

            Process indexer = new Process();
            if (File.Exists(siftFileName)) { File.Delete(siftFileName); }
            if (File.Exists("output.txt")) { File.Delete("output.txt"); }
            indexer.StartInfo.FileName = alg;
            indexer.StartInfo.Arguments = flags;
            indexer.StartInfo.CreateNoWindow = true;
            indexer.Start();
            indexer.WaitForExit();
            Directory.CreateDirectory(targetDir);

            if (File.Exists("output.txt") || File.Exists(siftFileName))
            {
                String tmp = shape ? "output.txt" : siftFileName;
                File.Copy(tmp, targetDir + "1.txt");
            }else{
                MessageBox.Show("Error, output.txt doesn't exist");
                //Console.WriteLine("Error, output.txt doesn't exist");
            }

            if(File.Exists("c:\\Preetika\\MWD\\ProjectCode\\STR\\1.txt")){
                Process str = new Process();
                str.StartInfo.FileName = "STR.exe";
                str.StartInfo.CreateNoWindow = true;
                str.Start();
            }else{
                MessageBox.Show("1.txt was not created, STR not called");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void algorithm_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
