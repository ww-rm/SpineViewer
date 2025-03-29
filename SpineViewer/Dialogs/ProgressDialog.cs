using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpineViewer.Dialogs
{
    public partial class ProgressDialog : Form
    {
        private Logger logger = LogManager.GetCurrentClassLogger();

        public ProgressDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// BackgroundWorker.DoWork 接口暴露
        /// </summary>
        [Category("自定义"), Description("BackgroundWorker 的 DoWork 事件")]
        public event DoWorkEventHandler? DoWork
        {
            add => backgroundWorker.DoWork += value;
            remove => backgroundWorker.DoWork -= value;
        }

        /// <summary>
        /// 启动后台执行
        /// </summary>
        public void RunWorkerAsync() => backgroundWorker.RunWorkerAsync();

        /// <summary>
        /// 使用给定参数启动后台执行
        /// </summary>
        public void RunWorkerAsync(object? argument) => backgroundWorker.RunWorkerAsync(argument);

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            label_Tip.Text = e.UserState as string;
            progressBar.Value = e.ProgressPercentage;
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                logger.Error(e.Error.ToString());
                MessageBox.Error(e.Error.ToString(), "执行出错");
                DialogResult = DialogResult.Abort;
            }
            else if (e.Cancelled)
            {
                DialogResult = DialogResult.Cancel;
            }
            else
            {
                DialogResult= DialogResult.OK;
            }
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            backgroundWorker.CancelAsync();
            button_Cancel.Enabled = false;
        }
    }
}
