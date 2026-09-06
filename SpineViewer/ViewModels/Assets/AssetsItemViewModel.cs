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
        private const string PreviewFileNameFormat = ".{0}.preview.webp";

        protected static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly AssetsRepoViewModel _vmRepo;
        protected readonly string _relativePath;
        protected readonly string _fileName;

        public AssetsItemViewModel(AssetsRepoViewModel vmRepo, string relativePath)
        {
            _vmRepo = vmRepo;
            _relativePath = relativePath;
            _fileName = Path.GetFileName(_relativePath);
        }

        /// <summary>
        /// 相对资源库的相对路径
        /// </summary>
        public string RelativePath => _relativePath;

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName => _fileName;

        /// <summary>
        /// 本地存储完整路径
        /// </summary
        public string LocalFullPath => Path.Combine(_vmRepo.LocalDirectory, _relativePath);

        /// <summary>
        /// 文件所处本地目录
        /// </summary>
        public string LocalDirectory => Path.GetDirectoryName(LocalFullPath) ?? "";

        /// <summary>
        /// 预览图路径
        /// </summary>
        public string PreviewFilePath => Path.Combine(LocalDirectory, string.Format(PreviewFileNameFormat, _fileName));

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
