using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AudioCap.Lib;
using CSCore;
using CSCore.Codecs;
using CSCore.MediaFoundation;

namespace AudioCap.WinForm
{
    public partial class Form1 : Form
    {
        private readonly AudioCapLib _audioCapLib = new AudioCapLib();
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txt_folder.Text = folderBrowserDialog1.SelectedPath;
            }
          
        }

        public void UpdateFileName()
        {
            lblFileName.Text = txt_folder.Text + "\\" + txtFileName.Text+".wav";
            lblOutPut.Text= txt_folder.Text + "\\" + txtFileName.Text+".mp3";
          
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _audioCapLib.StartRecord(lblFileName.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _audioCapLib.StopRecord();
            _audioCapLib.WaveToMp3(lblFileName.Text, lblOutPut.Text);
            var duration = _audioCapLib.Play(lblOutPut.Text);
            _audioCapLib.Stop();
            textBox2.Text = duration.TotalMilliseconds.ToString();
        }

      
        private void txtFileName_TextChanged(object sender, EventArgs e)
        {
            UpdateFileName();
        }

        private void txt_folder_TextChanged(object sender, EventArgs e)
        {
            UpdateFileName();
            webBrowser1.Navigate(txt_folder.Text);
        }
        int interval = 500;
        private double totalSize = 0;
        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                    button7.PerformClick();
                    trackBar1.Minimum = 0;
                    trackBar1.Value = 0;
                    timer1.Interval = interval;
                    var duration = button8.Visible? _audioCapLib.Play(textBox3.Text) : _audioCapLib.Play(lblOutPut.Text);
                    totalSize = duration.TotalMilliseconds;
                    trackBar1.Maximum = (int)(duration.TotalMilliseconds);
                    timer1.Enabled = true;
               
            }
            catch (Exception ex)
            {
                
               // MessageBox.Show(ex.Message);
            }
         
        }

        private int positionMiliseconds = 0; 
        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                positionMiliseconds = trackBar1.Value * interval;
                timer1.Enabled = false;
                _audioCapLib.Stop();
            }
            catch (Exception)
            {
                
              
            }

        }

      
        private void Form1_Load(object sender, EventArgs e)
        {
            txt_folder.Text = "C:\\music";
            txtFileName.Text = "Sample";
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            try
            {
                trackBar1.Value += interval;
                Text = trackBar1.Value.ToString();

                label2.Text = (trackBar1.Value * interval / 500).ToString() + " s";
                if (trackBar1.Value >= trackBar1.Maximum)
                {
                    timer1.Enabled = false;
                    button7.PerformClick();
                }
            }
            catch (Exception)
            {

                timer1.Enabled = false;
                button7.PerformClick();
            }
        }

        private static int history = 0;
        private void button9_Click(object sender, EventArgs e)
        {
            if (!button8.Visible)
            {
                history++;
            }

            button8.Visible = true;
            var fileName = lblFileName.Text;
            var output = txt_folder.Text+ "\\" + history + ".mp3";
            textBox3.Text = output;
            //System.IO.File.Move(fileName, input);
            //Task.Delay(TimeSpan.FromSeconds(1)).Wait();
            //System.IO.File.Delete(fileName);
            //Task.Delay(TimeSpan.FromSeconds(1)).Wait();
            //var  outputFileName = fileName;

            _audioCapLib.Trim(Convert.ToInt32(textBox1.Text), Convert.ToInt32(textBox2.Text), fileName, output);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            button8.Visible = false;
        }

        private void label5_Click(object sender, EventArgs e)
        {
            textBox2.Text = (Convert.ToInt32(textBox1.Text) + Convert.ToInt32(Text)).ToString();
        }
    }
}
