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
            // TODO: 改成统一导出调用
            lock (spineListView.Spines)
            {
                if (spineListView.Spines.Count <= 0)
                {
                    MessageBox.Info("请至少打开一个骨骼文件");
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
            lock (spineListView.Spines)
            {
                if (spineListView.Spines.Count <= 0)
                {
                    MessageBox.Info("请至少打开一个骨骼文件");
                    return;
                }
            }

            var saveDialog = new Dialogs.ExportPreviewDialog();
            if (saveDialog.ShowDialog() != DialogResult.OK)
                return;

            var progressDialog = new Dialogs.ProgressDialog();
            progressDialog.DoWork += ExportPreview_Work;
            progressDialog.RunWorkerAsync(saveDialog.Result);
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

        private void ExportPng_Work(object? sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            var arguments = e.Argument as Dialogs.ExportPngDialog;
            var outputDir = arguments.OutputDir;
            var duration = arguments.Duration;
            var fps = arguments.Fps;
            var timestamp = DateTime.Now.ToString("yyMMddHHmmss");

            var frameArgs = spinePreviewer.GetFrameArgs();
            var renderSelectedOnly = spinePreviewer.RenderSelectedOnly;

            var resolution = frameArgs.Resolution;
            var tex = new SFML.Graphics.RenderTexture((uint)resolution.Width, (uint)resolution.Height);
            tex.SetView(frameArgs.View);
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
                        if (renderSelectedOnly && !spine.IsSelected)
                            continue;

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

        private void ExportPreview_Work(object? sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            var arguments = e.Argument as Dialogs.ExportPreviewDialogResult;

            var outputDir = arguments.OutputDir;
            var imageFormat = arguments.ImageFormat;
            var resolution = arguments.Resolution;
            var padding = arguments.Padding;
            var dpi = arguments.DPI;
            var nameSuffix = arguments.NameSuffix;
            var renderSelectedOnly = spinePreviewer.RenderSelectedOnly;

            var tex = new SFML.Graphics.RenderTexture((uint)resolution.Width, (uint)resolution.Height);

            int success = 0;
            int error = 0;
            spinePreviewer.StopPreview();
            lock (spineListView.Spines)
            {
                var spines = spineListView.Spines;
                int totalCount = spines.Count;
                worker.ReportProgress(0, $"已处理 0/{totalCount}");
                for (int i = 0; i < totalCount; i++)
                {
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        break;
                    }

                    var spine = spines[i];
                    if (renderSelectedOnly && !spine.IsSelected)
                        continue;

                    var filename = $"{spine.Name}{nameSuffix}{imageFormat.GetSuffix()}";
                    var savePath = outputDir is null ? Path.Combine(spine.AssetsDir, filename) : Path.Combine(outputDir, filename);

                    var tmp = spine.CurrentAnimation;
                    spine.CurrentAnimation = Spine.Spine.EMPTY_ANIMATION;
                    tex.SetView(spine.GetInitView(resolution, padding)); 
                    tex.Clear(SFML.Graphics.Color.Transparent);
                    tex.Draw(spine);
                    tex.Display();
                    spine.CurrentAnimation = tmp;

                    try
                    {
                        using (var img = new Bitmap(tex.Texture.CopyToBitmap()))
                        {
                            img.SetResolution(dpi.Width, dpi.Height);
                            img.Save(savePath, imageFormat);
                        }
                        success++;
                    }
                    catch (Exception ex)
                    {
                        Program.Logger.Error(ex.ToString());
                        Program.Logger.Error("Failed to save preview {}", spine.SkelPath);
                        error++;
                    }

                    worker.ReportProgress((int)((i + 1) * 100.0) / totalCount, $"已处理 {i + 1}/{totalCount}");
                }
            }
            spinePreviewer.StartPreview();

            if (error > 0)
            {
                Program.Logger.Warn("Preview save {} successfully, {} failed", success, error);
            }
            else
            {
                Program.Logger.Info("{} preview saved successfully", success);
            }

            Program.LogCurrentMemoryUsage();
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
