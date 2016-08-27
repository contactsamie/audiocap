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
            webBrowser1.Navigate(txt_folder.Text);
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
        }

        private void button4_Click(object sender, EventArgs e)
        {
            _audioCapLib.WaveToMp3(lblFileName.Text,lblOutPut.Text);
        }

        private void txtFileName_TextChanged(object sender, EventArgs e)
        {
            UpdateFileName();
        }

        private void txt_folder_TextChanged(object sender, EventArgs e)
        {
            UpdateFileName();
        }
        int interval = 100;
        private double totalSize = 0;
        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                if (System.IO.File.Exists(lblFileName.Text))
                {
                    trackBar1.Minimum = 0;
                    trackBar1.Value = 0;
                    timer1.Interval = interval;
                    var duration = _audioCapLib.Play(lblFileName.Text);
                    totalSize = duration.TotalMilliseconds;
                    trackBar1.Maximum = (int)((duration.TotalMilliseconds) / interval);
                    timer1.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
         
        }

        private int positionMiliseconds = 0; 
        private void button7_Click(object sender, EventArgs e)
        {
            positionMiliseconds= trackBar1.Value + interval;
            timer1.Enabled = false;
            _audioCapLib.Stop();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                if (System.IO.File.Exists(lblFileName.Text))
                    System.IO.File.Delete(lblFileName.Text);
                if (System.IO.File.Exists(lblOutPut.Text))
                    System.IO.File.Delete(lblOutPut.Text);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }

            webBrowser1.Refresh();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            
            trackBar1.Value += 1;
            Text = trackBar1.Value.ToString();
            if (trackBar1.Value >= trackBar1.Maximum)
            {
                timer1.Enabled = false;
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            _audioCapLib.CutL(positionMiliseconds, totalSize,lblFileName.Text);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            _audioCapLib.CutR(positionMiliseconds, lblFileName.Text);
        }

     }
}
