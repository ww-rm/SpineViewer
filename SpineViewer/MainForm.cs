using NLog;
using SpineViewer.Spine;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

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

        private void toolStripMenuItem_ExportPreview_Click(object sender, EventArgs e)
        {
            spineListView.ExportPreviews();
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
            var openDialog = new Dialogs.ConvertFileFormatDialog();
            if (openDialog.ShowDialog() != DialogResult.OK)
                return;

            var progressDialog = new Dialogs.ProgressDialog();
            progressDialog.DoWork += ConvertFileFormat_Work;
            progressDialog.RunWorkerAsync(openDialog);
            progressDialog.ShowDialog();
        }

        private void toolStripMenuItem_ManageResource_Click(object sender, EventArgs e)
        {
            
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

        private void ConvertFileFormat_Work(object? sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            var arguments = e.Argument as Dialogs.ConvertFileFormatDialog;
            var skelPaths = arguments.SkelPaths;
            var srcVersion = arguments.SourceVersion;
            var tgtVersion = arguments.TargetVersion;
            var jsonSource = arguments.JsonSource;
            var jsonTarget = arguments.JsonTarget;
            var newSuffix = jsonTarget ? ".json" : ".skel";

            if (jsonTarget == jsonSource)
            {
                if (tgtVersion == srcVersion)
                    return;
                else
                    newSuffix += $".{tgtVersion.ToString().ToLower()}"; // TODO: 仅转换版本的情况下考虑文件覆盖问题
            }

            int totalCount = skelPaths.Length;
            int success = 0;
            int error = 0;

            SkeletonConverter srcCvter = SkeletonConverter.New(srcVersion);
            SkeletonConverter tgtCvter = tgtVersion == srcVersion ? srcCvter : SkeletonConverter.New(tgtVersion);

            worker.ReportProgress(0, $"已处理 0/{totalCount}");
            for (int i = 0; i < totalCount; i++)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                var skelPath = skelPaths[i];
                var newPath = Path.ChangeExtension(skelPath, newSuffix);

                try
                {
                    var root = jsonSource ? srcCvter.ReadJson(skelPath) : srcCvter.ReadBinary(skelPath);
                    if (tgtVersion != srcVersion) root = srcCvter.ToVersion(root, tgtVersion);
                    if (jsonTarget) tgtCvter.WriteJson(root, newPath); else tgtCvter.WriteBinary(root, newPath);
                    success++;
                }
                catch (Exception ex)
                {
                    Program.Logger.Error(ex.ToString());
                    Program.Logger.Error("Failed to convert {}", skelPath);
                    error++;
                }

                worker.ReportProgress((int)((i + 1) * 100.0) / totalCount, $"已处理 {i + 1}/{totalCount}");
            }

            if (error > 0)
            {
                Program.Logger.Warn("Batch convert {} successfully, {} failed", success, error);
            }
            else
            {
                Program.Logger.Info("{} skel converted successfully", success);
            }
        }
    }
}
