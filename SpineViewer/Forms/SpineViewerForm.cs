using NLog;
using SpineViewer.Spine;
using System.ComponentModel;
using System.Diagnostics;
using SpineViewer.Natives;
using SpineViewer.Utils;
using SpineViewer.Spine.SpineExporter;
using System.Configuration;
using SpineViewer.Utils.Localize;

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
				MessagePopup.Warn(Properties.Resources.failLoadingFragmentShader, Properties.Resources.msgBoxWarning);
			}

#if DEBUG
			toolStripMenuItem_Debug.Visible = true;
#endif
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
			spinePreviewPanel.StartRender();
			string cultureName = ConfigurationManager.AppSettings["localize"];
			switch (cultureName)
			{
				case "zh-CN":
					ToolStripMenuItem_Chinese.Enabled = false;
					break;
				case "en-US":
					ToolStripMenuItem_English.Enabled = false;
					break;
				default:
					ToolStripMenuItem_Chinese.Enabled = false;
					break;
			}
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			spinePreviewPanel.StopRender();
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
			if (spinePreviewPanel.IsUpdating && MessagePopup.Quest(Properties.Resources.isUpdatingAndManuallyExportFrame, Properties.Resources.msgBoxQuest) != DialogResult.OK)
				return;

			var k = nameof(toolStripMenuItem_ExportFrame);
			if (!exporterCache.ContainsKey(k)) exporterCache[k] = new FrameExporter();

			var exporter = exporterCache[k];
			using var view = spinePreviewPanel.GetView();
			exporter.Resolution = spinePreviewPanel.Resolution;
			exporter.PreviewerView = view;
			exporter.RenderSelectedOnly = spinePreviewPanel.RenderSelectedOnly;

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
			using var view = spinePreviewPanel.GetView();
			exporter.Resolution = spinePreviewPanel.Resolution;
			exporter.PreviewerView = view;
			exporter.RenderSelectedOnly = spinePreviewPanel.RenderSelectedOnly;

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
			using var view = spinePreviewPanel.GetView();
			exporter.Resolution = spinePreviewPanel.Resolution;
			exporter.PreviewerView = view;
			exporter.RenderSelectedOnly = spinePreviewPanel.RenderSelectedOnly;

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
			using var view = spinePreviewPanel.GetView();
			exporter.Resolution = spinePreviewPanel.Resolution;
			exporter.PreviewerView = view;
			exporter.RenderSelectedOnly = spinePreviewPanel.RenderSelectedOnly;

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
			using var view = spinePreviewPanel.GetView();
			exporter.Resolution = spinePreviewPanel.Resolution;
			exporter.PreviewerView = view;
			exporter.RenderSelectedOnly = spinePreviewPanel.RenderSelectedOnly;

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
			using var view = spinePreviewPanel.GetView();
			exporter.Resolution = spinePreviewPanel.Resolution;
			exporter.PreviewerView = view;
			exporter.RenderSelectedOnly = spinePreviewPanel.RenderSelectedOnly;

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
			using var view = spinePreviewPanel.GetView();
			exporter.Resolution = spinePreviewPanel.Resolution;
			exporter.PreviewerView = view;
			exporter.RenderSelectedOnly = spinePreviewPanel.RenderSelectedOnly;

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
			using var view = spinePreviewPanel.GetView();
			exporter.Resolution = spinePreviewPanel.Resolution;
			exporter.PreviewerView = view;
			exporter.RenderSelectedOnly = spinePreviewPanel.RenderSelectedOnly;

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
			using var view = spinePreviewPanel.GetView();
			exporter.Resolution = spinePreviewPanel.Resolution;
			exporter.PreviewerView = view;
			exporter.RenderSelectedOnly = spinePreviewPanel.RenderSelectedOnly;

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
			using var view = spinePreviewPanel.GetView();
			exporter.Resolution = spinePreviewPanel.Resolution;
			exporter.PreviewerView = view;
			exporter.RenderSelectedOnly = spinePreviewPanel.RenderSelectedOnly;

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

		private void Export_Work(object? sender, DoWorkEventArgs e)
		{
			var worker = (BackgroundWorker)sender;
			var exporter = (Exporter)e.Argument;
			Invoke(() => TaskbarManager.SetProgressState(Handle, TBPFLAG.TBPF_INDETERMINATE));
			spinePreviewPanel.StopRender();
			lock (spineListView.Spines) { exporter.Export(spineListView.Spines.Where(sp => !sp.IsHidden).ToArray(), (BackgroundWorker)sender); }
			e.Cancel = worker.CancellationPending;
			spinePreviewPanel.StartRender();
			Invoke(() => TaskbarManager.SetProgressState(Handle, TBPFLAG.TBPF_NOPROGRESS));
		}

		private void ConvertFileFormat_Work(object? sender, DoWorkEventArgs e)
		{
			var worker = sender as BackgroundWorker;
			var args = e.Argument as Dialogs.ConvertFileFormatDialogResult;
			var newSuffix = args.JsonTarget ? ".json" : ".skel";

			int totalCount = args.SkelPaths.Length;
			int success = 0;
			int error = 0;

			SkeletonConverter srcCvter = args.SourceVersion != SpineVersion.Auto ? SkeletonConverter.New(args.SourceVersion) : null;
			SkeletonConverter tgtCvter = SkeletonConverter.New(args.TargetVersion);

			worker.ReportProgress(0, $"{Properties.Resources.process} 0/{totalCount}");
			for (int i = 0; i < totalCount; i++)
			{
				if (worker.CancellationPending)
				{
					e.Cancel = true;
					break;
				}

				var skelPath = args.SkelPaths[i];
				var newPath = Path.ChangeExtension(skelPath, newSuffix);
				if (args.OutputDir is string outputDir) newPath = Path.Combine(outputDir, Path.GetFileName(newPath));

				try
				{
					if (args.SourceVersion == SpineVersion.Auto)
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
					root = srcCvter.ToVersion(root, args.TargetVersion);
					if (args.JsonTarget) tgtCvter.WriteJson(root, newPath);
					else tgtCvter.WriteBinary(root, newPath);
					success++;
				}
				catch (Exception ex)
				{
					logger.Error(ex.ToString());
					logger.Error("Failed to convert {}", skelPath);
					error++;
				}

				worker.ReportProgress((int)((i + 1) * 100.0) / totalCount, $"{Properties.Resources.process} {i + 1}/{totalCount}");
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

		private void toolStripMenuItem_DesktopProjection_Click(object sender, EventArgs e)
		{
			toolStripMenuItem_DesktopProjection.Checked = !toolStripMenuItem_DesktopProjection.Checked;
			spinePreviewPanel.EnableDesktopProjection = toolStripMenuItem_DesktopProjection.Checked;
		}

		private void toolStripMenuItem_Debug_Click(object sender, EventArgs e)
		{
#if DEBUG
			//var cvt = SkeletonConverter.New(SpineVersion.V38);
			//var root = cvt.ReadBinary(@"D:\ACGN\AzurLane_Export\AzurLane_Dynamic\docs\aerhangeersike\aerhangeersike_3\aerhangeersike_3 - 副本.skel");
			//cvt.WriteJson(root, @"D:\ACGN\AzurLane_Export\AzurLane_Dynamic\docs\aerhangeersike\aerhangeersike_3\aerhangeersike_3.json");

			//root = cvt.ReadJson(@"D:\ACGN\AzurLane_Export\AzurLane_Dynamic\docs\aerhangeersike\aerhangeersike_3\aerhangeersike_3.json");
			//cvt.WriteBinary(root, @"D:\ACGN\AzurLane_Export\AzurLane_Dynamic\docs\aerhangeersike\aerhangeersike_3\aerhangeersike_3.skel");
			//var sp = SpineObject.New(SpineVersion.V38, @"D:\ACGN\AzurLane_Export\AzurLane_Dynamic\docs\aerhangeersike\aerhangeersike_3\aerhangeersike_3.skel");

			//var cvt = SkeletonConverter.New(SpineVersion.V38);
			//var root = cvt.ReadJson(@"D:\ACGN\G\GirlsCreation\standing_spine\st4020069\st4020069.json");
			//cvt.WriteBinary(root, @"D:\ACGN\G\GirlsCreation\standing_spine\st4020069\st4020069.skel");
			//var sp = SpineObject.New(SpineVersion.V38, @"D:\ACGN\G\GirlsCreation\standing_spine\st4020069\st4020069.skel");
			//_Test();
#endif
		}

		private void ToolStripMenuItem_English_Click(object sender, EventArgs e)
		{
			ChangeLanguage("en-US");
		}

		private void ToolStripMenuItem_Chinese_Click(object sender, EventArgs e)
		{
			ChangeLanguage("zh-CN");
		}

		private void ChangeLanguage(string localize)
		{
			DialogResult result = MessageBox.Show(
				Properties.Resources.restartPrompt,
				Properties.Resources.restartTitle,
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
			{
				LocalizeConfiguration.UpdateLocalizeSetting(localize);
				LocalizeConfiguration.SetCulture();
				Application.Restart();
				Environment.Exit(0);   
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
