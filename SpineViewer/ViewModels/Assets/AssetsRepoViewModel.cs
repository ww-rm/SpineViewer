using CommunityToolkit.Mvvm.ComponentModel;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.ViewModels.Assets
{
    /// <summary>
    /// 资源库 ViewModel
    /// </summary>
    public abstract class AssetsRepoViewModel : ObservableObject, IExplorerOpenable
    {
        protected static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 资源库所处本地目录
        /// </summary>
        public abstract string LocalDirectory { get; }

        /// <summary>
        /// 默认资源库名称
        /// </summary>
        public abstract string DefaultName { get; }

        /// <summary>
        /// 资源库名称
        /// </summary>
        public string Name
        {
            get => string.IsNullOrWhiteSpace(_name) ? DefaultName : _name;
            set => SetProperty(ref _name, value);
        }
        private string _name = "";

        /// <summary>
        /// 该资源库下的所有模型资源列表
        /// </summary>
        public abstract IReadOnlyList<AssetsItemViewModel> Items { get; }

        /// <summary>
        /// 刷新该资源库下的模型资源列表 <see cref="Items"/>
        /// </summary>
        public abstract void RefreshItems();

        #region IExplorerOpenable

        string IExplorerOpenable.OpenInExplorerDirectory => LocalDirectory;

        #endregion
    }

    /// <summary>
    /// 资源库 ViewModel
    /// </summary>
    public abstract class AssetsRepoViewModel<TItem> : AssetsRepoViewModel where TItem : AssetsItemViewModel
    {
        public abstract override IReadOnlyList<TItem> Items { get; }
    }
}
