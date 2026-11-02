using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Backprop;

namespace Backprop_logicalOperations
{
    public partial class Form2 : Form
    {
        private Bitmap loaded;
        private NeuralNet nn;

        //to store pictures
        private class TrainingSample
        {
            public string FileName { get; set; }
            public double[] Pixels { get; set; }
            public double NormalizedTarget { get; set; }
            public double OriginalTarget { get; set; }

            public override string ToString()
            {
                return $"{FileName} -> Target: {OriginalTarget} (Norm: {NormalizedTarget:0.0})";
            }
        }

        private readonly List<TrainingSample> trainingSet = new List<TrainingSample>();
        private const int NetWidth = 16;
        private const int NetHeight = 16;
        private const int HiddenNodes = 20;

        public Form2()
        {
            InitializeComponent();
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
            //for progress bar updates
            backgroundWorker1.DoWork += backgroundWorker1_DoWork;
            backgroundWorker1.ProgressChanged += backgroundWorker1_ProgressChanged;
            backgroundWorker1.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;
        }

        //resizing for different size images and converting to grayscale
        private double[] ExtractNormalizedPixels(Image img, int width, int height)
        {
            double[] pixels = new double[width * height];

            using (Bitmap resized = new Bitmap(width, height))
            {
                using (Graphics g = Graphics.FromImage(resized))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                    g.DrawImage(img, 0, 0, width, height);
                }

                int index = 0;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Color c = resized.GetPixel(x, y);
                        //grayscaling 
                        double gray = (0.299 * c.R + 0.587 * c.G + 0.114 * c.B) / 255.0;
                        pixels[index++] = gray;
                    }
                }
            }
            return pixels;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }
        private void button4_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            if (loaded != null) loaded.Dispose();
            using (Bitmap tmp = new Bitmap(openFileDialog1.FileName))
            {
                loaded = new Bitmap(tmp);
            }
            pictureBox1.Image = loaded;
            lblStatus.Text = "Loaded: " + Path.GetFileName(openFileDialog1.FileName);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int inputs = NetWidth * NetHeight;
            nn = new NeuralNet(inputs, HiddenNodes, 1);
            lblStatus.Text = $"Network initialized ({inputs} inputs, {HiddenNodes} hidden, 1 output).";
            MessageBox.Show("Neural Network created successfully.");
        }
        private void button5_Click(object sender, EventArgs e)
        {
            if (loaded == null)
            {
                MessageBox.Show("Please load an image first.");
                return;
            }

            double rawTarget = (double)numericUpDown1.Value;
            //value calculation for desired output
            double normTarget = (rawTarget >= 1.0) ? (rawTarget / 10.0) : rawTarget;

            TrainingSample sample = new TrainingSample
            {
                FileName = Path.GetFileName(openFileDialog1.FileName),
                Pixels = ExtractNormalizedPixels(loaded, NetWidth, NetHeight),
                NormalizedTarget = normTarget,
                OriginalTarget = rawTarget
            };

            trainingSet.Add(sample);
            listBoxSamples.Items.Add(sample);
            lblStatus.Text = $"Added sample. Total in set: {trainingSet.Count}";
        }
        private void button6_Click(object sender, EventArgs e)
        {
            trainingSet.Clear();
            listBoxSamples.Items.Clear();
            lblStatus.Text = "Sample list cleared.";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (nn == null)
            {
                MessageBox.Show("Please create the neural network first (button1).");
                return;
            }
            if (trainingSet.Count < 4)
            {
                MessageBox.Show("Please add at least 4 images to the training set.");
                return;
            }
            if (backgroundWorker1.IsBusy) return;

            progressBar1.Value = 0;
            lblStatus.Text = "Training network...";
            backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            const int epochs = 2500;
            int step = Math.Max(1, epochs / 100);

            for (int ep = 0; ep < epochs; ep++)
            {
                if (backgroundWorker1.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                //training every image
                for (int s = 0; s < trainingSet.Count; s++)
                {
                    TrainingSample current = trainingSet[s];

                    for (int p = 0; p < current.Pixels.Length; p++)
                        nn.setInputs(p, current.Pixels[p]);

                    nn.setDesiredOutput(0, current.NormalizedTarget);
                    nn.learn();
                }

                if ((ep + 1) % step == 0)
                {
                    int pct = (int)((ep + 1) * 100L / epochs);
                    backgroundWorker1.ReportProgress(pct, ep + 1);
                }
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = Math.Min(100, Math.Max(0, e.ProgressPercentage));
            lblStatus.Text = $"Training... Epoch: {e.UserState}";
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                lblStatus.Text = "Training failed: " + e.Error.Message;
            }
            else if (e.Cancelled)
            {
                lblStatus.Text = "Training cancelled.";
            }
            else
            {
                progressBar1.Value = 100;
                lblStatus.Text = "Training complete!";
                MessageBox.Show("Training successfully finished.");
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (nn == null)
            {
                MessageBox.Show("Please create and train the network first.");
                return;
            }
            if (loaded == null)
            {
                MessageBox.Show("Please load an image to test.");
                return;
            }

            double[] testPixels = ExtractNormalizedPixels(loaded, NetWidth, NetHeight);

            for (int p = 0; p < testPixels.Length; p++)
                nn.setInputs(p, testPixels[p]);

            nn.run();

            double rawOutput = nn.getOuputData(0);
            double scaledOutput = rawOutput * 10.0;

            textBox1.Text = scaledOutput.ToString("0.00");
            lblStatus.Text = $"Test completed | Output: {scaledOutput:0.00} (Raw: {rawOutput:0.000})";
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void listBoxSamples_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
        }

        private void lblStatus_Click(object sender, EventArgs e)
        {
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }
    }
}