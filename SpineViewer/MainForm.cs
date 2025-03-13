using NLog;
using SpineViewer.Spine;
using System.ComponentModel;
using System.Diagnostics;

namespace SpineViewer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            InitializeLogConfiguration();
        }

        /// <summary>
        /// 初始化窗口日志器
        /// </summary>
        private void InitializeLogConfiguration()
        {
            // 窗口日志
            var rtbTarget = new NLog.Windows.Forms.RichTextBoxTarget
            {
                Name = "rtbTarget",
                TargetForm = this,
                TargetRichTextBox = rtbLog,
                AutoScroll = true,
                MaxLines = 3000,
                SupportLinks = true,
                Layout = "[${level:format=OneLetter}]${date:format=yyyy-MM-dd HH\\:mm\\:ss} - ${message}"
            };

            rtbTarget.WordColoringRules.Add(new("[D]", "Gray", "Empty", FontStyle.Bold));
            rtbTarget.WordColoringRules.Add(new("[I]", "DimGray", "Empty", FontStyle.Bold));
            rtbTarget.WordColoringRules.Add(new("[W]", "DarkOrange", "Empty", FontStyle.Bold));
            rtbTarget.WordColoringRules.Add(new("[E]", "Red", "Empty", FontStyle.Bold));
            rtbTarget.WordColoringRules.Add(new("[F]", "DarkRed", "Empty", FontStyle.Bold));

            LogManager.Configuration.AddTarget(rtbTarget);
            LogManager.Configuration.AddRule(LogLevel.Debug, LogLevel.Fatal, rtbTarget);
            LogManager.ReconfigExistingLoggers();
        }

        private void ExportPng_Work(object? sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            var arguments = e.Argument as Dialogs.ExportPngDialog;
            var outputDir = arguments.OutputDir;
            var duration = arguments.Duration;
            var fps = arguments.Fps;
            var timestamp = DateTime.Now.ToString("yyMMddHHmmss");

            var resolution = spinePreviewer.Resolution;
            var tex = new SFML.Graphics.RenderTexture((uint)resolution.Width, (uint)resolution.Height);
            tex.SetView(spinePreviewer.View);
            var delta = 1f / fps;
            var frameCount = 1 + (int)(duration / delta); // 零帧开始导出

            spinePreviewer.StopPreview();

            lock (spineListView.Spines)
            {
                var spinesReverse = spineListView.Spines.Reverse();

                // 重置动画时间
                foreach (var spine in spinesReverse)
                    spine.CurrentAnimation = spine.CurrentAnimation;

                Program.Logger.Info(
                    "Begin exporting png frames to output dir {}, duration: {}, fps: {}, totally {} spines",
                    [outputDir, duration, fps, spinesReverse.Count()]
                );

                // 逐帧导出
                var success = 0;
                worker.ReportProgress(0, $"已处理 0/{frameCount}");
                for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
                {
                    if (worker.CancellationPending)
                        break;

                    tex.Clear(SFML.Graphics.Color.Transparent);

                    foreach (var spine in spinesReverse)
                    {
                        tex.Draw(spine);
                        spine.Update(delta);
                    }

                    tex.Display();
                    using (var img = tex.Texture.CopyToImage())
                    {
                        img.SaveToFile(Path.Combine(outputDir, $"{timestamp}_{fps}_{frameIndex:d6}.png"));
                    }

                    success++;
                    worker.ReportProgress((int)((frameIndex + 1) * 100.0) / frameCount, $"已处理 {frameIndex + 1}/{frameCount}");
                }

                Program.Logger.Info("Exporting done: {}/{}", success, frameCount);
            }

            spinePreviewer.StartPreview();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            spinePreviewer.StartPreview();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            spinePreviewer.StopPreview();
        }

        private void toolStripMenuItem_Open_Click(object sender, EventArgs e)
        {
            spineListView.Add();
        }

        private void toolStripMenuItem_BatchOpen_Click(object sender, EventArgs e)
        {
            spineListView.BatchAdd();
        }

        private void toolStripMenuItem_Export_Click(object sender, EventArgs e)
        {
            lock (spineListView.Spines)
            {
                if (spineListView.Spines.Count <= 0)
                {
                    MessageBox.Show("请至少打开一个骨骼文件", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }

            var exportDialog = new Dialogs.ExportPngDialog();
            if (exportDialog.ShowDialog() != DialogResult.OK)
                return;

            var progressDialog = new Dialogs.ProgressDialog();
            progressDialog.DoWork += ExportPng_Work;
            progressDialog.RunWorkerAsync(exportDialog);
            progressDialog.ShowDialog();
        }

        private void toolStripMenuItem_Exit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void toolStripMenuItem_ResetAnimation_Click(object sender, EventArgs e)
        {
            lock (spineListView.Spines)
            {
                foreach (var spine in spineListView.Spines)
                    spine.CurrentAnimation = spine.CurrentAnimation;
            }
        }

        private void toolStripMenuItem_ConvertFileFormat_Click(object sender, EventArgs e)
        {

            return;
        }

        private void toolStripMenuItem_About_Click(object sender, EventArgs e)
        {
            (new Dialogs.AboutDialog()).ShowDialog();
        }

        private void toolStripMenuItem_Diagnostics_Click(object sender, EventArgs e)
        {
            (new Dialogs.DiagnosticsDialog()).ShowDialog();
        }

        private void splitContainer_SplitterMoved(object sender, SplitterEventArgs e) { ActiveControl = null; }

        private void splitContainer_MouseUp(object sender, MouseEventArgs e) { ActiveControl = null; }

        private void propertyGrid_PropertyValueChanged(object sender, PropertyValueChangedEventArgs e) { (sender as PropertyGrid)?.Refresh(); }

        private void spinePreviewer_MouseUp(object sender, MouseEventArgs e) { propertyGrid_Spine.Refresh(); }

    }
}
