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
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            InitializeLogConfiguration();

            // 在此处将导出菜单需要的类绑定起来
            toolStripMenuItem_ExportFrame.Tag = ExportType.Frame;
            toolStripMenuItem_ExportFrameSequence.Tag = ExportType.FrameSequence;
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

            lock (spineListView.Spines)
            {
                if (spineListView.Spines.Count <= 0)
                {
                    MessageBox.Info("请至少打开一个骨骼文件");
                    return;
                }
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

        //IEnumerable<IVideoFrame> testExport(int fps)
        //{
        //    var duration = 2f;
        //    var resolution = spinePreviewer.Resolution;
        //    var delta = 1f / fps;
        //    var frameCount = 1 + (int)(duration / delta); // 零帧开始导出

        //    var spinesReverse = spineListView.Spines.Reverse();

        //    // 重置动画时间
        //    foreach (var spine in spinesReverse)
        //        spine.CurrentAnimation = spine.CurrentAnimation;

        //    // 逐帧导出
        //    var success = 0;
        //    for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
        //    {
        //        using var tex = new SFML.Graphics.RenderTexture((uint)resolution.Width, (uint)resolution.Height);
        //        tex.SetView(spinePreviewer.View);
        //        tex.Clear(SFML.Graphics.Color.Transparent);

        //        foreach (var spine in spinesReverse)
        //        {
        //            tex.Draw(spine);
        //            spine.Update(delta);
        //        }

        //        tex.Display();
        //        Debug.WriteLine($"ThreadID: {Environment.CurrentManagedThreadId}");
        //        var frame = tex.Texture.CopyToFrame();
        //        tex.Dispose();
        //        yield return frame;

        //        success++;
        //    }

        //    Program.Logger.Info("Exporting done: {}/{}", success, frameCount);
        //}

        private void toolStripMenuItem_ManageResource_Click(object sender, EventArgs e)
        {
            //spinePreviewer.StopPreview();

            //lock (spineListView.Spines)
            //{
            //    //var fps = 24;
            //    ////foreach (var i in testExport(fps))
            //    ////    _ = i;
            //    ////var t = testExport(fps).ToArray();
            //    ////var a = testExport(fps).GetEnumerator();
            //    ////while (a.MoveNext());
            //    //var videoFramesSource = new RawVideoPipeSource(testExport(fps)) { FrameRate = fps };
            //    //var outputPath = @"C:\Users\ljh\Desktop\test\a.mov";
            //    //var task = FFMpegArguments
            //    //    .FromPipeInput(videoFramesSource)
            //    //    .OutputToFile(outputPath, true
            //    //    , options => options
            //    //    //.WithCustomArgument("-vf \"split[s0][s1];[s0]palettegen=reserve_transparent=1[p];[s1][p]paletteuse=alpha_threshold=128\""))
            //    //    .WithCustomArgument("-c:v prores_ks -profile:v 4444 -pix_fmt yuva444p10le"))
            //    //    .ProcessAsynchronously();
            //    //task.Wait();
            //}

            //spinePreviewer.StartPreview();
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

        private void propertyGrid_PropertyValueChanged(object sender, PropertyValueChangedEventArgs e) => (sender as PropertyGrid)?.Refresh();

        private void spinePreviewer_MouseUp(object sender, MouseEventArgs e) 
        { 
            propertyGrid_Spine.Refresh(); 
        }

        private void Export_Work(object? sender, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker)sender;
            var exporter = (Exporter.Exporter)e.Argument;
            spinePreviewer.StopRender();
            lock (spineListView.Spines) { exporter.Export(spineListView.Spines.ToArray(), (BackgroundWorker)sender); }
            e.Cancel = worker.CancellationPending;
            spinePreviewer.StartRender();
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
                        if (Spine.Spine.GetVersion(skelPath) is Spine.Version detectedSrcVersion)
                            srcCvter = SkeletonConverter.New(detectedSrcVersion);
                        else
                            throw new InvalidDataException($"Auto version detection failed for {skelPath}, try to use a specific version");
                    }
                    var root = srcCvter.Read(skelPath);
                    root = srcCvter.ToVersion(root, tgtVersion);
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
