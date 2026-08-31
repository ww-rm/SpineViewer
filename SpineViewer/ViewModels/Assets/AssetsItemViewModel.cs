using CommunityToolkit.Mvvm.ComponentModel;
using NLog;
using SpineViewer.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SpineViewer.ViewModels.Assets
{
    /// <summary>
    /// 资源库模型 ViewModel
    /// </summary>
    public abstract class AssetsItemViewModel : ObservableObject, IExplorerOpenable
    {
        /// <summary>
        /// 缩略图文件名格式字符串, 需要一个参数
        /// </summary>
        public static string PreviewFileNameFormat => ".{0}.preview.webp";

        protected static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly AssetsRepoViewModel _vmRepo;

        public AssetsItemViewModel(AssetsRepoViewModel vmRepo, string relativePath)
        {
            _vmRepo = vmRepo;
            RelativePath = relativePath;

            LocalFullPath = Path.Combine(vmRepo.LocalDirectory, relativePath);
            FileName = Path.GetFileName(relativePath);
            LocalDirectory = Path.GetDirectoryName(LocalFullPath) ?? "";
        }

        /// <summary>
        /// 相对资源库的相对路径
        /// </summary>
        public string RelativePath { get; }

        /// <summary>
        /// 本地存储完整路径
        /// </summary
        public string LocalFullPath { get; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// 文件所处本地目录
        /// </summary>
        public string LocalDirectory { get; }

        /// <summary>
        /// 预览图路径
        /// </summary>
        public string PreviewFilePath { get => _previewFilePath ??= Path.Combine(LocalDirectory, string.Format(PreviewFileNameFormat, FileName)); }
        private string? _previewFilePath;

        /// <summary>
        /// 预览图
        /// </summary>
        public ImageSource? PreviewImage
        {
            get
            {
                try
                {
                    return WpfExtension.LoadWebpWithAlpha(PreviewFilePath);
                }
                catch (FileNotFoundException)
                {
                    return null;
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex.ToString());
                    _logger.Warn("Failed to load preview image for {0}, {1}", LocalFullPath, ex.Message);
                    return null;
                }
            }
        }

        #region IExplorerOpenable

        string IExplorerOpenable.OpenInExplorerDirectory => LocalDirectory;

        #endregion
    }
}
