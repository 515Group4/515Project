using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace SearchInterface
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        const string imageFolder = @"G:\Media\Images";
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
