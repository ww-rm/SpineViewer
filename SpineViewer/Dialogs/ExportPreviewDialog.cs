using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpineViewer.Dialogs
{
    public partial class ExportPreviewDialog: Form
    {
        /// <summary>
        /// 对话框结果
        /// </summary>
        public readonly ExportPreviewDialogResult Result = new();

        public ExportPreviewDialog()
        {
            InitializeComponent();
            propertyGrid.SelectedObject = Result;
        }

        private void button_SelectOutputDir_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.InitialDirectory = textBox_OutputDir.Text;
            if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
                return;
            textBox_OutputDir.Text = Path.GetFullPath(folderBrowserDialog.SelectedPath);
        }

        private void button_Ok_Click(object sender, EventArgs e)
        {
            var outputDir = textBox_OutputDir.Text;
            if (string.IsNullOrEmpty(outputDir))
            {
                Result.OutputDir = null;
            }
            else
            {
                if (File.Exists(outputDir))
                {
                    MessageBox.Info("输出文件夹无效");
                    return;
                }

                if (!Directory.Exists(outputDir))
                {
                    if (MessageBox.Quest($"文件夹 {outputDir} 不存在，是否创建？") != DialogResult.OK)
                        return;

                    try
                    {
                        Directory.CreateDirectory(outputDir);
                    }
                    catch (Exception ex)
                    {
                        Program.Logger.Error(ex.ToString());
                        MessageBox.Error(ex.ToString(), "文件夹创建失败");
                        return;
                    }
                }
                Result.OutputDir = Path.GetFullPath(outputDir);
            }

            DialogResult = DialogResult.OK;
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }

    public class ExportPreviewDialogResult
    {
        /// <summary>
        /// 输出路径
        /// </summary>
        [Browsable(false)]
        public string? OutputDir { get; set; } = null;

        /// <summary>
        /// 预览图格式
        /// </summary>
        [TypeConverter(typeof(ImageFormatConverter))]
        [Category("导出参数"), DisplayName("预览图格式")]
        public ImageFormat ImageFormat
        {
            get => imageFormat;
            set
            {
                if (value == ImageFormat.MemoryBmp) value = ImageFormat.Bmp;
                imageFormat = value;
            }
        }
        private ImageFormat imageFormat = ImageFormat.Png;

        /// <summary>
        /// 预览图分辨率
        /// </summary>
        [TypeConverter(typeof(SizeConverter))]
        [Category("导出参数"), DisplayName("分辨率")]
        public Size Resolution
        {
            get => resolution;
            set
            {
                if (value.Width <= 0) value.Width = 128;
                if (value.Height <= 0) value.Height = 128;
                resolution = value;
            }
        }
        private Size resolution = new(512, 512);

        /// <summary>
        /// 四周填充像素值
        /// </summary>
        [TypeConverter(typeof(PaddingConverter))]
        [Category("导出参数"), DisplayName("四周填充像素值")]
        public Padding Padding
        {
            get => padding;
            set
            {
                if (value.Left <= 0) value.Left = 10;
                if (value.Right <= 0) value.Right = 10;
                if (value.Top <= 0) value.Top = 10;
                if (value.Bottom <= 0) value.Bottom = 10;
                padding = value;
            }
        }
        private Padding padding = new(1);

        /// <summary>
        /// DPI
        /// </summary>
        [TypeConverter(typeof(SizeFConverter))]
        [Category("导出参数"), DisplayName("DPI")]
        public SizeF DPI
        {
            get => dpi;
            set
            {
                if (value.Width <= 0) value.Width = 144;
                if (value.Height <= 0) value.Height = 144;
                dpi = value;
            }
        }
        private SizeF dpi = new(144, 144);
    }

}
