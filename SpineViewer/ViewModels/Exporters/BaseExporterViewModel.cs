using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using SFMLRenderer;
using Spine;
using Spine.Exporters;
using SpineViewer.Extensions;
using SpineViewer.Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SpineViewer.ViewModels.Exporters
{
    public abstract class BaseExporterViewModel: ObservableObject
    {
        protected static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        protected readonly MainWindowViewModel _vmMain;

        protected readonly ISFMLRenderer _renderer;

        public BaseExporterViewModel(MainWindowViewModel vmMain)
        {
            _vmMain = vmMain;
            _renderer = _vmMain.SFMLRenderer;
        }

        public uint ResolutionX => _vmMain.SFMLRendererViewModel.ResolutionX;

        public uint ResolutionY => _vmMain.SFMLRendererViewModel.ResolutionY;

        /// <summary>
        /// 是否导出成单个
        /// </summary>
        public bool ExportSingle { get => _exportSingle; set => SetProperty(ref _exportSingle, value); }
        protected bool _exportSingle = false;

        /// <summary>
        /// 输出文件夹, 如果指定则将输出内容输出到该文件夹, 如果没指定则输出到各自目录下, 导出单个时必须指定
        /// </summary>
        public string? OutputDir { get => _outputDir; set => SetProperty(ref _outputDir, value); }
        protected string? _outputDir;

        /// <summary>
        /// 背景颜色
        /// </summary>
        public Color BackgroundColor { get => _backgroundColor; set => SetProperty(ref _backgroundColor, value); }
        protected Color _backgroundColor = Color.FromArgb(0, 0, 0, 0);

        /// <summary>
        /// 四周边缘距离
        /// </summary>
        public uint Margin { get => _margin; set => SetProperty(ref _margin, value); }
        protected uint _margin = 0;

        /// <summary>
        /// 使用自动分辨率
        /// </summary>
        public bool AutoResolution { get => _autoResolution; set => SetProperty(ref _autoResolution, value); }
        protected bool _autoResolution = false;

        /// <summary>
        /// 最大分辨率
        /// </summary>
        public uint MaxResolution { get => _maxResolution; set => SetProperty(ref _maxResolution, value); }
        protected uint _maxResolution = 2048;

        /// <summary>
        /// 使用提供的包围盒设置自动分辨率
        /// </summary>
        private void SetAutoResolution(BaseExporter exporter, Rect bounds)
        {
            if (!_autoResolution) return;

            var resolution = bounds.Size.ToVector2u();
            if (resolution.X >= _maxResolution || resolution.Y >= _maxResolution)
            {
                // 缩小到最大像素限制
                var scale = Math.Min(_maxResolution / bounds.Width, _maxResolution / bounds.Height);
                resolution.X = (uint)(bounds.Width * scale);
                resolution.Y = (uint)(bounds.Height * scale);
            }
            exporter.Resolution = new(resolution.X + _margin * 2, resolution.Y + _margin * 2);

            var viewBounds = bounds.ToFloatRect().GetCanvasBounds(resolution, _margin);
            exporter.Size = new(viewBounds.Width, -viewBounds.Height);
            exporter.Center = viewBounds.Position + viewBounds.Size / 2;
            exporter.Rotation = 0;
        }

        /// <summary>
        /// 使用提供的模型设置导出器的自动分辨率和视区参数, 静态画面
        /// </summary>
        protected void SetAutoResolutionStatic(BaseExporter exporter, params SpineObject[] spines)
        {
            var bounds = spines[0].GetAnimationBounds();
            foreach (var sp in spines.Skip(1)) bounds.Union(sp.GetAnimationBounds());
            SetAutoResolution(exporter, bounds);
        }

        /// <summary>
        /// 使用提供的模型设置导出器的自动分辨率和视区参数, 动画画面
        /// </summary>
        protected void SetAutoResolutionAnimated(BaseExporter exporter, params SpineObject[] spines)
        {
            var bounds = spines[0].GetAnimationBounds();
            foreach (var sp in spines.Skip(1)) bounds.Union(sp.GetAnimationBounds());
            SetAutoResolution(exporter, bounds);
        }

        /// <summary>
        /// 检查参数是否合法并规范化参数值, 否则返回用户错误原因
        /// </summary>
        public virtual string? Validate()
        {
            if (!string.IsNullOrWhiteSpace(_outputDir) && File.Exists(_outputDir))
                return AppResource.Str_InvalidOutputDir;
            if (!string.IsNullOrWhiteSpace(_outputDir) && !Directory.Exists(_outputDir))
                return AppResource.Str_OutputDirNotFound;
            if (_exportSingle && string.IsNullOrWhiteSpace(_outputDir))
                return AppResource.Str_OutputDirRequired;
            if (_autoResolution && _maxResolution <= 0)
                return AppResource.Str_InvalidMaxResolution;
            OutputDir = string.IsNullOrWhiteSpace(_outputDir) ? null : Path.GetFullPath(_outputDir);
            return null;
        }

        public RelayCommand<IList?> Cmd_Export => _cmd_Export ??= new(Export_Execute, args => args is not null && args.Count > 0);
        private RelayCommand<IList?>? _cmd_Export;

        protected abstract void Export_Execute(IList? args);
    }
}
