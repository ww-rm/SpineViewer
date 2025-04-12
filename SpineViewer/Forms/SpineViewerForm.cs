﻿using NLog;
using SpineViewer.Spine;
using System.ComponentModel;
using System.Diagnostics;
using SpineViewer.Natives;
using SpineViewer.Utils;
using SpineViewer.Spine.SpineExporter;

namespace SpineViewer
{
    internal partial class SpineViewerForm : Form
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly Dictionary<string, Exporter> exporterCache = [];

        public SpineViewerForm()
        {
            InitializeComponent();
            InitializeLogConfiguration();

            // 执行一些初始化工作
            try
            {
                SFMLShader.Init();
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                logger.Error("Failed to load fragment shader");
                MessagePopup.Warn("Fragment shader 加载失败，预乘Alpha通道属性失效");
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

        #region private void toolStripMenuItem_ExportXXX_Click(object sender, EventArgs e)

        private void toolStripMenuItem_ExportFrame_Click(object sender, EventArgs e)
        {
            if (spinePreviewer.IsUpdating && MessagePopup.Quest("画面仍在更新，建议手动暂停画面后导出固定的一帧，是否继续？") != DialogResult.OK)
                return;

            var k = nameof(toolStripMenuItem_ExportFrame);
            if (!exporterCache.ContainsKey(k)) exporterCache[k] = new FrameExporter();

            var exporter = exporterCache[k];
            exporter.Resolution = spinePreviewer.Resolution;
            exporter.View = spinePreviewer.GetView();
            exporter.RenderSelectedOnly = spinePreviewer.RenderSelectedOnly;

            var exportDialog = new Dialogs.ExportDialog(new FrameExporterProperty((FrameExporter)exporter));
            if (exportDialog.ShowDialog() != DialogResult.OK)
                return;

            var progressDialog = new Dialogs.ProgressDialog();
            progressDialog.DoWork += Export_Work;
            progressDialog.RunWorkerAsync(exporter);
            progressDialog.ShowDialog();
        }

        private void toolStripMenuItem_ExportFrameSequence_Click(object sender, EventArgs e)
        {
            var k = nameof(toolStripMenuItem_ExportFrameSequence);
            if (!exporterCache.ContainsKey(k)) exporterCache[k] = new FrameSequenceExporter();

            var exporter = exporterCache[k];
            exporter.Resolution = spinePreviewer.Resolution;
            exporter.View = spinePreviewer.GetView();
            exporter.RenderSelectedOnly = spinePreviewer.RenderSelectedOnly;

            var exportDialog = new Dialogs.ExportDialog(new FrameSequenceExporterProperty((FrameSequenceExporter)exporter));
            if (exportDialog.ShowDialog() != DialogResult.OK)
                return;

            var progressDialog = new Dialogs.ProgressDialog();
            progressDialog.DoWork += Export_Work;
            progressDialog.RunWorkerAsync(exporter);
            progressDialog.ShowDialog();
        }

        private void toolStripMenuItem_ExportGif_Click(object sender, EventArgs e)
        {
            var k = nameof(toolStripMenuItem_ExportGif);
            if (!exporterCache.ContainsKey(k)) exporterCache[k] = new GifExporter();

            var exporter = exporterCache[k];
            exporter.Resolution = spinePreviewer.Resolution;
            exporter.View = spinePreviewer.GetView();
            exporter.RenderSelectedOnly = spinePreviewer.RenderSelectedOnly;

            var exportDialog = new Dialogs.ExportDialog(new GifExporterProperty((GifExporter)exporter));
            if (exportDialog.ShowDialog() != DialogResult.OK)
                return;

            var progressDialog = new Dialogs.ProgressDialog();
            progressDialog.DoWork += Export_Work;
            progressDialog.RunWorkerAsync(exporter);
            progressDialog.ShowDialog();
        }

        private void toolStripMenuItem_ExportWebp_Click(object sender, EventArgs e)
        {
            var k = nameof(toolStripMenuItem_ExportWebp);
            if (!exporterCache.ContainsKey(k)) exporterCache[k] = new WebpExporter();

            var exporter = exporterCache[k];
            exporter.Resolution = spinePreviewer.Resolution;
            exporter.View = spinePreviewer.GetView();
            exporter.RenderSelectedOnly = spinePreviewer.RenderSelectedOnly;

            var exportDialog = new Dialogs.ExportDialog(new WebpExporterProperty((WebpExporter)exporter));
            if (exportDialog.ShowDialog() != DialogResult.OK)
                return;

            var progressDialog = new Dialogs.ProgressDialog();
            progressDialog.DoWork += Export_Work;
            progressDialog.RunWorkerAsync(exporter);
            progressDialog.ShowDialog();
        }

        private void toolStripMenuItem_ExportAvif_Click(object sender, EventArgs e)
        {
            var k = nameof(toolStripMenuItem_ExportAvif);
            if (!exporterCache.ContainsKey(k)) exporterCache[k] = new AvifExporter();

            var exporter = exporterCache[k];
            exporter.Resolution = spinePreviewer.Resolution;
            exporter.View = spinePreviewer.GetView();
            exporter.RenderSelectedOnly = spinePreviewer.RenderSelectedOnly;

            var exportDialog = new Dialogs.ExportDialog(new AvifExporterProperty((AvifExporter)exporter));
            if (exportDialog.ShowDialog() != DialogResult.OK)
                return;

            var progressDialog = new Dialogs.ProgressDialog();
            progressDialog.DoWork += Export_Work;
            progressDialog.RunWorkerAsync(exporter);
            progressDialog.ShowDialog();
        }

        private void toolStripMenuItem_ExportMp4_Click(object sender, EventArgs e)
        {
            var k = nameof(toolStripMenuItem_ExportMp4);
            if (!exporterCache.ContainsKey(k)) exporterCache[k] = new Mp4Exporter();

            var exporter = exporterCache[k];
            exporter.Resolution = spinePreviewer.Resolution;
            exporter.View = spinePreviewer.GetView();
            exporter.RenderSelectedOnly = spinePreviewer.RenderSelectedOnly;

            var exportDialog = new Dialogs.ExportDialog(new Mp4ExporterProperty((Mp4Exporter)exporter));
            if (exportDialog.ShowDialog() != DialogResult.OK)
                return;

            var progressDialog = new Dialogs.ProgressDialog();
            progressDialog.DoWork += Export_Work;
            progressDialog.RunWorkerAsync(exporter);
            progressDialog.ShowDialog();
        }

        private void toolStripMenuItem_ExportWebm_Click(object sender, EventArgs e)
        {
            var k = nameof(toolStripMenuItem_ExportWebm);
            if (!exporterCache.ContainsKey(k)) exporterCache[k] = new WebmExporter();

            var exporter = exporterCache[k];
            exporter.Resolution = spinePreviewer.Resolution;
            exporter.View = spinePreviewer.GetView();
            exporter.RenderSelectedOnly = spinePreviewer.RenderSelectedOnly;

            var exportDialog = new Dialogs.ExportDialog(new WebmExporterProperty((WebmExporter)exporter));
            if (exportDialog.ShowDialog() != DialogResult.OK)
                return;

            var progressDialog = new Dialogs.ProgressDialog();
            progressDialog.DoWork += Export_Work;
            progressDialog.RunWorkerAsync(exporter);
            progressDialog.ShowDialog();
        }

        private void toolStripMenuItem_ExportMkv_Click(object sender, EventArgs e)
        {
            var k = nameof(toolStripMenuItem_ExportMkv);
            if (!exporterCache.ContainsKey(k)) exporterCache[k] = new MkvExporter();

            var exporter = exporterCache[k];
            exporter.Resolution = spinePreviewer.Resolution;
            exporter.View = spinePreviewer.GetView();
            exporter.RenderSelectedOnly = spinePreviewer.RenderSelectedOnly;

            var exportDialog = new Dialogs.ExportDialog(new MkvExporterProperty((MkvExporter)exporter));
            if (exportDialog.ShowDialog() != DialogResult.OK)
                return;

            var progressDialog = new Dialogs.ProgressDialog();
            progressDialog.DoWork += Export_Work;
            progressDialog.RunWorkerAsync(exporter);
            progressDialog.ShowDialog();
        }

        private void toolStripMenuItem_ExportMov_Click(object sender, EventArgs e)
        {
            var k = nameof(toolStripMenuItem_ExportMov);
            if (!exporterCache.ContainsKey(k)) exporterCache[k] = new MovExporter();

            var exporter = exporterCache[k];
            exporter.Resolution = spinePreviewer.Resolution;
            exporter.View = spinePreviewer.GetView();
            exporter.RenderSelectedOnly = spinePreviewer.RenderSelectedOnly;

            var exportDialog = new Dialogs.ExportDialog(new MovExporterProperty((MovExporter)exporter));
            if (exportDialog.ShowDialog() != DialogResult.OK)
                return;

            var progressDialog = new Dialogs.ProgressDialog();
            progressDialog.DoWork += Export_Work;
            progressDialog.RunWorkerAsync(exporter);
            progressDialog.ShowDialog();
        }

        private void toolStripMenuItem_ExportCustom_Click(object sender, EventArgs e)
        {
            var k = nameof(toolStripMenuItem_ExportCustom);
            if (!exporterCache.ContainsKey(k)) exporterCache[k] = new CustomExporter();

            var exporter = exporterCache[k];
            exporter.Resolution = spinePreviewer.Resolution;
            exporter.View = spinePreviewer.GetView();
            exporter.RenderSelectedOnly = spinePreviewer.RenderSelectedOnly;

            var exportDialog = new Dialogs.ExportDialog(new CustomExporterProperty((CustomExporter)exporter));
            if (exportDialog.ShowDialog() != DialogResult.OK)
                return;

            var progressDialog = new Dialogs.ProgressDialog();
            progressDialog.DoWork += Export_Work;
            progressDialog.RunWorkerAsync(exporter);
            progressDialog.ShowDialog();
        }

        #endregion

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
            using var dialog = new Dialogs.AboutDialog();
            dialog.ShowDialog();
        }

        private void toolStripMenuItem_Diagnostics_Click(object sender, EventArgs e)
        {
            using var dialog = new Dialogs.DiagnosticsDialog();
            dialog.ShowDialog();
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
            var exporter = (Exporter)e.Argument;
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

            SkeletonConverter srcCvter = srcVersion != SpineVersion.Auto ? SkeletonConverter.New(srcVersion) : null;
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
                    if (srcVersion == SpineVersion.Auto)
                    {
                        try
                        {
                            srcCvter = SkeletonConverter.New(SpineUtils.GetVersion(skelPath));
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

        //private System.Windows.Forms.Timer timer = new();
        //private PetForm pet = new PetForm();
        //private IntPtr screenDC;
        //private IntPtr memDC;
        //private void _Test()
        //{
        //    screenDC = Win32.GetDC(IntPtr.Zero);
        //    memDC = Win32.CreateCompatibleDC(screenDC);
        //    pet.Show();
        //    timer.Tick += Timer_Tick;
        //    timer.Enabled = true;
        //    timer.Interval = 50;
        //    timer.Start();
        //}

        //private void Timer_Tick(object? sender, EventArgs e)
        //{
        //    using var tex = new SFML.Graphics.RenderTexture((uint)pet.Width, (uint)pet.Height);
        //    var v = spinePreviewer.GetView();
        //    tex.SetView(v);
        //    tex.Clear(new SFML.Graphics.Color(0, 0, 0, 0));
        //    lock (spineListView.Spines)
        //    {
        //        foreach (var sp in spineListView.Spines)
        //            tex.Draw(sp);
        //    }
        //    tex.Display();
        //    using var frame = new SFMLImageVideoFrame(tex.Texture.CopyToImage());
        //    using var bitmap = frame.CopyToBitmap();

        //    var newBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
        //    var oldBitmap = Win32.SelectObject(memDC, newBitmap);

        //    Win32.SIZE size = new Win32.SIZE { cx = pet.Width, cy = pet.Height };
        //    Win32.POINT srcPos = new Win32.POINT { x = 0, y = 0 };
        //    Win32.BLENDFUNCTION blend = new Win32.BLENDFUNCTION { BlendOp = 0, BlendFlags = 0, SourceConstantAlpha = 255, AlphaFormat = Win32.AC_SRC_ALPHA };

        //    Win32.UpdateLayeredWindow(pet.Handle, screenDC, IntPtr.Zero, ref size, memDC, ref srcPos, 0, ref blend, Win32.ULW_ALPHA);

        //    Win32.SelectObject(memDC, oldBitmap);
        //    Win32.DeleteObject(newBitmap);
        //}

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
