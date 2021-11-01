using Microsoft.Extensions.Configuration;
using OZARKTool.Models;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OZARKTool
{
    public partial class frmMain : Form
    {

        private static ApplicationSettings appSettings;
        private OrmLiteConnectionFactory dbFactory;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();


        public frmMain()
        {
            appSettings = Program.Configuration.GetSection("ApplicationSettings").Get<ApplicationSettings>();

            dbFactory = new OrmLiteConnectionFactory(appSettings.ConnectionString, SqlServerDialect.Provider);


            InitializeComponent();

            dgSetA.DataError += DgSetA_DataError;
            dgSetB.DataError += DgSetB_DataError;

            txtTimer.Text = appSettings.DefaultTimer.ToString();
            txtThreshold.Text = appSettings.DefaultThreshold.ToString();
        }

        //private void DgSetB_DataError(object sender, DataGridViewDataErrorEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

        //private void DgSetA_DataError(object sender, DataGridViewDataErrorEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

        private void DgSetB_DataError(object sender, System.Windows.Forms.DataGridViewDataErrorEventArgs e)
        {
            //throw new System.NotImplementedException();
            e.Cancel = true;
        }

        private void DgSetA_DataError(object sender, System.Windows.Forms.DataGridViewDataErrorEventArgs e)
        {
            //throw new System.NotImplementedException();
            e.Cancel = true;
        }

        private void DoRun()
        {
            Log("Executing..");

            using (var conn = new SqlConnection(appSettings.ConnectionString))
            {
                var cmd = new SqlCommand("sp_BlitzCache", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                var ds = new DataSet();
                using (var da = new SqlDataAdapter(cmd))
                {
                    da.Fill(ds);
                    dgSetA.DataSource = ds.Tables[0];

                    int threshold = int.Parse(txtThreshold.Text);
                    Log($"Checking for threshold avg {threshold}");

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        //Log($"-- {row[16]}");
                        var avg = int.Parse(row[16].ToString());
                                                
                        if (avg >= threshold)
                        {
                            Log("ALERT");
                            Log("-----------------------------------");
                            Log("QUERY:");
                            Log(row[2].ToString());
                            Log(">>>>");
                            Log($"Avg Duration (ms) - {avg} milliseconds");
                        }
                    }
                    
                    dgSetB.DataSource = ds.Tables[1];
                    
                }
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            Log("Starting");

            btnStart.Enabled = false;
            btnStop.Enabled = true;

            timer1.Interval = int.Parse(txtTimer.Text) * 60 * 1000;
            Log($"Interval set to {timer1.Interval} milliseconds");
            timer1.Start();

            DoRun();
        }

        private void Log(string text)
        {
            Logger.Info(text);
            wl(text);
            Application.DoEvents();
        }

        private void w(string text)
        {
            rtbOutput.AppendText(text);
        }

        private void wl(string text)
        {
            w(text + Environment.NewLine);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            DoRun();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            Log("Stopping");

            timer1.Stop();

            btnStop.Enabled = false;
            btnStart.Enabled = true;

        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            rtbOutput.Clear();
        }
    }
}
