using FFMpegCore.Pipes;
using FFMpegCore;
using NLog;
using SFML.System;
using SpineViewer.Spine;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using FFMpegCore.Enums;
using SpineViewer.Exporter;

namespace SpineViewer
{
    internal partial class MainForm : Form
    {
        private Logger logger = LogManager.GetCurrentClassLogger();

        public MainForm()
        {
            InitializeComponent();
            InitializeLogConfiguration();

            // 在此处将导出菜单需要的类绑定起来
            toolStripMenuItem_ExportFrame.Tag = ExportType.Frame;
            toolStripMenuItem_ExportFrameSequence.Tag = ExportType.FrameSequence;
            toolStripMenuItem_ExportGif.Tag = ExportType.GIF;
            toolStripMenuItem_ExportMkv.Tag = ExportType.MKV;
            toolStripMenuItem_ExportMp4.Tag = ExportType.MP4;
            toolStripMenuItem_ExportMov.Tag = ExportType.MOV;
            toolStripMenuItem_ExportWebm.Tag = ExportType.WebM;

            // 执行一些初始化工作
            try
            {
                Spine.Shader.Init();
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                logger.Error("Failed to load fragment shader");
                MessageBox.Warn("Fragment shader 加载失败，预乘Alpha通道属性失效");
            }

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
            spinePreviewer.StartRender();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            spinePreviewer.StopRender();
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
            ExportType type = (ExportType)((ToolStripMenuItem)sender).Tag;

            if (type == ExportType.Frame && spinePreviewer.IsUpdating)
            {
                if (MessageBox.Quest("画面仍在更新，建议手动暂停画面后导出固定的一帧，是否继续？") != DialogResult.OK)
                    return;
            }

            var exportArgs = ExportArgs.New(type, spinePreviewer.Resolution, spinePreviewer.GetView(), spinePreviewer.RenderSelectedOnly);
            var exportDialog = new Dialogs.ExportDialog() { ExportArgs = exportArgs };
            if (exportDialog.ShowDialog() != DialogResult.OK)
                return;

            var exporter = Exporter.Exporter.New(type, exportArgs);

            var progressDialog = new Dialogs.ProgressDialog();
            progressDialog.DoWork += Export_Work;
            progressDialog.RunWorkerAsync(exporter);
            progressDialog.ShowDialog();
        }

        private void toolStripMenuItem_Exit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void toolStripMenuItem_ConvertFileFormat_Click(object sender, EventArgs e)
        {
            var openDialog = new Dialogs.ConvertFileFormatDialog();
            if (openDialog.ShowDialog() != DialogResult.OK)
                return;

            var progressDialog = new Dialogs.ProgressDialog();
            progressDialog.DoWork += ConvertFileFormat_Work;
            progressDialog.RunWorkerAsync(openDialog.Result);
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

        private void splitContainer_SplitterMoved(object sender, SplitterEventArgs e) => ActiveControl = null;

        private void splitContainer_MouseUp(object sender, MouseEventArgs e) => ActiveControl = null;

        private void propertyGrid_PropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
        {
            // 用来解决对面板某些值修改之后, 其他被联动修改的值不会实时刷新的问题
            (sender as PropertyGrid)?.Refresh();
        }

        private void Export_Work(object? sender, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker)sender;
            var exporter = (Exporter.Exporter)e.Argument;
            Invoke(() => TaskbarManager.SetProgressState(Handle, TBPFLAG.TBPF_INDETERMINATE));
            spinePreviewer.StopRender();
            lock (spineListView.Spines) { exporter.Export(spineListView.Spines.Where(sp => !sp.IsHidden).ToArray(), (BackgroundWorker)sender); }
            e.Cancel = worker.CancellationPending;
            spinePreviewer.StartRender();
            Invoke(() => TaskbarManager.SetProgressState(Handle, TBPFLAG.TBPF_NOPROGRESS));
        }

        private void ConvertFileFormat_Work(object? sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            var arguments = e.Argument as Dialogs.ConvertFileFormatDialogResult;
            var skelPaths = arguments.SkelPaths;
            var srcVersion = arguments.SourceVersion;
            var tgtVersion = arguments.TargetVersion;
            var jsonTarget = arguments.JsonTarget;
            var newSuffix = jsonTarget ? ".json" : ".skel";

            int totalCount = skelPaths.Length;
            int success = 0;
            int error = 0;

            SkeletonConverter srcCvter = srcVersion != Spine.Version.Auto ? SkeletonConverter.New(srcVersion) : null;
            SkeletonConverter tgtCvter = SkeletonConverter.New(tgtVersion);

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
                    if (srcVersion == Spine.Version.Auto)
                    {
                        try
                        {
                            srcCvter = SkeletonConverter.New(Spine.Spine.GetVersion(skelPath));
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidDataException($"Auto version detection failed for {skelPath}, try to use a specific version", ex);
                        }
                    }
                    var root = srcCvter.Read(skelPath);
                    root = srcCvter.ToVersion(root, tgtVersion);
                    if (jsonTarget) tgtCvter.WriteJson(root, newPath); else tgtCvter.WriteBinary(root, newPath);
                    success++;
                }
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());
                    logger.Error("Failed to convert {}", skelPath);
                    error++;
                }

                worker.ReportProgress((int)((i + 1) * 100.0) / totalCount, $"已处理 {i + 1}/{totalCount}");
            }

            if (error > 0)
            {
                logger.Warn("Batch convert {} successfully, {} failed", success, error);
            }
            else
            {
                logger.Info("{} skel converted successfully", success);
            }
        }

        //private void spinePreviewer_KeyDown(object sender, KeyEventArgs e)
        //{
        //    switch (e.KeyCode)
        //    {
        //        case Keys.Space:
        //            if ((ModifierKeys & Keys.Alt) != 0)
        //                spinePreviewer.ClickStopButton();
        //            else
        //                spinePreviewer.ClickStartButton();
        //            break;
        //        case Keys.Right:
        //            if ((ModifierKeys & Keys.Alt) != 0)
        //                spinePreviewer.ClickForwardFastButton();
        //            else
        //                spinePreviewer.ClickForwardStepButton();
        //            break;
        //        case Keys.Left:
        //            if ((ModifierKeys & Keys.Alt) != 0)
        //                spinePreviewer.ClickRestartButton();
        //            break;
        //    }
        //}
    }
}
