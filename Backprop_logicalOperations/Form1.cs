using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Backprop;
namespace Backprop_logicalOperations
{
    public partial class Form1 : Form
    {
        NeuralNet nn;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            nn = new NeuralNet(2, 10, 1); //input nn, hidden nn (how many  braincells, output nn

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            for (int i = 0; i < 1000; i++)
            {

                //perform AND operation
                ////1 AND 1 = 1
                //nn.setInputs(0, 1.0);
                //nn.setInputs(1, 1.0);
                //nn.setDesiredOutput(0, 1.0);
                //nn.learn();

                ////0 AND 1 = 0
                //nn.setInputs(0, 0);
                //nn.setInputs(1, 1.0);
                //nn.setDesiredOutput(0, 0);
                //nn.learn();

                ////1 AND 0 = 0
                //nn.setInputs(0, 1.0);
                //nn.setInputs(1, 0);
                //nn.setDesiredOutput(0, 0);
                //nn.learn();

                ////0 AND 0 = 0
                //nn.setInputs(0, 0);
                //nn.setInputs(1, 0);
                //nn.setDesiredOutput(0, 0);
                //nn.learn();

                //XOR operation
                //1 AND 1 = 0
                nn.setInputs(0, 1.0);
                nn.setInputs(1, 1.0);
                nn.setDesiredOutput(0, 0);
                nn.learn();

                //0 AND 1 = 1
                nn.setInputs(0, 0);
                nn.setInputs(1, 1.0);
                nn.setDesiredOutput(0, 1);
                nn.learn();

                //1 AND 0 = 1
                nn.setInputs(0, 1.0);
                nn.setInputs(1, 0);
                nn.setDesiredOutput(0, 1);
                nn.learn();

                //0 AND 0 = 0
                nn.setInputs(0, 0);
                nn.setInputs(1, 0);
                nn.setDesiredOutput(0, 0);
                nn.learn();

            }
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            nn.setInputs(0, Convert.ToDouble(textBox1.Text));
            nn.setInputs(1, Convert.ToDouble(textBox2.Text));
            nn.run();
            textBox3.Text = "" + nn.getOuputData(0);
        }
    }
}
