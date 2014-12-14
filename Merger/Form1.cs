using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Merger
{
    public partial class Form1 : Form
    {
        double _TimeOffst = double.Parse(System.Configuration.ConfigurationSettings.AppSettings["TimeOffset"].Trim());
        
        public Form1()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Start")
            {
                timer1.Interval = int.Parse(System.Configuration.ConfigurationSettings.AppSettings["CheckTimeMinute"].Trim()) * 60 * 1000;
                timer1.Enabled = true;
                timer1.Start();
                button1.Text = "Stop";
                button1.BackColor = Color.Red;
                MainJob();
            }
            else
            {
                timer1.Enabled = false;
                button1.Text = "Start";
                button1.BackColor = Color.MidnightBlue;
            }
           
          
        }
        protected void SetDateToLbl()
        {
            this.Text ="News Merger V1.5 :"+ DateTime.Now.AddHours(_TimeOffst).ToString() +" (Last Activity)";
        }
        protected DateTime ConvertFnameToTime(string FileName)
        {
            DateTime Tm = DateTime.Now;
            string Hour = FileName.Substring(0, 2);
            string Min = FileName.Substring(2, 2);
            string Sec = FileName.Substring(4, 2);


            Tm = DateTime.Parse(DateTime.Now.Year.ToString() + "/" + DateTime.Now.Month.ToString() + "/" +
                DateTime.Now.Day.ToString() + "  " + Hour + ":" + Min + ":" + Sec);
            return Tm;
        }
        protected bool Convert(string InFile, string OutFile, string TemDir, string Ext)
        {
            // -i "concat:1.mpg|2.mpg"  -c copy    001.mpg

            progressBar1.Value = 0;
            label1.Text = "0%";

            Process proc = new Process(); if (Environment.Is64BitOperatingSystem)
            {
                proc.StartInfo.FileName = Path.GetDirectoryName(Application.ExecutablePath) + "\\ffmpeg64";
            }
            else
            {
                proc.StartInfo.FileName = Path.GetDirectoryName(Application.ExecutablePath) + "\\ffmpeg32";
            }

            DirectoryInfo Dir = new DirectoryInfo(TemDir);
            if (!Dir.Exists)
            {
                Dir.Create();
            }

            if (Ext == "mpg")
            {
                proc.StartInfo.Arguments = "-i " + "\"" + InFile + "\"" + "       -b " + System.Configuration.ConfigurationSettings.AppSettings["Bitrate"].Trim() + "k     -y  " + "\"" + OutFile + "\"";
            }
            else
            {
                //ffmpeg -i 001.mpg  -b 700k     -ar 11025 -y   113.flv
                proc.StartInfo.Arguments = "-i " + "\"" + InFile + "\"" + "   -r "+System.Configuration.ConfigurationSettings.AppSettings["Fps"].Trim()+"    -b  "+ System.Configuration.ConfigurationSettings.AppSettings["Bitrate"].Trim() + "k      -ar 44100 -y  " + "\"" + OutFile + "\"";
                label5.Text = OutFile.Replace("\\\\","\\");
            }
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.EnableRaisingEvents = true;
            proc.Exited += new EventHandler(myProcess_Exited);
            if (!proc.Start())
            {
                return false;
            }
          
            richTextBox2.Text += "Convert Started : "+InFile + "\n";
            richTextBox2.SelectionStart = richTextBox2.Text.Length;
            richTextBox2.ScrollToCaret();
            Application.DoEvents();
            
            
            
            proc.PriorityClass = ProcessPriorityClass.RealTime;
            StreamReader reader = proc.StandardError;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (richTextBox1.Lines.Length > 10)
                {
                    richTextBox1.Text = "";
                }

                FindDuration(line, "1");
                richTextBox1.Text += (line) + " \n";
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
                Application.DoEvents();
            }
            proc.Close();
            richTextBox2.Text += "Convert Finished : " + InFile + "\n";
            richTextBox2.SelectionStart = richTextBox2.Text.Length;
            richTextBox2.ScrollToCaret();           
            Application.DoEvents();
            
            return true;
        }
        private void myProcess_Exited(object sender, System.EventArgs e)
        {
            this.Invoke(new MethodInvoker(delegate()
            {
                progressBar1.Value = progressBar1.Maximum;
                label1.Text = ((progressBar1.Value * 100) / progressBar1.Maximum).ToString() + "%";
                Application.DoEvents();

            }));
        }
        protected bool Concat(string InFile1, string InFile2, string OutFile, string TemDir)
        {
            // -i "concat:1.mpg|2.mpg"  -c copy    001.mpg

            progressBar1.Value = 0;
            label1.Text = "0%";

            Process proc = new Process();
            if (Environment.Is64BitOperatingSystem)
            {
                proc.StartInfo.FileName = Path.GetDirectoryName(Application.ExecutablePath) + "\\ffmpeg64";
            }
            else
            {
                proc.StartInfo.FileName = Path.GetDirectoryName(Application.ExecutablePath) + "\\ffmpeg32";
            }
            DirectoryInfo Dir = new DirectoryInfo(TemDir);
            if (!Dir.Exists)
            {
                Dir.Create();
            }
            proc.StartInfo.Arguments = "-i " + "\"concat:" + InFile1 + "|" + InFile2 + "\"" + "   -c copy   -y  " + "\"" + OutFile + "\"";
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.EnableRaisingEvents = true;
            proc.Exited += new EventHandler(myProcess_Exited);
            if (!proc.Start())
            {
                return false;
            }
            proc.PriorityClass = ProcessPriorityClass.RealTime;
            StreamReader reader = proc.StandardError;
            string line;



            richTextBox2.Text += "Merge Started" +"\n";         
            richTextBox2.SelectionStart = richTextBox2.Text.Length;
            richTextBox2.ScrollToCaret();
            Application.DoEvents();
            
            
            
            while ((line = reader.ReadLine()) != null)
            {
                if (richTextBox1.Lines.Length > 10)
                {
                    richTextBox1.Text = "";
                }
                FindDuration(line, "1");
                richTextBox1.Text += (line) + " \n";
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
                Application.DoEvents();
            }
            proc.Close();
            richTextBox2.Text += "Merge Finished" + "\n";
            richTextBox2.SelectionStart = richTextBox2.Text.Length;
            richTextBox2.ScrollToCaret();
            Application.DoEvents();
            return true;
        }
        protected void FindDuration(string Str, string ProgressControl)
        {
            string TimeCode = "";
            if (Str.Contains("Duration:"))
            {
                TimeCode = Str.Substring(Str.IndexOf("Duration: "), 21).Replace("Duration: ", "").Trim();
                string[] Times = TimeCode.Split('.')[0].Split(':');
                double Frames = double.Parse(Times[0].ToString()) * (3600) * (25) +
                    double.Parse(Times[1].ToString()) * (60) * (25) +
                    double.Parse(Times[2].ToString()) * (25);
                if (ProgressControl == "1")
                {
                    progressBar1.Maximum = int.Parse(Frames.ToString());
                }
                else
                {

                }
            }
            if (Str.Contains("time="))
            {
                try
                {
                    string CurTime = "";
                    CurTime = Str.Substring(Str.IndexOf("time="), 16).Replace("time=", "").Trim();
                    string[] CTimes = CurTime.Split('.')[0].Split(':');
                    double CurFrame = double.Parse(CTimes[0].ToString()) * (3600) * (25) +
                        double.Parse(CTimes[1].ToString()) * (60) * (25) +
                        double.Parse(CTimes[2].ToString()) * (25);

                    if (ProgressControl == "1")
                    {
                        progressBar1.Value = int.Parse(CurFrame.ToString());

                        label1.Text = ((progressBar1.Value * 100) / progressBar1.Maximum).ToString() + "%";
                    }
                    else
                    {

                    }

                    //label3.Text = CurFrame.ToString();
                    Application.DoEvents();
                }
                catch
                {


                }
            }
            if (Str.Contains("fps="))
            {

                string Speed = "";

                Speed = Str.Substring(Str.IndexOf("fps="), 8).Replace("fps=", "").Trim();

                label4.Text = "Speed: " + (float.Parse(Speed) / 25).ToString() + " X ";
                Application.DoEvents();

            }
        }
    
        private void timer1_Tick(object sender, EventArgs e)
        {
            SetDateToLbl();
            MainJob();
        }
        protected void MainJob()
        {
            try
            {
                richTextBox2.Text = "";
                timer1.Enabled = false;
                richTextBox2.Text += "Check File Started : " + "\n";
                richTextBox2.SelectionStart = richTextBox2.Text.Length;
                richTextBox2.ScrollToCaret();
                Application.DoEvents();

                string CurDate = DateTime.Now.AddHours(_TimeOffst).Year + "-" + string.Format("{0:00}", DateTime.Now.AddHours(_TimeOffst).Month) +
                    "-" + string.Format("{0:00}", DateTime.Now.AddHours(_TimeOffst).Day);

                string StartTime = "";
                string EndTime = "";


                DirectoryInfo Dir = new DirectoryInfo(System.Configuration.ConfigurationSettings.AppSettings["Source"].Trim() + CurDate);
                if (Dir.Exists)
                {


                    for (int i = 0; i < 24; i++)
                    {
                        SetDateToLbl();
                        int Hour = i;
                        string File1 = "";
                        string File2 = "";
                        richTextBox2.Text += "\n================================\n";
                        richTextBox2.Text += "News Hour : " + Hour + "\n";
                        richTextBox2.SelectionStart = richTextBox2.Text.Length;
                        richTextBox2.ScrollToCaret();
                        label3.Text = Hour.ToString();
                        Application.DoEvents();

                        StartTime = string.Format("{0:00}", Hour) + System.Configuration.ConfigurationSettings.AppSettings["BeforNewsFileMinSec"].Trim();
                        EndTime = string.Format("{0:00}", Hour) + System.Configuration.ConfigurationSettings.AppSettings["AfterNewsFileMinSec"].Trim();

                       string   StartTime2 = string.Format("{0:00}", Hour) + System.Configuration.ConfigurationSettings.AppSettings["BeforNewsFileMinSecEnd"].Trim();
                       string EndTime2 = string.Format("{0:00}", Hour) + System.Configuration.ConfigurationSettings.AppSettings["AfterNewsFileMinSecEnd"].Trim();
                        string DestFile = "";

                        //if (Hour != 0)
                        //{
                            //if (!Directory.Exists(System.Configuration.ConfigurationSettings.AppSettings["Temp"].Trim() + CurDate + "\\"))
                            //{
                            //    Directory.CreateDirectory(System.Configuration.ConfigurationSettings.AppSettings["Temp"].Trim() + CurDate + "\\");
                            //}
                            if (!Directory.Exists(System.Configuration.ConfigurationSettings.AppSettings["Dest"].Trim() + CurDate + "\\"))
                            {
                                Directory.CreateDirectory(System.Configuration.ConfigurationSettings.AppSettings["Dest"].Trim() + CurDate + "\\");
                            }

                            DestFile = System.Configuration.ConfigurationSettings.AppSettings["Dest"].Trim() + CurDate + "\\" + string.Format("{0:00}", Hour) + ".flv";
                            if (!File.Exists(DestFile))
                            {
                                richTextBox2.Text += "News Hour : " + Hour + " Started Finding Wmv Files\n";
                                richTextBox2.SelectionStart = richTextBox2.Text.Length;
                                richTextBox2.ScrollToCaret();
                                Application.DoEvents();

                                foreach (FileInfo item in Dir.GetFiles())
                                {
                                    if (item.Name.StartsWith(string.Format("{0:00}", Hour)))
                                    {
                                        if (
                                            ConvertFnameToTime(Path.GetFileNameWithoutExtension(item.FullName)) >= ConvertFnameToTime(Path.GetFileNameWithoutExtension(StartTime))
                                            &&
                                           ConvertFnameToTime(Path.GetFileNameWithoutExtension(item.FullName)) <= ConvertFnameToTime(Path.GetFileNameWithoutExtension(StartTime2))
                                            )
                                        {
                                            File1 = item.FullName;
                                            richTextBox2.Text += "File1: " + File1 + "\n";
                                            richTextBox2.SelectionStart = richTextBox2.Text.Length;
                                            richTextBox2.ScrollToCaret();
                                            Application.DoEvents();
                                        }
                                    }
                                    if (item.Name.StartsWith(string.Format("{0:00}", Hour)))
                                    {
                                        if (ConvertFnameToTime(Path.GetFileNameWithoutExtension(item.FullName)) >= ConvertFnameToTime(Path.GetFileNameWithoutExtension(EndTime))
                                            &&
                                            ConvertFnameToTime(Path.GetFileNameWithoutExtension(item.FullName)) <= ConvertFnameToTime(Path.GetFileNameWithoutExtension(EndTime2)))
                                        {
                                            File2 = item.FullName;
                                            richTextBox2.Text += "File2: " + File2 + "\n";
                                            richTextBox2.SelectionStart = richTextBox2.Text.Length;
                                            richTextBox2.ScrollToCaret();
                                            Application.DoEvents();
                                        }
                                    }
                                }
                                if (File1.Length < 3)
                                {
                                    richTextBox2.Text += "File1: " + "Not Found" + "\n";
                                    richTextBox2.SelectionStart = richTextBox2.Text.Length;
                                    richTextBox2.ScrollToCaret();
                                    Application.DoEvents();
                                }
                                if (File2.Length < 3)
                                {
                                    richTextBox2.Text += "File2: " + "Not Found" + "\n";
                                    richTextBox2.SelectionStart = richTextBox2.Text.Length;
                                    richTextBox2.ScrollToCaret();
                                    Application.DoEvents();
                                }
                                if (File1.Length > 4 && File2.Length > 4)
                                {

                                    richTextBox2.Text += "News Hour : " + Hour + " Finished Finding Wmv Files\n";
                                    richTextBox2.SelectionStart = richTextBox2.Text.Length;
                                    richTextBox2.ScrollToCaret();
                                    Application.DoEvents();
                                    Convert(File1, System.Configuration.ConfigurationSettings.AppSettings["Temp"].Trim() + "1.mpg", System.Configuration.ConfigurationSettings.AppSettings["Temp"].Trim(), "mpg");
                                    Convert(File2, System.Configuration.ConfigurationSettings.AppSettings["Temp"].Trim() + "2.mpg", System.Configuration.ConfigurationSettings.AppSettings["Temp"].Trim(), "mpg");
                                    Concat(System.Configuration.ConfigurationSettings.AppSettings["Temp"].Trim() + "1.mpg", System.Configuration.ConfigurationSettings.AppSettings["Temp"].Trim() + "2.mpg", System.Configuration.ConfigurationSettings.AppSettings["Temp"].Trim() + "12.mpg", System.Configuration.ConfigurationSettings.AppSettings["Temp"].Trim());
                                    Convert(System.Configuration.ConfigurationSettings.AppSettings["Temp"].Trim() + "12.mpg", DestFile, System.Configuration.ConfigurationSettings.AppSettings["Temp"].Trim(), "flv");

                                    File.Delete(System.Configuration.ConfigurationSettings.AppSettings["Temp"].Trim() + "1.mpg");
                                    File.Delete(System.Configuration.ConfigurationSettings.AppSettings["Temp"].Trim() + "2.mpg");
                                    File.Delete(System.Configuration.ConfigurationSettings.AppSettings["Temp"].Trim() + "12.mpg");
                                    if (Directory.Exists(System.Configuration.ConfigurationSettings.AppSettings["WebDest"].Trim()))
                                    {
                                        richTextBox2.Text += "News Hour : " + Hour + " Start Copy To" + System.Configuration.ConfigurationSettings.AppSettings["WebDest"].Trim() + "\n";
                                        richTextBox2.SelectionStart = richTextBox2.Text.Length;
                                        richTextBox2.ScrollToCaret();
                                        Application.DoEvents();
                                        File.Copy(DestFile, System.Configuration.ConfigurationSettings.AppSettings["WebDest"].Trim() + Path.GetFileName(DestFile), true);
                                        richTextBox2.Text += "News Hour : " + Hour + " End Copy To" + System.Configuration.ConfigurationSettings.AppSettings["WebDest"].Trim() + "\n";
                                        label5.Text = System.Configuration.ConfigurationSettings.AppSettings["WebDest"].Trim().Replace("\\\\", "\\") + Path.GetFileName(DestFile);
                                        richTextBox2.SelectionStart = richTextBox2.Text.Length;
                                        richTextBox2.ScrollToCaret();
                                        Application.DoEvents();
                                    }
                                    else
                                    {
                                        Directory.CreateDirectory(System.Configuration.ConfigurationSettings.AppSettings["WebDest"].Trim());
                                        richTextBox2.Text += "News Hour : " + Hour + " Start Copy To" + System.Configuration.ConfigurationSettings.AppSettings["WebDest"].Trim() + "\n";
                                        richTextBox2.SelectionStart = richTextBox2.Text.Length;
                                        richTextBox2.ScrollToCaret();
                                        Application.DoEvents();
                                        File.Copy(DestFile, System.Configuration.ConfigurationSettings.AppSettings["WebDest"].Trim() + Path.GetFileName(DestFile), true);
                                        label5.Text = System.Configuration.ConfigurationSettings.AppSettings["WebDest"].Trim().Replace("\\\\", "\\") + Path.GetFileName(DestFile);
                                        richTextBox2.Text += "News Hour : " + Hour + " End Copy To" + System.Configuration.ConfigurationSettings.AppSettings["WebDest"].Trim() + "\n";
                                        richTextBox2.SelectionStart = richTextBox2.Text.Length;
                                        richTextBox2.ScrollToCaret();
                                        Application.DoEvents();
                                    }

                                }
                                else
                                {
                                    richTextBox2.Text += "News Hour : " + Hour + " It's Soon\n";
                                    richTextBox2.SelectionStart = richTextBox2.Text.Length;
                                    richTextBox2.ScrollToCaret();
                                    Application.DoEvents();
                                }
                            }
                            else
                            {
                                richTextBox2.Text += "File Exist: " + DestFile + "\n";
                                richTextBox2.SelectionStart = richTextBox2.Text.Length;
                                richTextBox2.ScrollToCaret();
                                Application.DoEvents();

                            }
                        //}
                        //else
                        //{
                        //    //if (!Directory.Exists(System.Configuration.ConfigurationSettings.AppSettings["Temp"].Trim() + CurDate + "\\"))
                        //    //{
                        //    //    Directory.CreateDirectory(System.Configuration.ConfigurationSettings.AppSettings["Temp"].Trim() + CurDate + "\\");
                        //    //}
                        //    if (!Directory.Exists(System.Configuration.ConfigurationSettings.AppSettings["Dest"].Trim() + CurDate + "\\"))
                        //    {
                        //        Directory.CreateDirectory(System.Configuration.ConfigurationSettings.AppSettings["Dest"].Trim() + CurDate + "\\");
                        //    }
                        //    DestFile = System.Configuration.ConfigurationSettings.AppSettings["Dest"].Trim() + CurDate + "\\" + string.Format("{0:00}", Hour) + ".flv";
                        //    if (!File.Exists(DestFile))
                        //    {
                        //        richTextBox2.Text += "News Hour : " + Hour + " Started Finding Wmv Files\n";
                        //        richTextBox2.SelectionStart = richTextBox2.Text.Length;
                        //        richTextBox2.ScrollToCaret();
                        //        Application.DoEvents();

                        //        foreach (FileInfo item in Dir.GetFiles())
                        //        {
                        //            if (item.Name.StartsWith(string.Format("{0:00}", Hour)))
                        //            {
                        //                if (ConvertFnameToTime(Path.GetFileNameWithoutExtension(item.FullName)) <= ConvertFnameToTime(Path.GetFileNameWithoutExtension(EndTime)))
                        //                {
                        //                    File2 = item.FullName;
                        //                    richTextBox2.Text += "File2: " + File2 + "\n";
                        //                    richTextBox2.SelectionStart = richTextBox2.Text.Length;
                        //                    richTextBox2.ScrollToCaret();
                        //                    Application.DoEvents();

                        //                }
                        //            }
                        //        }


                        //        string CurDateYesterday = DateTime.Now.AddHours(_TimeOffst).Year + "-" + string.Format("{0:00}", DateTime.Now.AddHours(_TimeOffst).Month) +
                        //             "-" + string.Format("{0:00}", DateTime.Now.AddHours(_TimeOffst).AddDays(-1).Day);
                        //        string StartTimeYesterday = "";
                        //        StartTimeYesterday = string.Format("{0:00}", 23) + System.Configuration.ConfigurationSettings.AppSettings["BeforNewsFileMinSec"].Trim();

                        //        DirectoryInfo DirYesterday = new DirectoryInfo(System.Configuration.ConfigurationSettings.AppSettings["Source"].Trim() + CurDateYesterday);

                        //        foreach (FileInfo item in DirYesterday.GetFiles())
                        //        {
                        //            if (item.Name.StartsWith(string.Format("{0:00}", 23)))
                        //            {
                        //                if (ConvertFnameToTime(Path.GetFileNameWithoutExtension(item.FullName)) >= ConvertFnameToTime(Path.GetFileNameWithoutExtension(StartTimeYesterday)))
                        //                {
                        //                    File1 = item.FullName;
                        //                    richTextBox2.Text += "File1: " + File1 + "\n";
                        //                    richTextBox2.SelectionStart = richTextBox2.Text.Length;
                        //                    richTextBox2.ScrollToCaret();
                        //                    Application.DoEvents();

                        //                }
                        //            }
                        //        }

                        //        if (File1.Length < 3)
                        //        {
                        //            richTextBox2.Text += "File1: " + "Not Found" + "\n";
                        //            richTextBox2.SelectionStart = richTextBox2.Text.Length;
                        //            richTextBox2.ScrollToCaret();
                        //            Application.DoEvents();
                        //        }
                        //        if (File2.Length < 3)
                        //        {
                        //            richTextBox2.Text += "File2: " + "Not Found" + "\n";
                        //            richTextBox2.SelectionStart = richTextBox2.Text.Length;
                        //            richTextBox2.ScrollToCaret();
                        //            Application.DoEvents();
                        //        }
                        //        if (File1.Length > 4 && File2.Length > 4)
                        //        {

                        //            richTextBox2.Text += "News Hour : " + Hour + " Finished Finding Wmv Files\n";
                        //            richTextBox2.SelectionStart = richTextBox2.Text.Length;
                        //            richTextBox2.ScrollToCaret();
                        //            Application.DoEvents();
                        //            Convert(File1, System.Configuration.ConfigurationSettings.AppSettings["Temp"].Trim() + "1.mpg", System.Configuration.ConfigurationSettings.AppSettings["Temp"].Trim(), "mpg");
                        //            Convert(File2, System.Configuration.ConfigurationSettings.AppSettings["Temp"].Trim() + "2.mpg", System.Configuration.ConfigurationSettings.AppSettings["Temp"].Trim(), "mpg");
                        //            Concat(System.Configuration.ConfigurationSettings.AppSettings["Temp"].Trim() + "1.mpg", System.Configuration.ConfigurationSettings.AppSettings["Temp"].Trim() + "2.mpg", System.Configuration.ConfigurationSettings.AppSettings["Temp"].Trim() + "12.mpg", System.Configuration.ConfigurationSettings.AppSettings["Temp"].Trim());
                        //            Convert(System.Configuration.ConfigurationSettings.AppSettings["Temp"].Trim() + "12.mpg", DestFile, System.Configuration.ConfigurationSettings.AppSettings["Temp"].Trim(), "flv");

                        //            File.Delete(System.Configuration.ConfigurationSettings.AppSettings["Temp"].Trim() + "1.mpg");
                        //            File.Delete(System.Configuration.ConfigurationSettings.AppSettings["Temp"].Trim() + "2.mpg");
                        //            File.Delete(System.Configuration.ConfigurationSettings.AppSettings["Temp"].Trim() + "12.mpg");
                        //            if (Directory.Exists(System.Configuration.ConfigurationSettings.AppSettings["WebDest"].Trim()))
                        //            {
                        //                richTextBox2.Text += "News Hour : " + Hour + " Start Copy To" + System.Configuration.ConfigurationSettings.AppSettings["WebDest"].Trim() + "\n";
                        //                richTextBox2.SelectionStart = richTextBox2.Text.Length;
                        //                richTextBox2.ScrollToCaret();
                        //                Application.DoEvents();
                        //                File.Copy(DestFile, System.Configuration.ConfigurationSettings.AppSettings["WebDest"].Trim() + Path.GetFileName(DestFile), true);
                        //                richTextBox2.Text += "News Hour : " + Hour + " End Copy To" + System.Configuration.ConfigurationSettings.AppSettings["WebDest"].Trim() + "\n";
                        //                label5.Text = System.Configuration.ConfigurationSettings.AppSettings["WebDest"].Trim().Replace("\\\\", "\\") + Path.GetFileName(DestFile);
                        //                richTextBox2.SelectionStart = richTextBox2.Text.Length;
                        //                richTextBox2.ScrollToCaret();
                        //                Application.DoEvents();
                        //            }
                        //            else
                        //            {
                        //                Directory.CreateDirectory(System.Configuration.ConfigurationSettings.AppSettings["WebDest"].Trim());
                        //                richTextBox2.Text += "News Hour : " + Hour + " Start Copy To" + System.Configuration.ConfigurationSettings.AppSettings["WebDest"].Trim() + "\n";
                        //                richTextBox2.SelectionStart = richTextBox2.Text.Length;
                        //                richTextBox2.ScrollToCaret();
                        //                Application.DoEvents();
                        //                File.Copy(DestFile, System.Configuration.ConfigurationSettings.AppSettings["WebDest"].Trim() + Path.GetFileName(DestFile), true);
                        //                label5.Text = System.Configuration.ConfigurationSettings.AppSettings["WebDest"].Trim().Replace("\\\\", "\\") + Path.GetFileName(DestFile);
                        //                richTextBox2.Text += "News Hour : " + Hour + " End Copy To" + System.Configuration.ConfigurationSettings.AppSettings["WebDest"].Trim() + "\n";
                        //                richTextBox2.SelectionStart = richTextBox2.Text.Length;
                        //                richTextBox2.ScrollToCaret();
                        //                Application.DoEvents();
                        //            }
                        //        }
                        //        else
                        //        {
                        //            richTextBox2.Text += "News Hour : " + Hour + " It's Soon\n";
                        //            richTextBox2.SelectionStart = richTextBox2.Text.Length;
                        //            richTextBox2.ScrollToCaret();
                        //            Application.DoEvents();
                        //        }
                        //    }
                        //    else
                        //    {
                        //        richTextBox2.Text += "File Exist: " + DestFile + "\n";
                        //        richTextBox2.SelectionStart = richTextBox2.Text.Length;
                        //        richTextBox2.ScrollToCaret();
                        //        Application.DoEvents();

                        //    }

                        //}

                    }
                }
                else
                {
                    richTextBox2.Text += "Directory Not created: " +CurDate+ "\n";
                    richTextBox2.SelectionStart = richTextBox2.Text.Length;
                    richTextBox2.ScrollToCaret();
                    Application.DoEvents();
                }
                timer1.Enabled = true;
            }
            catch (Exception Exp)
            {
                richTextBox2.Text += "MJ:" + Exp.Message + "\n";
                richTextBox2.SelectionStart = richTextBox2.Text.Length;
                richTextBox2.ScrollToCaret();
                Application.DoEvents();

                timer1.Enabled = true;
            }
          
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetDateToLbl();
        }
    }
}
